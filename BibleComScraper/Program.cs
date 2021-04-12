using Microsoft.Data.Sqlite;
using System;

namespace BibleComScraper
{
    class Program
    {
        private static HttpService http = new HttpService();

        static void Main(string[] args)
        {

            Console.WriteLine("bible.com scraper\n-=≡=-=≡=-=≡=-=≡=-\n\n");

            string langPage = string.Empty;
            while (string.IsNullOrWhiteSpace(langPage))
            {
                Console.Write("Enter iso language code [eng]: ");
                Console.ForegroundColor = ConsoleColor.White;
                var languageCode = Console.ReadLine();
                if (languageCode == string.Empty) languageCode = "eng";
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

            Console.WriteLine(langPage);

            using (var connection = new SqliteConnection("Data Source=bible.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                var reader = command.ExecuteScalar().ToString();
                Console.WriteLine(reader.ToString());

                connection.Close();
            }

            Console.ReadKey();

        }





    }
}
