using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common.Model;
using LunchCrawler.Data.Local;


namespace LunchCrawler.Common.Interfaces
{
    public interface ILunchMenuDetection
    {
        /// <summary>
        /// Scores a single HtmlNode as lunch menu content.
        /// </summary>
        /// <param name="node">Node to be scored.</param>
        LunchMenuScorePoint ScoreNode(HtmlNode node);


        /// <summary>
        /// Updates the detection count for lunch menu keywords inside the DB.
        /// </summary>
        void UpdateLunchMenuKeywordCountsDB();


        /// <summary>
        /// Updates the detection count for deep link keywords inside the DB.
        /// </summary>
        void UpdateDeepLinkKeywordCountsDB();


        /// <summary>
        /// Calculates scorepoints for a given document.
        /// </summary>
        /// <param name="lunchMenuDocument">Document to be scored.</param>
        IList<LunchMenuScorePoint> GetScorePointsForDocument(LunchRestaurantDocument lunchMenuDocument);


        /// <summary>
        /// Seeks deep-links recursively.
        /// </summary>
        /// <param name="pageDocument">HTML document to be checked.</param>
        /// <param name="deepLinks">Collection of deep links.</param>
        void FindDeepLinks(LunchRestaurantDocument pageDocument, IList<RestaurantDeepLink> deepLinks);


        /// <summary>
        /// Scores a single HtmlNode as a deep link.
        /// </summary>
        /// <param name="link">Node to be scored.</param>
        LunchMenuScorePoint ScoreDeepLink(string link);
    }
}