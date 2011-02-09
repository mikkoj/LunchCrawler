using System;
using System.Collections.Generic;
using System.Linq;


namespace LunchCrawler.MenuSeeker.Test.Model
{
    public class LunchMenuScores
    {
        public LunchMenuScores()
        {
            Points = new List<LunchMenuScorePoint>();
        }

        public IList<LunchMenuScorePoint> Points { get; set; }

        /// <summary>
        /// Probability that the analyzed URL is a lunch menu.
        /// </summary>
        public decimal LunchMenuProbability
        {
            get
            {
                var totalPoints = Points.Sum(point => point.PointsGiven);
                
                // TODO: logic for probability
                return totalPoints / 100.0M;
            }
        }
    }
}

