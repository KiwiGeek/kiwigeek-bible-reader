using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using Microsoft.Data.Sqlite;

namespace KiwiGeekBible.WPF.Services
{
    class BibleService
    {

        internal string BibleCode { get; init; }
        private readonly SqliteConnection sqlConn;

        public BibleService(string bibleCode)
        {
            BibleCode = bibleCode;
            if (!DatabaseExists) throw new FileNotFoundException($"Cannot find {DatabaseFileName}");
            sqlConn = CreateOpenedSqliteConnection();
        }

        private string DatabaseFileName
        {
            get
            {
                return $"Versions/{BibleCode}.bible.db";
            }
        }

        internal bool DatabaseExists
        {
            get
            {
                return File.Exists(DatabaseFileName);
            }
        }


        private SqliteConnection CreateOpenedSqliteConnection()
        {
            SqliteConnection connection = new SqliteConnection($"Data Source={DatabaseFileName}");
            connection.Open();
            return connection;
        }


        internal List<Book> GetBooks()
        {

            List<Book> results = new List<Book>();


            using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT name, code FROM books ORDER BY [index]";
                using (SqliteDataReader sqlReader = sqlCmd.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        Book newBook = new Book()
                        {
                            Name = sqlReader.GetString(sqlReader.GetOrdinal("name")),
                            CanonicalCode = sqlReader.GetString(sqlReader.GetOrdinal("code"))
                        };
                        results.Add(newBook);
                    }
                }
            }


            return results;

        }

        internal List<Chapter> GetChapters(Book book)
        {

            List<Chapter> results = new List<Chapter>();


            using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT name FROM chapters WHERE code = $book ORDER BY [index]";
                sqlCmd.Parameters.AddWithValue("$book", book.CanonicalCode);
                using (SqliteDataReader sqlReader = sqlCmd.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        Chapter newChapter = new Chapter()
                        {
                            ChapterNumber = uint.Parse(sqlReader.GetString(sqlReader.GetOrdinal("name")))
                        };
                        results.Add(newChapter);
                    }
                }
            }


            return results;

        }

        internal List<Verse> GetVerses(Book book, Chapter chapter)
        {
            List<Verse> results = new List<Verse>();

            using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT verse_number, section, prefix, verse, suffix, starts_paragraph, ends_paragraph " +
                                     "FROM verses WHERE book = $book AND chapter = $chapter ORDER BY verse_number";
                sqlCmd.Parameters.AddWithValue("$book", book.CanonicalCode);
                sqlCmd.Parameters.AddWithValue("$chapter", chapter.ChapterNumber);
                using (SqliteDataReader sqlReader = sqlCmd.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        Verse newVerse = new Verse()
                        {
                            VerseNumber = uint.Parse(sqlReader.GetString(sqlReader.GetOrdinal("verse_number"))),
                            SectionTitle = sqlReader.GetString(sqlReader.GetOrdinal("section")),
                            VersePrefix = sqlReader.GetString(sqlReader.GetOrdinal("prefix")),
                            VerseText = sqlReader.GetString(sqlReader.GetOrdinal("verse")),
                            VerseSuffix = sqlReader.GetString(sqlReader.GetOrdinal("suffix")),
                            StartsParagraph = sqlReader.GetString(sqlReader.GetOrdinal("starts_paragraph")) != "0",
                            EndsParagraph = sqlReader.GetString(sqlReader.GetOrdinal("ends_paragraph")) != "0"
                        };
                        results.Add(newVerse);
                    }
                }

            }

            return results;

        }
    }
}
