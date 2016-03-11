using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;
using NuGet;
using OneClickNuget.Data;

namespace OneClickNuget.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly OpenFileDialog _openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            AddExtension = true,
            CheckFileExists = true,
            DefaultExt = ".csproj",
            Title = "Open a csproj file",
            Filter = "C# project file (*.csproj) | *.csproj"
        };

        private CancellationTokenSource _cancellationTokenSource = null;
        readonly NugetPackageManager _nugetPackageManager = new NugetPackageManager();
        private string _filePath = null;
        private Manifest _manifest = null;

        public MainWindow()
        {
            InitializeComponent();

            StatusProgressBar.Orientation = Orientation.Horizontal;
            StatusProgressBar.Minimum = 0;
            StatusProgressBar.Maximum = 100;

            _openFileDialog.FileOk += OpenFileDialogOnFileOk;

            RestoreState();
        }

        private void RestoreState()
        {
            ApiKeyTextBox.Text = StateManager.Get().ApiKey;
            ReleaseNotesTextBox.Text = StateManager.Get().LastReleaseNotes;
            AlwaysLoadFromInternetCheckBox.IsChecked = StateManager.Get().AlwaysLoadFromInternet;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            _openFileDialog.ShowDialog();
        }

        private async void OpenFileDialogOnFileOk(object sender, 
            CancelEventArgs cancelEventArgs)
        {
            using (new WaitCursor())
            {
                try
                {
                    ShowStatus("Please wait.... downloading package information"
                        , ContinuousProgressBarCommand.Start, ProgressResultType.Info);

                    _filePath = _openFileDialog.FileName;
                    PackageRetrieveOptions options = new PackageRetrieveOptions(_filePath, AlwaysLoadFromInternetCheckBox.IsChecked.GetValueOrDefault(false));
                    _manifest = await _nugetPackageManager.GetPackageInformation(options);
                    TextBlockProjectTitle.Text = $"{_manifest.Metadata.Id} {_manifest.Metadata.Version}";
                    VersionTextBox.Text = GetNewVersion();

                    dataGrid.ItemsSource = GetDependecies(options);

                    ShowStatus("Package information loaded", ContinuousProgressBarCommand.End, ProgressResultType.Info);
                }
                catch (Exception ex)
                {
                    ShowStatus("Could not load package information." + ex.Message,
                        ContinuousProgressBarCommand.End, ProgressResultType.Failure);
                }
            }
        }

        private IEnumerable<DependencyModel> GetDependecies(PackageRetrieveOptions options)
        {
            //todo:fix dependencyset issue.
            var nuspecDependencies = _manifest.Metadata
                        .DependencySets.First()
                        .Dependencies.Select(d => new DependencyModel
                        {
                            Id = d.Id,
                            Version = d.Version
                        }).ToList();

            var projetDependencies = _nugetPackageManager.GetProjectDependencies(options)
                .Select(d => new DependencyModel
                {
                    Id = d.Id,
                    Version = d.Version
                }).ToList();


            var finalDependencies = new List<DependencyModel>();

            finalDependencies.AddRange(projetDependencies);

            foreach (var nuspecDependency in nuspecDependencies)
            {
                if (!finalDependencies.Contains(nuspecDependency, new DependencyModelIdComparator()))
                {
                    finalDependencies.Add(nuspecDependency);
                }
            }

            return finalDependencies;
        }

        private string GetNewVersion()
        {
            var existingVersion = SemanticVersion.Parse(_manifest.Metadata.Version);

            var newVersion = new SemanticVersion(
                existingVersion.Version.Major,
                existingVersion.Version.Minor, 
                existingVersion.Version.Build + 1, 
                existingVersion.SpecialVersion);

            return newVersion.ToNormalizedString();
        }

        private async void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            SaveState();

            Progress<PackageProgress> progress = new Progress<PackageProgress>(ShowStatus);
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var dependencies = ((IEnumerable<DependencyModel>) dataGrid.ItemsSource)
                    .Select(x => new ManifestDependency {Id = x.Id, Version = x.Version})
                    .ToList();

                var publishOptions = new PublishOptions(
                    _filePath, 
                    VersionTextBox.Text, 
                    ReleaseNotesTextBox.Text, 
                    ApiKeyTextBox.Text,
                    dependencies,
                    AlwaysLoadFromInternetCheckBox.IsChecked.GetValueOrDefault(false));

                await _nugetPackageManager.Publish(publishOptions, progress, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                ShowStatus(new PackageProgress
                {
                    ProgressResultType = ProgressResultType.Failure,
                    Message = $"publish failed : {ex.Message}",
                });
            }
        }

        private void SaveState()
        {
            StateManager.Save(new ModelState
            {
                AlwaysLoadFromInternet = AlwaysLoadFromInternetCheckBox.IsChecked ?? false,
                ApiKey = ApiKeyTextBox.Text,
                LastReleaseNotes = ReleaseNotesTextBox.Text
            });
        }

        private void ShowStatus(PackageProgress progress)
        {
            StatusProgressBar.IsIndeterminate = false;

            StatusProgressBar.Foreground = progress.ProgressResultType == ProgressResultType.Failure ? 
                new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);

            if (progress.ProgressResultType != ProgressResultType.Failure)
            {
                StatusProgressBar.Value = progress.Percent;
            }

            StatusTextBox.Text = $"{progress.Percent}% : {progress.Message}";

        }

        private void ShowStatus(string status, 
            ContinuousProgressBarCommand progressBarCommand,
            ProgressResultType progressResultType)
        {
            StatusProgressBar.Value = progressResultType == ProgressResultType.Failure ? 100 : 0;

            StatusProgressBar.Foreground = progressResultType == ProgressResultType.Failure ?
                new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);

            StatusProgressBar.IsIndeterminate = progressBarCommand == ContinuousProgressBarCommand.Start;

            StatusTextBox.Text = status;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel(true);
        }

        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(DependencyModel.Id))
            {
                e.Column.IsReadOnly = true;
            }
        }
    }
}
