using System.Windows;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Utils;
using PoESkillTree.ViewModels;

namespace PoESkillTree.Views
{
    /// <summary>
    /// Interaction logic for MainRibbon.xaml
    /// </summary>
    public partial class MainRibbon
    {
        public MainRibbon()
        {
            InitializeComponent();
        }

        private MainWindow MainWindow => (MainWindow) DataContext;

        private async void OpenSettings(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialogAsync(
                new SettingsMenuViewModel(MainWindow.PersistentData, DialogCoordinator.Instance, MainWindow.BuildsControlViewModel),
                new SettingsMenuWindow());
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ResetTree(object sender, RoutedEventArgs e)
        {
            MainWindow.ResetTree();
        }

        private async void CraftItem(object sender, RoutedEventArgs e)
        {
            await MainWindow.CraftItemAsync();
        }

        private async void CraftUnique(object sender, RoutedEventArgs e)
        {
            await MainWindow.CraftUniqueAsync();
        }

        private async void ImportCharacter(object sender, RoutedEventArgs e)
        {
            await MainWindow.ImportCharacterAsync();
        }

        private async void ImportStash(object sender, RoutedEventArgs e)
        {
            await MainWindow.ImportStashAsync();
        }

        private void CheckAllHighlightedNodes(object sender, RoutedEventArgs e)
        {
            MainWindow.Tree.CheckAllHighlightedNodes();
        }

        private void CrossAllHighlightedNodes(object sender, RoutedEventArgs e)
        {
            MainWindow.Tree.CrossAllHighlightedNodes();
        }

        private async void UntagAllNodes(object sender, RoutedEventArgs e)
        {
            var response = await MainWindow.ShowQuestionAsync(L10n.Message("Are you sure?"),
                title: L10n.Message("Untag All Skill Nodes"), image: MessageBoxImage.None);
            if (response == MessageBoxResult.Yes)
                MainWindow.Tree.UntagAllNodes();
        }

        private void UnhighlightAllNodes(object sender, RoutedEventArgs e)
        {
            MainWindow.Tree.UnhighlightAllNodes();
            MainWindow.ClearSearch();
        }

        private async void ScreenShot(object sender, RoutedEventArgs e)
        {
            await MainWindow.ScreenShotAsync();
        }

        private async void CreatePoeUrl(object sender, RoutedEventArgs e)
        {
            await MainWindow.DownloadPoeUrlAsync();
        }

        private async void RedownloadTreeAssets(object sender, RoutedEventArgs e)
        {
            await MainWindow.RedownloadTreeAssetsAsync();
        }

        private async void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            await MainWindow.CheckForUpdatesAsync();
        }

        private void OpenPoEWebsite(object sender, RoutedEventArgs e)
        {
            Util.OpenInBrowser("https://www.pathofexile.com/");
        }

        private void OpenWiki(object sender, RoutedEventArgs e)
        {
            Util.OpenInBrowser("http://pathofexile.gamepedia.com/");
        }

        private async void OpenHelp(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialogAsync(new CloseableViewModel(), new HelpWindow());
        }

        private async void OpenHotkeys(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialogAsync(new CloseableViewModel(), new HotkeysWindow());
        }

        private async void OpenAbout(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialogAsync(new CloseableViewModel(), new AboutWindow());
        }
    }
}
