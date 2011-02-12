using LunchCrawler.Common.Model;
using LunchCrawler.Data.Local;


namespace LunchCrawler.Common.Interfaces
{
    /// <summary>
    /// Interface for a parsing strategy for a lunch menu document.
    /// A strategy should determine confidence with results, as a LunchMenu property.
    /// </summary>
    public interface ILunchMenuParsingStrategy
    {
        int Priority { get; }
        LunchMenu ParseLunchMenu(LunchRestaurant document);
    }
}
