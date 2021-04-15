using System;
using System.Net;

namespace BibleComScraper.Services
{
    class HttpService
    {

        private PageCacheService pageCache = new PageCacheService();

        public string GetPage(string url, bool ignoreCache = false)
        {

            // see if the page exists in the page cache;
            var pageInCache = pageCache.IsPageCached(url);
            Console.WriteLine((pageInCache ? "page is already in cache" : "page is not in cache"));
            if (pageInCache && !ignoreCache)
            {
                return pageCache.GetPage(url);
            }
            using (WebClient client = new WebClient())
            {
                // pretend to be the latest chrome
                client.Headers["User-Agent"] =
                    "Mozilla / 5.0(Windows NT 10.0; WOW64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 62.0.3202.9 Safari / 537.36";
                byte[] arr = client.DownloadData(url);
                string result = System.Text.Encoding.Default.GetString(arr);
                pageCache.AddPage(url, result);
                return result;
            }
        }
    }
}
