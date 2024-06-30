using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AloneSkylandLauncher.Controller
{
    public class VersionController
    {
        public const string GitName = "DaddyCalcifer", GitRepos = "AloneSkyland";
        private GitHubClient _githubClient;
        private string _appDataPath;

        public VersionController()
        {
            _githubClient = new GitHubClient(new ProductHeaderValue("AloneSkylanLauncher"));
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".AloneSkyland");
            Directory.CreateDirectory(_appDataPath);
        }

        public async Task LoadVersions(ComboBox VersionComboBox)
        {
            try
            {
                var releases = await _githubClient.Repository.Release.GetAll(GitName, GitRepos);
                VersionComboBox.ItemsSource = releases.Select(r => r.TagName).ToList();
                VersionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения версий с сервера: {ex.Message}");
            }
        }
        public async Task DownloadAndLaunchGame(string version, ProgressBar progressBar, System.Windows.Controls.Label statusLabel)
        {
            try
            {
                var releases = await _githubClient.Repository.Release.GetAll(GitName, GitRepos);
                var selectedRelease = releases.FirstOrDefault(r => r.TagName == version);

                if (selectedRelease == null)
                {
                    MessageBox.Show("Выбранная версия не найдена");
                    return;
                }

                var asset = selectedRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"));
                if (asset == null)
                {
                    MessageBox.Show("Версия не доступна для скачивания.");
                    return;
                }

                string downloadPath = Path.Combine(_appDataPath, $"{version.Replace(".", "")}.zip");

                using (var webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                        double downloadedMB = e.BytesReceived / 1024.0 / 1024.0;
                        double totalMB = e.TotalBytesToReceive / 1024.0 / 1024.0;
                        double speedKBps = e.BytesReceived / 1024.0 / 1024.0 / (e.ProgressPercentage / 100.0);
                        statusLabel.Content = $"{downloadedMB:F2} MB / {totalMB:F2} MB ({speedKBps:F2} MB/s)";
                    };

                    webClient.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            MessageBox.Show($"Ошибка при загрузке файла: {e.Error.Message}");
                            return;
                        }

                        // Распаковка архива после завершения загрузки
                        string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
                        if (Directory.Exists(versionPath))
                        {
                            Directory.Delete(versionPath, true);  // Удаляем предыдущую версию, если она существует
                        }

                        statusLabel.Content = "Распаковка файлов...";
                        System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, versionPath);
                        statusLabel.Content = "Распаковка завершена.";
                        File.Delete(downloadPath);

                        string gameExecutable = Path.Combine(versionPath, "game\\Alone Skyland.exe");
                        if (!File.Exists(gameExecutable))
                        {
                            MessageBox.Show("Файлы игры повреждены!",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                            return;
                        }
                        statusLabel.Content = "Загрузка завершена.";
                        MessageBox.Show("Загрузка завершена.");
                    };

                    // Асинхронная загрузка файла
                    await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), downloadPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке игры: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        public void LaunchGame(string version, string args="")
        {
            string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
            string gameExecutable = Path.Combine(versionPath, "game\\Alone Skyland.exe");

            var startInfo = new ProcessStartInfo(gameExecutable)
            {
                UseShellExecute = true,
                WorkingDirectory = versionPath,
                Arguments = args,
                //Verb = "runas"  // Запуск от имени администратора
            };

            try
            {
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        public bool isVersionInstalled(string version)
        {
            string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
            string gameExecutable = Path.Combine(versionPath, "game\\Alone Skyland.exe");
            return File.Exists(gameExecutable);
        }
    }
}
