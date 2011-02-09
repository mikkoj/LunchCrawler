namespace LunchCrawler.Common.Interfaces
{
    public interface ILunchMenuSeeker
    {
        /// <summary>
        /// Starts the seeking process in a separate thread.
        /// </summary>
        void Start();

        /// <summary>
        /// Seeks lunch menus from various sources.
        /// </summary>
        void SeekLunchMenus();
    }
}