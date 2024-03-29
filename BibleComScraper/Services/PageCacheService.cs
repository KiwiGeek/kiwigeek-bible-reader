﻿using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace BibleComScraper.Services
{
    class PageCacheService
    {
        private SqliteConnection pageCache = new SqliteConnection("Data Source=page_cache.db");

        public PageCacheService()
        {

            pageCache.Open();

            Debug.WriteLine("Initializing Page Cache");
            using (SqliteCommand sqlCmd = pageCache.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' and name='cache'";
                bool tableExists = (sqlCmd.ExecuteScalar().ToString() == "1");
                if (!tableExists)
                {
                    Debug.WriteLine("Cache table does not exist. Creating...");
                    sqlCmd.CommandText = "CREATE TABLE cache (url TEXT, content TEXT);";
                    sqlCmd.ExecuteNonQuery();

                }
            }

        }

        public bool IsPageCached(string url)
        {
            using (SqliteCommand sqlCmd = pageCache.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT COUNT(*) FROM cache WHERE url = $url";
                sqlCmd.Parameters.AddWithValue("$url", url);
                bool pageCacheExists = (sqlCmd.ExecuteScalar().ToString() == "1");
                return pageCacheExists;
            }
        }

        public string GetPage(string url)
        {
            using (SqliteCommand sqlCmd = pageCache.CreateCommand())
            {
                sqlCmd.CommandText = "SELECT content FROM cache WHERE url = $url";
                sqlCmd.Parameters.AddWithValue("$url", url);
                string pageContect = sqlCmd.ExecuteScalar().ToString();
                return pageContect;
            }
        }

        public void AddPage(string url, string content)
        {
            using (SqliteCommand sqlCmd = pageCache.CreateCommand())
            {
                sqlCmd.CommandText = "INSERT INTO cache (url, content) VALUES ($url, $content)";
                sqlCmd.Parameters.AddWithValue("$url", url);
                sqlCmd.Parameters.AddWithValue("$content", content);
                sqlCmd.ExecuteNonQuery();
            }
        }

        ~PageCacheService()
        {
            pageCache.Close();
        }
    }
}
