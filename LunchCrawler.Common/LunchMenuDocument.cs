using HtmlAgilityPack;


namespace LunchCrawler.Common
{
    public class LunchMenuDocument
    {
        /// <summary>
        /// MIME-type for the document.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// SHA-256 hash for the document.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Represents a complete HTML document.
        /// </summary>
        public HtmlDocument HtmlDocument { get; set; }
    }
}
