using System;
using System.IO;
using BibleComScraper.Classes;
using BibleComScraper.Enums;
using Microsoft.Data.Sqlite;

namespace BibleComScraper.Services
{
    class BibleService
    {

        public string BibleCode { get; init; }
        public string BibleName { get; init; }
        public uint TranslationCode { get; init; }

        public BibleService(string bibleCode, string bibleName, uint translationCode)
        {
            BibleCode = bibleCode;
            BibleName = bibleName;
            TranslationCode = translationCode;
        }

        private string DatabaseFileName
        {
            get
            {
                return $"{BibleCode}.bible.db";
            }
        }

        public bool DatabaseExists
        {
            get
            {
                return File.Exists(DatabaseFileName);
            }
        }

        public void DeleteDatabase()
        {
            if (DatabaseExists)
            {
                File.Delete(DatabaseFileName);
            }
        }

        private bool TableExists(string tableName)
        {
            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$tableName";
                    sqlCmd.Parameters.AddWithValue("$tableName", tableName);
                    return int.Parse(sqlCmd.ExecuteScalar().ToString() ?? "0") > 0;
                }
            }

        }

        private bool IsMetadataInitialized()
        {
            return TableExists("translation_metadata");
        }

        private bool IsJobQueueInitialized()
        {
            return TableExists("worker_queue");
        }
        private bool IsBookListInitialized()
        {
            return TableExists("books");
        }

        private bool IsChapterListInitialized()
        {
            return TableExists("chapters");
        }

        public bool InitializeDatabase()
        {
            if (!IsMetadataInitialized())
            {
                using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
                {
                    // Create the translation metadata
                    using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                    {
                        sqlCmd.CommandText = "CREATE TABLE translation_metadata (key TEXT, value TEXT)";
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd.CommandText = "INSERT INTO translation_metadata (key, value) VALUES ($key, $value)";
                        sqlCmd.Parameters.AddWithValue("$key", "name");
                        sqlCmd.Parameters.AddWithValue("$value", BibleName);
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd.Parameters.Clear();
                        sqlCmd.Parameters.AddWithValue("$key", "code");
                        sqlCmd.Parameters.AddWithValue("$value", BibleCode);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
            }

            if (!IsJobQueueInitialized())
            {
                using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
                {
                    // Create the translation metadata
                    using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                    {
                        sqlCmd.CommandText = "CREATE TABLE worker_queue (id TEXT, type TEXT, url TEXT)";
                        sqlCmd.ExecuteNonQueryAsync();
                    }
                }

                // initialize the starting job  
                CreateJob(JobTypes.EnumerateBooks, $"https://www.bible.com/json/bible/books/{TranslationCode}");
            }

            if (!IsBookListInitialized())
            {
                using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
                {
                    using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                    {
                        sqlCmd.CommandText = "CREATE TABLE books ([index] NUMBER, name TEXT, code TEXT)";
                        sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }

            if (!IsChapterListInitialized())
            {
                using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
                {
                    using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                    {
                        sqlCmd.CommandText = "CREATE TABLE chapters (code TEXT, [index] NUMBER, name TEXT)";
                        sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }

            return true;
        }

        public Guid CreateJob(JobTypes type, string url)
        {
            Guid id = Guid.NewGuid();
            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                // Create the translation metadata
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "INSERT INTO worker_queue (id, type, url) VALUES ($id, $type, $url)";
                    sqlCmd.Parameters.AddWithValue("$id", id.ToString());
                    sqlCmd.Parameters.AddWithValue("$type", type);
                    sqlCmd.Parameters.AddWithValue("$url", url);
                    sqlCmd.ExecuteNonQueryAsync();
                }
            }
            return id;
        }

        public uint JobsRemaining
        {
            get
            {
                using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
                {
                    // Create the translation metadata
                    using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                    {
                        sqlCmd.CommandText = "SELECT COUNT(*) FROM worker_queue";
                        return uint.Parse(sqlCmd.ExecuteScalar().ToString()!);
                    }
                }
            }
        }

        private SqliteConnection CreateOpenedSqliteConnection()
        {
            SqliteConnection connection = new SqliteConnection($"Data Source={DatabaseFileName}");
            connection.Open();
            return connection;
        }

        public Job GetRandomJob()
        {
            if (JobsRemaining == 0) throw new InvalidOperationException("No jobs remain");

            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "SELECT id, type, url FROM worker_queue ORDER BY RANDOM() LIMIT 1";
                    using (SqliteDataReader sqlReader = sqlCmd.ExecuteReader())
                    {
                        sqlReader.Read();
                        Guid id = sqlReader.GetGuid(sqlReader.GetOrdinal("id"));
                        JobTypes type = (JobTypes) sqlReader.GetInt16(sqlReader.GetOrdinal("type"));
                        string url = sqlReader.GetString(sqlReader.GetOrdinal("url"));
                        return new Job(id, type, url);
                    }
                }
            }
        }

        public void DeleteJob(Job job)
        {
            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "DELETE FROM worker_queue WHERE id = $id";
                    sqlCmd.Parameters.AddWithValue("$id", job.Id.ToString());
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }

        public void AddBook(Book book)
        {
            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "INSERT INTO books ([index], name, code) values ($index, $name, $code)";
                    sqlCmd.Parameters.AddWithValue("$index", book.Index);
                    sqlCmd.Parameters.AddWithValue("$name", book.Name);
                    sqlCmd.Parameters.AddWithValue("$code", book.Code);
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }

        public void AddChapter(Chapter chapter)
        {
            using (SqliteConnection sqlConn = CreateOpenedSqliteConnection())
            {
                using (SqliteCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "INSERT INTO chapters ([index], name, code) values ($index, $name, $code)";
                    sqlCmd.Parameters.AddWithValue("$index", chapter.Index);
                    sqlCmd.Parameters.AddWithValue("$name", chapter.Name);
                    sqlCmd.Parameters.AddWithValue("$code", chapter.Code);
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }


    }
}
