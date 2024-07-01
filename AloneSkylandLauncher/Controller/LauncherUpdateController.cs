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
        public const string url = "https://github.com/DaddyCalcifer/AloneSkylandLauncher";
        private GitHubReleasesController _releasesController1 = new GitHubReleasesController(url);

        private readonly string _launcherPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public async Task<string> GetLatestLauncherVersionAsync()
        {
            try
            {
                var releases = await _releasesController1.GetLastReleaseAsync();

                return releases;
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
                var latestRelease = await GetLatestLauncherVersionAsync();

                if (latestRelease == null)
                {
                    MessageBox.Show("Не удалось найти последнюю версию лаунчера.");
                    return;
                }

                string downloadPath = Path.Combine(_launcherPath, "AloneSkylandLauncher_Update.zip");
                string extractPath = Path.Combine(_launcherPath, "UpdateTemp");

                using (var webClient = new WebClient())
                {
                    var downloadUrl = $"https://github.com/DaddyCalcifer/AloneSkylandLauncher/releases/download/{latestRelease}/launcher.zip";
                    await _releasesController1.DownloadReleaseAsync(downloadUrl,downloadPath);
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

                if (File.Exists(backupLauncherPath))
                {
                    File.Delete(backupLauncherPath);
                }

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
