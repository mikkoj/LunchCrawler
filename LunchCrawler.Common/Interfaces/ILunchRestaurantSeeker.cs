namespace LunchCrawler.Common.Interfaces
{
    public interface ILunchRestaurantSeeker
    {
        /// <summary>
        /// Starts the seeking process in a separate thread.
        /// </summary>
        void Start();

        /// <summary>
        /// Seeks lunch restaurants from various sources.
        /// </summary>
        void SeekLunchRestaurants();
    }
}