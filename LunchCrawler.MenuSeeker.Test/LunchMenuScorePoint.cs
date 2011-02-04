using LunchCrawler.Common.Enums;


namespace LunchCrawler.MenuSeeker.Test
{
    /// <summary>
    /// Represents a single lunch menu -match that was given a point.
    /// </summary>
    public class LunchMenuScorePoint
    {
        /// <summary>
        /// Determines the location of the detection (url path, title, content etc.)
        /// </summary>
        public LunchMenuDetectionLocation DetectionLocation { get; set; }

        /// <summary>
        /// Determines the type of the match (exact, partial, fuzzy etc.)
        /// </summary>
        public StringMatchType DetectionType { get; set; }

        /// <summary>
        /// Detected word.
        /// </summary>
        public string DetectedWord { get; set; }

        /// <summary>
        /// Points given to the word.
        /// </summary>
        public int PointsGiven { get; set; }
    }
}
