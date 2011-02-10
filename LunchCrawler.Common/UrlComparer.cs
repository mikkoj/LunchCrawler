using System;
using System.Collections.Generic;

namespace LunchCrawler.Common
{
    public class UrlComparer : IEqualityComparer<string>
    {
        public bool Equals(string url1, string url2)
        {
            if (!Utils.GetBaseUrl(url1).Equals(Utils.GetBaseUrl(url2), StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (url1.Equals(url2, StringComparison.InvariantCultureIgnoreCase))
                return true;

            LunchMenuDocument l1 = Utils.GetLunchMenuDocumentForUrl(url1);
            LunchMenuDocument l2 = Utils.GetLunchMenuDocumentForUrl(url2);

            return l1.Hash != null && l1.Hash.Equals(l2.Hash);
        }

        public int GetHashCode(string url)
        {
            return url.GetHashCode();
        }
    }
}
