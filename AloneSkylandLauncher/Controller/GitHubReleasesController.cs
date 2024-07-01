using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;

public class GitHubReleasesController
{
    public string GitHubReleasesUrl;

    public GitHubReleasesController(string url)
    {
        GitHubReleasesUrl = url + "/releases";
    }

    public async Task<List<string>> GetReleasesAsync()
    {
        var releases = new List<string>();

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
                var versionn = versionNodes[i].InnerText.Trim();
                Console.WriteLine($"{versionn}");
                releases.Add(versionn);
            }
        }
        return releases;
    }
    public async Task<string> GetLastReleaseAsync()
    {
        var releases = new List<string>();
        string version = string.Empty;
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetStringAsync(GitHubReleasesUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            var versionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class, 'ml-1 wb-break-all')]");
            version = versionNode.InnerText.Trim();
        }
        return version;
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

            await webClient.DownloadFileTaskAsync(new Uri(url), downloadPath);
        }
    }
}
