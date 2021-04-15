using System;
using System.Linq;
using System.Text.RegularExpressions;
using BibleComScraper.Classes;
using BibleComScraper.Enums;
using BibleComScraper.Services;

namespace BibleComScraper
{
    static class Program
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

    }
}