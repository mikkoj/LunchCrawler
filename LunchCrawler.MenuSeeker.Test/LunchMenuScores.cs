using System.Collections.Generic;
using System.Linq;


namespace LunchCrawler.MenuSeeker.Test
{
    public class LunchMenuScores
    {
        public IList<LunchMenuScorePoint> Points { get; set; }

        /// <summary>
        /// Probability that the analyzed URL is a lunch menu.
        /// </summary>
        public double LunchMenuProbability
        {
            get
            {
                var totalPoints = Points.Sum(point => point.PointsGiven);
                
                // TODO: logic for probability
                return totalPoints / 50.0;
            }
        }
    }
}

