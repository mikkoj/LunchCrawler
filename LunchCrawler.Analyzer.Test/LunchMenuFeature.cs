using LunchCrawler.Common.Enums;
using HtmlAgilityPack;


namespace LunchCrawler.Analyzer.Test
{
    public class LunchMenuFeature
    {
        public LunchMenuFeatureType Type { get; set; }
        public StringMatchType FoodMatchType { get; set; }
        public int Line { get; set; }
        public int LinePosition { get; set; }

        public string InnerText { get; set; }
        public string OuterHtml { get; set; }

        public HtmlNode DetectedNode { get; set; }
    }
}
