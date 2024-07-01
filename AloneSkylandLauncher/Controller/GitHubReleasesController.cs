using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;

public class GitHubReleasesController
{
    public static readonly string GitHubReleasesUrl = "https://github.com/DaddyCalcifer/AloneSkyland/releases";

    async Task<Dictionary<string, string>> GetReleasesAsync()
    {
        var releases = new Dictionary<string, string>();

        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetStringAsync(GitHubReleasesUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            var versionNodes = htmlDocument.DocumentNode.SelectNodes("//span[contains(@class, 'ml-1 wb-break-all')]");
            var count = versionNodes.Count;
            Console.WriteLine(count);
            for (var i = 0; i < count; i++)
            {
                Console.WriteLine($"{versionNodes[i].InnerText.Trim()}");
            }
            if (versionNodes != null)
            {
                foreach (var versionNode in versionNodes)
                {
                    var version = versionNode.InnerText.Trim();
                    var tagUrl = $"https://github.com/DaddyCalcifer/AloneSkyland/releases/tag/{version}";

                    var tagResponse = await client.GetStringAsync(tagUrl);
                    var tagDocument = new HtmlDocument();
                    tagDocument.LoadHtml(tagResponse);

                    var assetNode = tagDocument.DocumentNode.SelectSingleNode("//a[contains(@href, '.zip')]");
                    if (assetNode != null)
                    {
                        var downloadUrl = "https://github.com" + assetNode.GetAttributeValue("href", "").Trim();
                        releases[version] = downloadUrl;
                    }
                }
            }
        }

        return releases;
    }

    public async Task DownloadReleaseAsync(string url, string downloadPath, ProgressBar progressBar = null, Label statusLabel = null)
    {
        using (var webClient = new WebClient())
        {
            webClient.DownloadProgressChanged += (s, e) =>
            {
                if (progressBar != null)
                {
                    progressBar.Value = e.ProgressPercentage;
                }

                if (statusLabel != null)
                {
                    double downloadedMB = e.BytesReceived / 1024.0 / 1024.0;
                    double totalMB = e.TotalBytesToReceive / 1024.0 / 1024.0;
                    double speedKBps = e.BytesReceived / 1024.0 / 1024.0 / (e.ProgressPercentage / 100.0);
                    statusLabel.Content = $"{downloadedMB:F2} MB / {totalMB:F2} MB ({speedKBps:F2} MB/s)";
                }
            };

            webClient.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    System.Windows.MessageBox.Show($"Error downloading file: {e.Error.Message}");
                }
                else
                {
                    System.Windows.MessageBox.Show("Download completed.");
                }
            };

            await webClient.DownloadFileTaskAsync(new Uri(url), downloadPath);
        }
    }
}
