using System;
using System.Collections.Generic;

namespace LunchCrawler.Common
{
    public class UrlComparer : IEqualityComparer<string>
    {
        public bool Equals(string url1, string url2)
        {
            return Utils.GetBaseUrl(url1).Equals(Utils.GetBaseUrl(url2), StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string url)
        {
            return Utils.GetBaseUrl(url).GetHashCode();
        }
    }
}
