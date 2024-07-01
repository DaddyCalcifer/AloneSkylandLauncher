using AloneSkylandLauncher.Controller;
using HtmlAgilityPack;
using Markdig.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AloneSkylandLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VersionController versionController;
        LauncherUpdateController launcherUpdateController;
        private readonly HttpClient _httpClient = new HttpClient();
        bool currentVersionInstalled = false;
        List<string> releases;
        public MainWindow()
        {
            InitializeComponent();
            this.Width = 700;
            this.Height = 470;
            releases = new List<string>();
            versionController = new VersionController(this);
            launcherUpdateController = new LauncherUpdateController();

            //CheckForUpdates();
            loadLabel.Content = String.Empty;
            LoadMarkdownFile();
        }

        private async void LoadMarkdownFile()
        {
            string markdownContent = await DownloadMarkdownFileAsync("https://raw.githubusercontent.com/DaddyCalcifer/AloneSkyland/main/README.md");
            InfoPanel.Markdown = markdownContent;
        }

        private async Task<string> DownloadMarkdownFileAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {e.Message}");
                return string.Empty;
            }
        }

        async void CheckForUpdates()
        {
            if (launcherUpdateController != null)
            {
                var last_ver = await launcherUpdateController.GetLatestLauncherVersionAsync();
                if (last_ver != LauncherPrefs.Version)
                {
                    if(MessageBox.Show(
                        "Найдена новая версия лаунчера.\nЗагрузить и установить обновление?",
                        "Обновление лаунчера",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await launcherUpdateController.UpdateLauncherAsync();
                }
            }
        }

        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            await downloadGame();
        }
        async Task downloadGame()
        {
            string selectedVersion = versionBox.SelectedItem as string;
            if (currentVersionInstalled)
            {
                if (MessageBox.Show("Вы уверены что хотите удалить игру?" +
                    "\nИгровые сохранения тоже будут удалены!",
                    "Удаление",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    versionController.DeleteVersion(selectedVersion);
            }
            else
            {
                if (string.IsNullOrEmpty(selectedVersion))
                {
                    MessageBox.Show("Ошибка: Не выбрана версия",
                        "Ошибка загрузки",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                await versionController.DownloadAndLaunchGame(selectedVersion, loadBar, loadLabel);
            }
            updatePlayPanel();
        }
        public void updatePlayPanel()
        {
            string selectedVersion = versionBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVersion))
            {
                currentVersionInstalled = versionController.isVersionInstalled(selectedVersion);
            }
            else return;
            if (currentVersionInstalled)
            {
                PlayButton.IsEnabled = true;
                downloadButton.Content = "Удалить";
            }
            else
            {
                PlayButton.IsEnabled = false;
                downloadButton.Content = "Загрузить";
            }
        }
        private void versionBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            updatePlayPanel();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedVersion = versionBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVersion))
            {
                versionController.LaunchGame(selectedVersion);
            }
            else MessageBox.Show("Ошибка при запуске игры: Не выбрана версия",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await versionController.LoadVersions(versionBox);
        }
    }
}
