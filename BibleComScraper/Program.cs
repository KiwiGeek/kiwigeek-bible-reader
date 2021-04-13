using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BibleComScraper
{
    class Program
    {
        private static HttpService http = new HttpService();

        static void Main(string[] args)
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
                    langPage = http.GetPage($"https://www.bible.com/languages/{languageCode}");
                }
                catch
                {
                    Console.WriteLine("Language is unsupported or incorrect, or something else went terribly wrong.");
                }
            }

            // get a translation
            Console.WriteLine("Possible translations: ");

            Regex translationsReg = new Regex("<a role=button target=_self class=\"db pb2 lh-copy yv-green link\" href=(?<url>\\/versions\\/(?<slug>(?<code>\\d*)-.*?))>(?<name>.*?)<\\/a>");
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
                                             .Groups["url"].ToString();

                    startingPageContent = http.GetPage(startingUrl);

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
                .Groups["name"].ToString();
            string bibleCode = Regex.Match(firstChapterUrl, ".*\\.(.+?)$").Groups[1].Value;

            Console.Write($"Retrieving {bibleName} starting from {firstChapterUrl}");

            try
            {
                if (File.Exists($"{bibleCode}.bible.db"))
                {
                    File.Delete($"{bibleCode}.bible.db");
                }
            }
            catch
            {
                Console.WriteLine($"can't delete existing file {bibleCode}.bible.db.  Quitting...");
                return;
            }

            using (var connection = new SqliteConnection($"Data Source={bibleCode}.bible.db"))
            {
                connection.Open();

                // Create the translation metadata
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE translation_metadata (key TEXT, value TEXT)";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO translation_metadata (key, value) VALUES ($key, $value)";
                    command.Parameters.AddWithValue("$key", "name");
                    command.Parameters.AddWithValue("$value", bibleName);
                    command.ExecuteNonQuery();

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$key", "code");
                    command.Parameters.AddWithValue("$value", bibleCode);
                    command.ExecuteNonQuery();

                }

                // get all the books of a specific translation
                // https://www.bible.com/json/bible/books/296
                // get all the chapters of a specific book
                // https://www.bible.com/json/bible/books/296/GEN/chapters

                connection.Close();
            }

            Console.ReadKey();

        }

    }
}
