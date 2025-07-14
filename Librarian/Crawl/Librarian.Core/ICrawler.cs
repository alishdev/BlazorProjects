namespace Librarian.Core
{
    public interface ICrawler
    {
        /// <summary>
        /// The main execution method for the crawler.
        /// </summary>
        /// <param name="parameter">A configuration object passed from the scheduler.</param>
        void Run(object parameter);
    }
}