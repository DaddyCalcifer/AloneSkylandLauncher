using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace AloneSkylandLauncher.Controller
{
    public class LauncherUpdateController
    {
        public const string GitName = "DaddyCalcifer";
        public const string GitRepos = "AloneSkylandLauncher";
        private GitHubClient _githubClient = new GitHubClient(new ProductHeaderValue("AloneSkylandLauncher"));

        private readonly string _launcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public async Task<string> GetLatestLauncherVersionAsync()
        {
            try
            {
                var releases = await _githubClient.Repository.Release.GetAll(GitName, GitRepos);
                var latestRelease = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

                return latestRelease?.TagName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении обновлений: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
            }
            return null;
        }

        private string GetCurrentLauncherVersion()
        {
            return LauncherPrefs.Version;
        }

        public async Task UpdateLauncherAsync()
        {
            string latestVersion = await GetLatestLauncherVersionAsync();
            string currentVersion = GetCurrentLauncherVersion();

            if (latestVersion != currentVersion){
                var releases = await _githubClient.Repository.Release.GetAll(GitName, GitRepos);
                var latestRelease = releases.FirstOrDefault(r => r.TagName == latestVersion);

                if (latestRelease == null)
                {
                    MessageBox.Show("Не удалось найти последнюю версию лаунчера.");
                    return;
                }

                var asset = latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"));
                if (asset == null)
                {
                    MessageBox.Show("Последняя версия лаунчера не доступна для скачивания.");
                    return;
                }

                string downloadPath = Path.Combine(_launcherPath, "AloneSkylandLauncher_Update.zip");
                string extractPath = Path.Combine(_launcherPath, "UpdateTemp");

                using (var webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), downloadPath);
                }

                // Распаковка архива
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                ZipFile.ExtractToDirectory(downloadPath, extractPath);

                // Замена текущего лаунчера новым
                string currentLauncherPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string backupLauncherPath = currentLauncherPath + ".bak";
                string newLauncherPath = Path.Combine(extractPath, "AloneSkylandLauncher.exe");

                try
                {
                    File.Move(currentLauncherPath, backupLauncherPath);
                    File.Move(newLauncherPath, currentLauncherPath);

                    // Перезапуск лаунчера
                    var startInfo = new ProcessStartInfo(currentLauncherPath)
                    {
                        UseShellExecute = true,
                        Verb = "runas"  // Запуск от имени администратора, если требуется
                    };
                    System.Diagnostics.Process.Start(startInfo);

                    // Удаление временных файлов
                    Directory.Delete(extractPath, true);
                    File.Delete(downloadPath);

                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении лаунчера: {ex.Message}");
                    if (File.Exists(backupLauncherPath))
                    {
                        File.Move(backupLauncherPath, currentLauncherPath);  // Восстановление старого лаунчера в случае ошибки
                    }
                }
            }
        }
    }
}
