using System.Collections.Generic;
using System.Linq;


namespace LunchCrawler.Common.Model
{
    public class LunchMenuScores
    {
        private decimal? _lunchmenuProbability;

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
                if (!_lunchmenuProbability.HasValue)
                {
                    var totalPoints = Points.Sum(point => point.PointsGiven);

                    // bonus points for all week days
                    var weekDays = new[] { "maanantai", "tiistai", "keskiviikko", "torstai", "perjantai" };
                    if (weekDays.All(day => Points.Any(point => point.DetectedKeyword.ToLowerInvariant().Equals(day))))
                    {
                        Points.Add(new LunchMenuScorePoint { PointsGiven = 20 });
                    }

                    // TODO: more logic for probability

                    _lunchmenuProbability = totalPoints / 100.0M;
                }

                return _lunchmenuProbability.Value;
            }
        }
    }
}

