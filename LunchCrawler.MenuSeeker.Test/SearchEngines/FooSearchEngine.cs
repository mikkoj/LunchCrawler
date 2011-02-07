using System.Collections.Generic;
using System.ComponentModel.Composition;

using LunchCrawler.Common.Interfaces;


namespace LunchCrawler.MenuSeeker.Test.SearchEngines
{
    /// <summary>
    /// Search engine for lunch menus based on Google's APIs.
    /// </summary>
    [Export(typeof(ISearchEngine))]
    public class FooSearchEngine : ISearchEngine
    {
        public IList<string> SearchForLinks(string query)
        {
            return null;
        }
    }
}
