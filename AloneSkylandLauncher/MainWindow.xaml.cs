using AloneSkylandLauncher.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
            versionController = new VersionController();
            versionController.LoadVersions(versionBox).ConfigureAwait(false);
            loadLabel.Content = String.Empty;
        }

        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            await downloadGame();
        }
        async Task downloadGame(bool reinstall=false)
        {
            string selectedVersion = versionBox.SelectedItem as string;
            if(reinstall)
            {
                if (MessageBox.Show("Вы уверены что хотите переустановить игру?", "Установка", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }
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
        private void versionBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            bool isVersionInstalled = false;
            string selectedVersion = versionBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedVersion))
            {
                isVersionInstalled = versionController.isVersionInstalled(selectedVersion);
            }
            else return;
            if (isVersionInstalled)
            {
                PlayButton.IsEnabled = true;
                downloadButton.Content = "Переустановить";
            }
            else
            {
                PlayButton.IsEnabled = false;
                downloadButton.Content = "Загрузить";
            }
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
    }
}
