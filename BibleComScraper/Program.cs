using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Transactions;
using BibleComScraper.Classes;
using BibleComScraper.Enums;
using BibleComScraper.Services;

namespace BibleComScraper
{
    internal static class Program
    {
        private static readonly HttpService Http = new HttpService();
        private static BibleService _bible;

        static void Main()
        {

            Console.WriteLine("bible.com scraper\n-=≡=-=≡=-=≡=-=≡=-\n\n");

            // get a language code
            string langPage = string.Empty;
            while (string.IsNullOrWhiteSpace(langPage))
            {
                Console.Write("Enter iso language code [eng]: ");
                Console.ForegroundColor = ConsoleColor.White;
                var languageCode = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(languageCode)) languageCode = "eng";
                languageCode = languageCode.ToLower();

                try
                {
                    langPage = Http.GetPage($"https://www.bible.com/languages/{languageCode}");
                }
                catch
                {
                    Console.WriteLine("Language is unsupported or incorrect, or something else went terribly wrong.");
                }
            }

            // get a translation
            Console.WriteLine("Possible translations: ");

            Regex translationsReg =
                new Regex(
                    "<a role=button target=_self class=\"db pb2 lh-copy yv-green link\" href=(?<url>\\/versions\\/(?<slug>(?<code>\\d*)-.*?))>(?<name>.*?)<\\/a>");
            MatchCollection results = translationsReg.Matches(langPage);
            foreach (Match m in results)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(m.Groups["code"].ToString());
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"\t: {m.Groups["name"]}");
            }

            string startingPageContent = string.Empty;
            string translationCode = string.Empty;
            while (string.IsNullOrWhiteSpace(startingPageContent))
            {
                Console.Write("Enter translation code [114]: ");
                Console.ForegroundColor = ConsoleColor.White;
                translationCode = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(translationCode)) translationCode = "114";
                translationCode = translationCode.ToLower();

                try
                {
                    string startingUrl = "https://www.bible.com" + results
                        .FirstOrDefault(f => f.Groups["code"].ToString() == translationCode)
                        ?.Groups["url"];

                    startingPageContent = Http.GetPage(startingUrl);

                }
                catch
                {
                    Console.WriteLine(
                        "Translation code is unsupported or incorrect, or something else went terribly wrong.");
                }
            }

            // from the startingPageContent, we need to get the link to the first verse of the Bible.
            string firstChapterUrl = "https://www.bible.com" + Regex
                .Match(startingPageContent,
                    "<a class=\"db pb3 link yv-green lh-copy\" role=button href=(?<firstChapterUrl>\\/bible\\/.*?)>Read Version")
                .Groups["firstChapterUrl"].Value;

            // we need to collate the Bible Metadata
            string bibleName = results
                .FirstOrDefault(f => f.Groups["code"].ToString() == translationCode)
                ?.Groups["name"].ToString();
            string bibleCode = Regex.Match(firstChapterUrl, ".*\\.(.+?)$").Groups[1].Value;

            _bible = new BibleService(bibleCode, bibleName, uint.Parse(translationCode));

            try
            {
                if (_bible.DatabaseExists)
                {
                    Console.Write("Bible DB already exists.  RESET or RESUME [resume]: ");
                    if (Console.ReadLine()?.ToUpper().Trim() == "RESET")
                    {
                        _bible.DeleteDatabase();
                    }
                }
            }
            catch
            {
                Console.WriteLine($"can't delete existing file {bibleCode}.bible.db.  Quitting...");
                return;
            }

            _bible.InitializeDatabase();

            // pull a job from the queue and process.  Then wait a random number of seconds.
            bool done = false;
            while (!done)
            {

                Console.WriteLine($"Jobs remaining: {_bible.JobsRemaining}");

                Job job = _bible.GetRandomJob();

                Console.WriteLine($"Got job {job.Id} which is an {job.Type} job");
                switch (job.Type)
                {
                    case JobTypes.EnumerateBooks:
                        EnumerateBooks(job);
                        break;
                    case JobTypes.EnumerateChapters:
                        EnumerateChapters(job);
                        break;
                    case JobTypes.EnumerateVerses:
                        EnumerateVerses(job);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown job type");
                }

                _bible.DeleteJob(job);
                done = (_bible.JobsRemaining == 0);

                Console.WriteLine("Job finished");
                //Console.ReadKey();
            }

            Console.WriteLine("All Done");
            Console.ReadKey();

        }


        private static void EnumerateBooks(Job job)
        {
            uint i = 0;
            string bookPage = Http.GetPage(job.Url);
            Regex booksRegex = new Regex("\"human\":\"(?<name>.*?)\",\"usfm\":\"(?<abbr>.*?)\"");
            MatchCollection booksMatches = booksRegex.Matches(bookPage);
            foreach (Match m in booksMatches)
            {
                Book newBook = new Book(i, m.Groups["name"].Value, m.Groups["abbr"].Value);
                _bible.AddBook(newBook);
                _bible.CreateJob(JobTypes.EnumerateChapters,
                    $"https://www.bible.com/json/bible/books/{_bible.TranslationCode}/{newBook.Code}/chapters");

                i++;
            }
        }

        private static void EnumerateChapters(Job job)
        {

            // extract the book code from the Url
            string bookCode = Regex.Match(job.Url, "\\d+\\/(?<code>.*?)\\/").Groups["code"].Value;

            uint i = 0;
            string chaptersPage = Http.GetPage(job.Url);
            Regex chapterRegex = new Regex("\"human\":\"(?<name>.*?)\",\"usfm\":\"(?<url>.*?)\"");
            MatchCollection chaptersMatches = chapterRegex.Matches(chaptersPage);
            foreach (Match m in chaptersMatches)
            {
                string chapterUrl = m.Groups["url"].Value;
                Chapter newChapter = new Chapter(bookCode, i, m.Groups["name"].Value);
                _bible.AddChapter(newChapter);
                _bible.CreateJob(JobTypes.EnumerateVerses,
                    $"https://www.bible.com/bible/{_bible.TranslationCode}/{chapterUrl}.{_bible.BibleCode}");
                i++;
            }
        }

        private static void EnumerateVerses(Job job)
        {

            // extract the book code from the Url
            string bookCode = Regex.Match(job.Url, "\\d+\\/(?<book>[A-Z0-9]+)\\.(?<chapter>.*)\\.").Groups["book"]
                .Value;
            // extract the chapter number from the url
            string chapterCode = Regex.Match(job.Url, "\\d+\\/(?<book>[A-Z0-9]+)\\.(?<chapter>.*)\\.").Groups["chapter"]
                .Value;

            // retrieve the Page
            string chapterPage = Http.GetPage(job.Url);
            Regex divRegex = new Regex("<div class=\\\"(d|p|s|q1|q2)\\\">.*?<\\/div>");
            MatchCollection divMatches = divRegex.Matches(chapterPage);

            // setup for the iteration
            uint currentVerse = 0;

            // there are 176 verses in Psalm 199, which is the longest chapter in the Bible.
            Verse[] verseList = InitializeArray<Verse>(176);

            foreach (Match m in divMatches)
            {
                // if the match is a class="s", then it's a section header for the _next_ verse
                // in the match is a class="p", then it's a verse text. May contain a span "qs" that's a verse footer (selah)
                // if the match is a class="d", then it's a verse prefix for the _next_ verse.
                // if the match is a class="q1, q2", then it's a verse text, where there's multiline-formatting. If it's a section with one
                //   these, we'll assume q1 starts a paragraph, and that paragraph ends a) with the next q1, or b) the next p. So when
                //   a p or q1 starts, set the previous verse EndsParagraph to true, (unless we're on the first verse).

                if (m.Value.Contains("class=\"s\""))
                {

                    string sectionHeader = m.Value
                        .Replace("&#8217;", "'")
                        .Replace("&#8220;", "“")
                        .Replace("&#8221;", "”")
                        .Replace("&#8212;", "—");


                    // strip out any heading tags
                    while (Regex.IsMatch(sectionHeader, "<span class=\"heading\">.*?<\\/span>"))
                    {
                        Match spanM = Regex.Match(sectionHeader, "<span class=\\\"heading\\\">(?<text>.*?)<\\/span>");
                        sectionHeader = sectionHeader.Replace(spanM.Value, spanM.Groups["text"].Value);
                    }

                    // strip out div tags
                    sectionHeader = Regex.Replace(sectionHeader, "<\\/div>", "");
                    sectionHeader = Regex.Replace(sectionHeader, "<div class=\\\"s\\\">", "");

                    // strip label from notes span from string
                    sectionHeader = sectionHeader.Replace("<span class=\"label\">#</span>", "");


                    // strip any body spans
                    sectionHeader = Regex.Replace(sectionHeader, "<span class=\\\" {0,1}body\\\">.*?<\\/span>", "");


                    // strip out notes spans
                    sectionHeader = Regex.Replace(sectionHeader, "<span class=\"note.*?<\\/span>", "");


                    // fix small caps.  Right now they'll look like this: 
                    // <span class="sc">Lord</span>
                    while (Regex.IsMatch(sectionHeader, "class=\"sc\""))
                    {
                        Match spanM = Regex.Match(sectionHeader, "<span class=\\\"sc\\\">(?<text>.*?)<\\/span>");
                        string innerText = spanM.Groups["text"].Value
                            .Replace("a", "ᴀ")
                            .Replace("b", "ʙ")
                            .Replace("c", "ᴄ")
                            .Replace("d", "ᴅ")
                            .Replace("e", "ᴇ")
                            .Replace("f", "ꜰ")
                            .Replace("g", "ɢ")
                            .Replace("h", "ʜ")
                            .Replace("i", "ɪ")
                            .Replace("j", "ᴊ")
                            .Replace("k", "ᴋ")
                            .Replace("l", "ʟ")
                            .Replace("m", "ᴍ")
                            .Replace("n", "ɴ")
                            .Replace("o", "ᴏ")
                            .Replace("p", "ᴘ")
                            .Replace("q", "ǫ")
                            .Replace("r", "ʀ")
                            .Replace("s", "s")
                            .Replace("t", "ᴛ")
                            .Replace("u", "ᴜ")
                            .Replace("v", "ᴠ")
                            .Replace("w", "ᴡ")
                            .Replace("x", "x")
                            .Replace("y", "ʏ")
                            .Replace("z", "ᴢ");
                        sectionHeader = sectionHeader.Replace(spanM.Value, innerText);
                    }

                    sectionHeader = sectionHeader.Replace(" &#160; ", " ").Trim();

                    if (!string.IsNullOrWhiteSpace(sectionHeader))
                    {
                        verseList[currentVerse].SectionTitle = sectionHeader;
                    }




                }
                else if (m.Value.Contains("class=\"d\""))
                {
                    string versePrefix = m.Value
                        .Replace("&#8217;", "'")
                        .Replace("&#8220;", "“")
                        .Replace("&#8221;", "”")
                        .Replace("&#8212;", "—");

                    // strip label from notes span from string
                    versePrefix = versePrefix.Replace("<span class=\"label\">#</span>", "");

                    // strip any body spans
                    versePrefix = Regex.Replace(versePrefix, "<span class=\\\" {0,1}body\\\">.*?<\\/span>", "");

                    // strip out notes spans
                    versePrefix = Regex.Replace(versePrefix, "<span class=\"note.*?<\\/span>", "");

                    // strip out div tags
                    versePrefix = Regex.Replace(versePrefix, "<\\/div>", "");
                    versePrefix = Regex.Replace(versePrefix, "<div class=\\\"d\\\">", "");

                    // strip out any content tags
                    while (Regex.IsMatch(versePrefix, "<span class=\"content\">.*?<\\/span>"))
                    {
                        Match spanM = Regex.Match(versePrefix, "<span class=\\\"content\\\">(?<content>.*?)<\\/span>");
                        versePrefix = versePrefix.Replace(spanM.Value, spanM.Groups["content"].Value);
                    }

                    // replace any italics tags
                    versePrefix = versePrefix.Replace("<span class=\"it\">", "<i>");
                    versePrefix = versePrefix.Replace("</span>", "</i>");

                    // strip out any double spaces
                    while (versePrefix.Contains("  "))
                    {
                        versePrefix = versePrefix.Replace("  ", " ");
                    }

                    // fix small caps.  Right now they'll look like this: 
                    // <span class="sc">Lord</i>
                    while (Regex.IsMatch(versePrefix, "class=\"sc\""))
                    {
                        Match spanM = Regex.Match(versePrefix, "<span class=\\\"sc\\\">(?<text>.*?)<\\/i>");
                        string innerText = spanM.Groups["text"].Value
                            .Replace("a", "ᴀ")
                            .Replace("b", "ʙ")
                            .Replace("c", "ᴄ")
                            .Replace("d", "ᴅ")
                            .Replace("e", "ᴇ")
                            .Replace("f", "ꜰ")
                            .Replace("g", "ɢ")
                            .Replace("h", "ʜ")
                            .Replace("i", "ɪ")
                            .Replace("j", "ᴊ")
                            .Replace("k", "ᴋ")
                            .Replace("l", "ʟ")
                            .Replace("m", "ᴍ")
                            .Replace("n", "ɴ")
                            .Replace("o", "ᴏ")
                            .Replace("p", "ᴘ")
                            .Replace("q", "ǫ")
                            .Replace("r", "ʀ")
                            .Replace("s", "s")
                            .Replace("t", "ᴛ")
                            .Replace("u", "ᴜ")
                            .Replace("v", "ᴠ")
                            .Replace("w", "ᴡ")
                            .Replace("x", "x")
                            .Replace("y", "ʏ")
                            .Replace("z", "ᴢ");
                        versePrefix = versePrefix.Replace(spanM.Value, innerText);
                    }

                    // strip out any doubled-up italics tags
                    versePrefix = versePrefix.Replace("</i> <i>", " ");

                    // finally, trim the strings
                    versePrefix = versePrefix.Trim();

                    verseList[currentVerse].VersePrefix = versePrefix;

                }
                else if (m.Value.Contains("class=\"p\""))
                {

                    // let's handle the verse suffix's first
                    if (m.Value.Contains("class=\"qs\""))
                    {
                        // it's a verse suffix
                        Regex verseSuffixRegex = new Regex("verse v(?<verse>\\d{0,3})\".*tent\\\">(?<text>.*?)<");
                        Match m2 = verseSuffixRegex.Match(m.Value);
                        verseList[uint.Parse(m2.Groups["verse"].Value) - 1].VerseSuffix = m2.Groups["text"].Value;
                    }
                    else
                    {

                        // it's a regular verse paragraph.

                        // let's start by scrubbing the easy stuff
                        string paragraph = m.Value
                            .Replace("&#8217;", "’")
                            .Replace("&#8216;", "‘")
                            .Replace("&#8220;", "“")
                            .Replace("&#8221;", "”")
                            .Replace("&#8212;", "—")
                            .Replace("&#8211;", "–")
                            .Replace("&#274;", "E")                     /// fix for bible.org making Enosh into Ē'nosh
                            .Replace("&#180;", "")                      /// fix for bible.org making Enosh into Ē'nosh
                            .Replace("&#233;", "é")
                                          ;

                        // strip out div tags
                        paragraph = Regex.Replace(paragraph, "<\\/div>", "");
                        paragraph = Regex.Replace(paragraph, "<div class=\\\"p\\\">", "");

                        // strip label from notes span from string
                        paragraph = paragraph.Replace("<span class=\"label\">#</span>", "");


                        // strip any body spans
                        paragraph = Regex.Replace(paragraph, "<span class=\\\" {0,1}body\\\">.*?<\\/span>", "");


                        // strip out notes spans
                        paragraph = Regex.Replace(paragraph, "<span class=\"note.*?<\\/span>", "");

                        // strip out the verse labels
                        paragraph = Regex.Replace(paragraph, "<span class=\"label\">\\d+<\\/span>", "");

                        // strip out any content tags
                        while (Regex.IsMatch(paragraph, "<span class=\"content\">.*?<\\/span>"))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"content\\\">(?<content>.*?)<\\/span>");
                            paragraph = paragraph.Replace(spanM.Value, spanM.Groups["content"].Value);
                        }

                        // strip out any bold spans 
                        while (Regex.IsMatch(paragraph, "<span class=\"bd\">.*?<\\/span>"))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"bd\\\">(?<content>.*?)<\\/span>");
                            paragraph = paragraph.Replace(spanM.Value, spanM.Groups["content"].Value);
                        }

                        // fix small caps.  Right now they'll look like this: 
                        // <span class="sc">Lord</i>
                        while (Regex.IsMatch(paragraph, "class=\"sc\""))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"sc\\\">(?<text>.*?)<\\/span>");
                            string innerText = spanM.Groups["text"].Value
                                .Replace("a", "ᴀ")
                                .Replace("b", "ʙ")
                                .Replace("c", "ᴄ")
                                .Replace("d", "ᴅ")
                                .Replace("e", "ᴇ")
                                .Replace("f", "ꜰ")
                                .Replace("g", "ɢ")
                                .Replace("h", "ʜ")
                                .Replace("i", "ɪ")
                                .Replace("j", "ᴊ")
                                .Replace("k", "ᴋ")
                                .Replace("l", "ʟ")
                                .Replace("m", "ᴍ")
                                .Replace("n", "ɴ")
                                .Replace("o", "ᴏ")
                                .Replace("p", "ᴘ")
                                .Replace("q", "ǫ")
                                .Replace("r", "ʀ")
                                .Replace("s", "s")
                                .Replace("t", "ᴛ")
                                .Replace("u", "ᴜ")
                                .Replace("v", "ᴠ")
                                .Replace("w", "ᴡ")
                                .Replace("x", "x")
                                .Replace("y", "ʏ")
                                .Replace("z", "ᴢ");
                            paragraph = paragraph.Replace(spanM.Value, innerText);
                        }

                        // convert italics tags
                        while (Regex.IsMatch(paragraph,
                            "(?<pretext>.*?)<span class=\\\"it\\\">(?<italicsText>.*?)<\\/span>(?<posttext>.*)"))
                        {
                            Regex ItalicsRegex =
                                new Regex(
                                    "(?<pretext>.*?)<span class=\"it\">(?<italicsText>.*?)<\\/span>(?<posttext>.*)");
                            Match m2 = ItalicsRegex.Match(paragraph);
                            paragraph = m2.Groups["pretext"] + "<i>" + m2.Groups["italicsText"] + "</i>" +
                                        m2.Groups["posttext"];
                        }

                        // convert words of Jesus to red. This needs to happen after italics.
                        while (Regex.IsMatch(paragraph, "<span class=\"wj\">.*?<\\/span>"))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"wj\\\">(?<content>.*?)<\\/span>");
                            string innerText = spanM.Groups["content"].Value;
                            paragraph = paragraph.Replace(spanM.Value, $"<font color=red>{innerText}</font>");
                        }

                        // strip out any double spaces
                        while (paragraph.Contains("  "))
                        {
                            paragraph = paragraph.Replace("  ", " ");
                        }

                        // strip out any doubled-up italics tags
                        paragraph = paragraph.Replace("</i> <i>", " ");

                        // strip out any doubled-up red-letter tags
                        paragraph = paragraph.Replace("</font><font color=red>", "")
                            .Replace("</font> <font color=red>", " ");


                        // now, for each span that is left, extract the verse, and the text, and slap it into the array.
                        bool firstVerseInParagraph = true;
                        Regex versesRegex = new Regex("<span class=\"verse v(?<verseNumber>\\d{1,3})\".*?>(?<text>.*?)<\\/span>");
                        MatchCollection verseMatches = versesRegex.Matches(paragraph);
                        foreach (Match v in verseMatches)
                        {
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].Book = bookCode;
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].ChapterName = chapterCode;
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseNumber =
                                uint.Parse(v.Groups["verseNumber"].Value);
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseText = v.Groups["text"].Value;
                            if (firstVerseInParagraph)
                            {
                                verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].StartsParagraph = true;
                                if (uint.Parse(v.Groups["verseNumber"].Value) > 1)
                                {
                                    verseList[uint.Parse(v.Groups["verseNumber"].Value) - 2].EndsParagraph = true;
                                }
                            }
                            firstVerseInParagraph = false;
                        }
                    }
                }
                else if (m.Value.Contains("class=\"q"))
                {

                    // let's handle the verse suffix's first
                    if (m.Value.Contains("class=\"qs\""))
                    {
                        // it's a verse suffix
                        Regex verseSuffixRegex = new Regex("verse v(?<verse>\\d{0,3})\".*tent\\\">(?<text>.*?)<");
                        Match m2 = verseSuffixRegex.Match(m.Value);
                        verseList[uint.Parse(m2.Groups["verse"].Value) - 1].VerseSuffix = m2.Groups["text"].Value;
                    }
                    else
                    {

                        // it's a multiline verse. Save whether it's a q1 or a q2
                        string multilineType = m.Value.Contains("q1") ? "q1" : "q2";

                        // let's start by scrubbing the easy stuff
                        string paragraph = m.Value
                            .Replace("&#8217;", "’")
                            .Replace("&#8216;", "‘")
                            .Replace("&#8220;", "“")
                            .Replace("&#8221;", "”")
                            .Replace("&#8212;", "—")
                            .Replace("&#8211;", "–")
                            .Replace("&#274;", "E")                     /// fix for bible.org making Enosh into Ē'nosh
                            .Replace("&#180;", " ")                     /// fix for bible.org making Enosh into Ē'nosh
                            .Replace("&#233;", "é")
                                          ;

                        // strip out div tags
                        paragraph = Regex.Replace(paragraph, "<\\/div>", "");
                        paragraph = Regex.Replace(paragraph, "<div class=\\\"q1\\\">", "");
                        paragraph = Regex.Replace(paragraph, "<div class=\\\"q2\\\">", "");

                        // strip label from notes span from string
                        paragraph = paragraph.Replace("<span class=\"label\">#</span>", "");


                        // strip any body spans
                        paragraph = Regex.Replace(paragraph, "<span class=\\\" {0,1}body\\\">.*?<\\/span>", "");


                        // strip out notes spans
                        paragraph = Regex.Replace(paragraph, "<span class=\"note.*?<\\/span>", "");

                        // strip out the verse labels
                        paragraph = Regex.Replace(paragraph, "<span class=\"label\">\\d+<\\/span>", "");

                        // strip out any content tags
                        while (Regex.IsMatch(paragraph, "<span class=\"content\">.*?<\\/span>"))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"content\\\">(?<content>.*?)<\\/span>");
                            paragraph = paragraph.Replace(spanM.Value, spanM.Groups["content"].Value);
                        }

                        // fix small caps.  Right now they'll look like this: 
                        // <span class="sc">Lord</i>
                        while (Regex.IsMatch(paragraph, "class=\"sc\""))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"sc\\\">(?<text>.*?)<\\/span>");
                            string innerText = spanM.Groups["text"].Value
                                .Replace("a", "ᴀ")
                                .Replace("b", "ʙ")
                                .Replace("c", "ᴄ")
                                .Replace("d", "ᴅ")
                                .Replace("e", "ᴇ")
                                .Replace("f", "ꜰ")
                                .Replace("g", "ɢ")
                                .Replace("h", "ʜ")
                                .Replace("i", "ɪ")
                                .Replace("j", "ᴊ")
                                .Replace("k", "ᴋ")
                                .Replace("l", "ʟ")
                                .Replace("m", "ᴍ")
                                .Replace("n", "ɴ")
                                .Replace("o", "ᴏ")
                                .Replace("p", "ᴘ")
                                .Replace("q", "ǫ")
                                .Replace("r", "ʀ")
                                .Replace("s", "s")
                                .Replace("t", "ᴛ")
                                .Replace("u", "ᴜ")
                                .Replace("v", "ᴠ")
                                .Replace("w", "ᴡ")
                                .Replace("x", "x")
                                .Replace("y", "ʏ")
                                .Replace("z", "ᴢ");
                            paragraph = paragraph.Replace(spanM.Value, innerText);
                        }

                        // convert italics tags
                        while (Regex.IsMatch(paragraph,
                            "(?<pretext>.*?)<span class=\\\"it\\\">(?<italicsText>.*?)<\\/span>(?<posttext>.*)"))
                        {
                            Regex ItalicsRegex =
                                new Regex(
                                    "(?<pretext>.*?)<span class=\"it\">(?<italicsText>.*?)<\\/span>(?<posttext>.*)");
                            Match m2 = ItalicsRegex.Match(paragraph);
                            paragraph = m2.Groups["pretext"] + "<i>" + m2.Groups["italicsText"] + "</i>" +
                                        m2.Groups["posttext"];
                        }

                        // convert words of Jesus to red. This needs to happen after italics.
                        while (Regex.IsMatch(paragraph, "<span class=\"wj\">.*?<\\/span>"))
                        {
                            Match spanM = Regex.Match(paragraph, "<span class=\\\"wj\\\">(?<content>.*?)<\\/span>");
                            string innerText = spanM.Groups["content"].Value;
                            paragraph = paragraph.Replace(spanM.Value, $"<font color=red>{innerText}</font>");
                        }

                        // strip out any double spaces
                        while (paragraph.Contains("  "))
                        {
                            paragraph = paragraph.Replace("  ", " ");
                        }

                        // strip out any doubled-up italics tags
                        paragraph = paragraph.Replace("</i> <i>", " ");

                        // strip out any doubled-up red-letter tags
                        paragraph = paragraph.Replace("</font><font color=red>", "")
                            .Replace("</font> <font color=red>", " ");


                        // now, for each span that is left, extract the verse, and the text, and slap it into the array.
                        bool firstVerseInParagraph = multilineType == "q1";
                        Regex versesRegex = new Regex("<span class=\"verse v(?<verseNumber>\\d{1,3})\".*?>(?<text>.*?)<\\/span>");
                        MatchCollection verseMatches = versesRegex.Matches(paragraph);
                        foreach (Match v in verseMatches)
                        {
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].Book = bookCode;
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].ChapterName = chapterCode;
                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseNumber =
                                uint.Parse(v.Groups["verseNumber"].Value);

                            verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseText =
                                verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseText +
                                (multilineType == "q1" && verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].VerseText == string.Empty ? string.Empty : "<br />") +
                                v.Groups["text"].Value;

                            if (firstVerseInParagraph)
                            {
                                verseList[uint.Parse(v.Groups["verseNumber"].Value) - 1].StartsParagraph = true;
                                if (uint.Parse(v.Groups["verseNumber"].Value) > 1)
                                {
                                    verseList[uint.Parse(v.Groups["verseNumber"].Value) - 2].EndsParagraph = true;
                                }
                            }
                        }
                    }
                }

            }

            foreach (Verse v in verseList.Where(f => f.VerseNumber != 0))
            {
                _bible.AddVerse(v);
            }

        }

        static T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }

            return array;
        }

    }
}