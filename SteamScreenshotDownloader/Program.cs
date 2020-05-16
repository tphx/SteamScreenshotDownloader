using System.Threading.Tasks;

namespace SteamScreenshotDownloader
{
    class Program
    {

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Downloader downloader = new Downloader();
            await downloader.Start();
        }

    }
}
