using HtmlAgilityPack;

using Lunch.Common.Enums;


namespace LunchCrawler.Analyzer.Test
{
    public class LunchFeature
    {
        public LunchFeatureType Type { get; set; }
        public FoodMatchType FoodMatchType { get; set; }
        public int Line { get; set; }
        public int LinePosition { get; set; }

        public string InnerText { get; set; }
        public string OuterHtml { get; set; }

        public HtmlNode DetectedNode { get; set; }
    }
}
