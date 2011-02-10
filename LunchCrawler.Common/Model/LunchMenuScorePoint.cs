using LunchCrawler.Common.Enums;


namespace LunchCrawler.Common.Model
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
        /// The basic lunch menu keyword that was detected.
        /// </summary>
        public string DetectedKeyword { get; set; }

        /// <summary>
        /// The entire text that was detected for the node, title, URL, or etc.
        /// </summary>
        public string DetectedText { get; set; }
        
        /// <summary>
        /// Points given to the word.
        /// </summary>
        public int PointsGiven { get; set; }
    }
}
