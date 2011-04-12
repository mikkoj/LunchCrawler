namespace LunchCrawler.Common.Enums
{
    public enum LunchRestaurantStatus
    {
        Unknown = 0,

        OK = 1,
        CannotConnect = 2,
        Invalid = 3,
        ShouldSkip = 4,
        AnalysisFailed = 5,
        ReAnalysisFailed = 6,
        ManuallyAdded = 7
    }
}
