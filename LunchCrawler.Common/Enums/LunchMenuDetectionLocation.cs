using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunchCrawler.Common.Enums
{
    /// <summary>
    /// The location of a lunch menu detection (url path, title, content etc.)
    /// </summary>
    public enum LunchMenuDetectionLocation
    {
        Unknown = 0,

        UrlPath = 1,
        PageTitle = 2,
        Content = 3
    }
}
