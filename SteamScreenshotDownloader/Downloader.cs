using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamScreenshotDownloader
{
    class Downloader
    {
        int totalScreenshots = 0;

        public async Task Start()
        {
            Console.WriteLine("Enter a steam ID to get the screenshots for");
            string id = Console.ReadLine();

            while(string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("Enter a steam ID to get the screenshots for");
                id = Console.ReadLine();
            }

            await GetPage(id, 1);

            Console.WriteLine("Done downloading " + totalScreenshots + " screenshots");
            Console.ReadKey();
        }

        private async Task GetPage(string id, int pageNumber)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
                var html = await client.GetStringAsync("https://steamcommunity.com/profiles/" + id + "/screenshots/?p=" + pageNumber + "&sort=newestfirst&browsefilter=myfiles&view=grid&privacy=30");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var result = doc.DocumentNode.SelectNodes("//div[contains(@class, 'imgWall')]");

                if (result != null)
                {
                    foreach (var c in result)
                    {
                        var url = Regex.Match(c.GetAttributeValue("style", ""), @"(?<=url\()(.*)(?=\))").Groups[1].Value;
                        string screenshotId = Regex.Match(c.InnerHtml, @"screenshots\[(.*?)\]").ToString();
                        screenshotId = screenshotId.Replace("screenshots[", "");
                        screenshotId = screenshotId.Replace("]", "");


                        if (!string.IsNullOrEmpty(screenshotId))
                        {
                            await GetScreenshots(screenshotId);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Could not find screenshots for Steam ID " + id);
                    return;
                }

                await NextPage(id, pageNumber, doc);
            }
        }

        private async Task NextPage(string id, int pageNumber, HtmlDocument doc)
        {
            var pageNumberLinks = doc.DocumentNode.SelectNodes("//a[contains(@class, 'pagingPageLink')]");

            if (pageNumberLinks != null)
            {
                if (pageNumber <= pageNumberLinks.Count)
                {
                    pageNumber++;
                    Console.WriteLine("Page " + pageNumber + " of " + (pageNumberLinks.Count + 1));
                    await GetPage(id, pageNumber);
                }
            }
        }
    
        private async Task GetScreenshots(string id)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
                var html = await client.GetStringAsync("https://steamcommunity.com/sharedfiles/filedetails/?id=" + id);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                var result = doc.DocumentNode.SelectNodes("//img[contains(@id, 'ActualMedia')]");

                foreach (var c in result)
                {
                    DownloadScreenshots(id, c.GetAttributeValue("src", "").Split('?').First());
                }
            }
        }

        private void DownloadScreenshots(string id, string url)
        {
            using (var client = new WebClient())
            {
                Console.WriteLine("Donwnloading screenshot {0} from {1}", id, url);
                client.DownloadFileAsync(new Uri(url), "Screenshots\\" + id + ".png");
                totalScreenshots++;
            }
        }
    }
}
