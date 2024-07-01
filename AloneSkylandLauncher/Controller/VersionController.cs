using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AloneSkylandLauncher.Controller
{
    public class VersionController
    {
        private GitHubReleasesController _releasesController;
        private string _appDataPath;
        public Dictionary<string, string> releases = new Dictionary<string, string>();
        MainWindow mw;

        public VersionController(MainWindow mw)
        {
            _releasesController = new GitHubReleasesController("https://github.com/DaddyCalcifer/AloneSkyland");
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".AloneSkyland");
            Directory.CreateDirectory(_appDataPath);
            this.mw = mw;
        }

        public async Task LoadVersions(ComboBox VersionComboBox)
        {
            var versions = await _releasesController.GetReleasesAsync();
            try
            {
                VersionComboBox.Items.Clear();
                foreach (var release in versions)
                {
                    VersionComboBox.Items.Add(release);
                }
                VersionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения версий с сервера: {ex.Message}");
            }
        }

        public void DeleteVersion(string version)
        {
            string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
            if (Directory.Exists(versionPath))
            {
                Directory.Delete(versionPath, true);  // Удаляем предыдущую версию, если она существует
                MessageBox.Show("Удаление завершено!");
            }
        }

        public async Task DownloadAndLaunchGame(string version, ProgressBar progressBar, Label statusLabel)
        {
            try
            {
                var downloadUrl = $"https://github.com/DaddyCalcifer/AloneSkyland/releases/download/{version}/game.zip";

                string downloadPath = Path.Combine(_appDataPath, $"{version.Replace(".", "")}.zip");

                await _releasesController.DownloadReleaseAsync(downloadUrl, downloadPath, progressBar, statusLabel);

                string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
                DeleteVersion(version);

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке игры: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void LaunchGame(string version, string args = "")
        {
            string versionPath = Path.Combine(_appDataPath, version.Replace(".", ""));
            string gameExecutable = Path.Combine(versionPath, "game\\Alone Skyland.exe");

            var startInfo = new ProcessStartInfo(gameExecutable)
            {
                UseShellExecute = true,
                WorkingDirectory = versionPath,
                Arguments = args,
                // Verb = "runas"  // Запуск от имени администратора
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
