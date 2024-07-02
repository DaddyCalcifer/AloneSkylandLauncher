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
using MaterialDesignThemes.Wpf;
using AloneSkylandLauncher.Model;

namespace AloneSkylandLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VersionController versionController;
        LauncherUpdateController launcherUpdateController;
        ProfileManager profileManager;
        private readonly HttpClient _httpClient = new HttpClient();
        bool currentVersionInstalled = false;
        List<string> releases;
        public MainWindow()
        {
            InitializeComponent();
            //this.Height = 470;
            //this.Width = 700;
            this.Title = $"AloneSkyland Launcher {LauncherPrefs.Version}";
            releases = new List<string>();
            versionController = new VersionController(this);
            launcherUpdateController = new LauncherUpdateController();
            profileManager = new ProfileManager(profileBox);

            loadBar.Visibility = Visibility.Hidden; 
            loadBar.Visibility = Visibility.Hidden;

            CheckForUpdates();
            loadLabel.Content = String.Empty;
            LoadMarkdownFile();
        }

        private async void LoadMarkdownFile()
        {
            InfoPanel.Markdown = string.Empty;
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
            string selectedProfile = profileBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVersion))
            {
                currentVersionInstalled = versionController.isVersionInstalled(selectedVersion);
            }
            else return;
            if (currentVersionInstalled)
            {
                PlayButton.IsEnabled = true;
                profileBox.IsEnabled = true;
                downloadButton.Content = "Удалить";
                downloadButton.Background = System.Windows.Media.Brushes.Red;
            }
            else
            {
                PlayButton.IsEnabled = false;
                profileBox.IsEnabled = false;
                downloadButton.Content = "Загрузить";
                downloadButton.Background = System.Windows.Media.Brushes.Green;
            }
        }
        private void versionBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            updatePlayPanel();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedVersion = versionBox.SelectedItem as string;
            string selectedProfile = profileBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVersion) || !string.IsNullOrEmpty(selectedProfile))
            {
                versionController.LaunchGame(selectedVersion,selectedProfile);
            }
            else MessageBox.Show("Ошибка при запуске игры: Не выбрана версия или игровой профиль",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await versionController.LoadVersions(versionBox);
            loadLData();
        }
        void loadLData()
        {
            var ldata = LauncherData.Load("launcher.config");
            if (ldata != null)
            {
                profileBox.Items.Clear();
                versionBox.SelectedItem = ldata.lastVersion;
                ProfileManager.profiles = ldata.profiles;
                profileManager.initProfiles();
                foreach (var profile in ldata.profiles)
                {
                    profileBox.Items.Add(profile);
                }
                updateProfilesUI();
                profileBox.SelectedItem = ldata.lastProfile;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveLData();
        }
        void saveLData()
        {
            string selectedVersion = versionBox.SelectedItem as string;
            string selectedProfile = profileBox.SelectedItem as string;
            LauncherData launcherData = new LauncherData();
            launcherData.lastVersion = selectedVersion;
            launcherData.lastProfile = selectedProfile;
            launcherData.profiles = ProfileManager.profiles;
            launcherData.Save("launcher.config");
        }

        void updateProfilesUI()
        {
            ProfileListBox.Items.Clear();
            foreach(var profile in ProfileManager.profiles)
            {
                ProfileListBox.Items.Add(profile);
            }
        }
        void updateStatsUI(WorldData data)
        {
            if (data == null)
            {
                statsPanel.Visibility = Visibility.Hidden;
                return;
            }
            statsPanel.Visibility = Visibility.Visible;
            statsVersion.Content = $"Версия игры: {data.version}";
            HealthProgressBar.Maximum = data.maxHP;
            HealthProgressBar.Value = data.hp;
            HungerProgressBar.Maximum = data.maxHunger;
            HungerProgressBar.Value = data.hunger;
            ThirstProgressBar.Maximum = data.maxThist;
            ThirstProgressBar.Value = data.thist;
            if (data.Lvl != 0)
            {
                CraftingLevelLabel.Content = $"Уровень:  {data.Lvl}\tОпыт: {Math.Round(data.exp)}";
                LearningPointsLabel.Content = $"Очки изучения: {data.learnPTS}";
            }
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileName = NewProfileTextBox.Text.Trim();
            if (profileName == null) return;
            profileManager.addProfile(profileName);
            updateProfilesUI();
            NewProfileTextBox.Text = string.Empty;
            ProfileListBox.SelectedItem = profileName;
            saveLData();
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileListBox.SelectedItem == null) return;
            profileManager.deleteProfile(ProfileListBox.SelectedItem as string);
            updateProfilesUI();
            statsPanel.Visibility = Visibility.Hidden;
            saveLData();
        }

        private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = WorldData.Load(ProfileManager.dataPath + ProfileListBox.SelectedItem as string + "\\save.asl");
            updateStatsUI(data);
        }
    }
}
