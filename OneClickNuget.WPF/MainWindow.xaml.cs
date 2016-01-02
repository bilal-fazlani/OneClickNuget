using System;
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
        readonly NugetPackageManager _publisher = new NugetPackageManager();
        private string _filePath = null;
        private Manifest _manifest = null;

        public MainWindow()
        {
            InitializeComponent();
            _openFileDialog.FileOk += OpenFileDialogOnFileOk;
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
                ShowStatus("Please wait.... downloading package information");
                _filePath = _openFileDialog.FileName;
                PackageRetrieveOptions options = new PackageRetrieveOptions(_filePath);
                _manifest = await _publisher.GetPackageInformation(options);
                TextBlockProjectTitle.Text = _manifest.Metadata.Id;
                VersionTextBox.Text = _manifest.Metadata.Version;
                ShowStatus("Package Information loaded");
            }
        }

        private async void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            Progress<PublishProgress> progress = new Progress<PublishProgress>(ShowStatus);
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var publishOptions = new PublishOptions(_filePath, VersionTextBox.Text, ReleaseNotesTextBox.Text);
                await _publisher.Publish(publishOptions, progress, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Publish failed", 
                    MessageBoxButton.OK, MessageBoxImage.Error);

                StatusTextBox.Text = $"publish failed : {ex.Message}";
            }
        }

        private void ShowStatus(PublishProgress progress)
        {
            StatusTextBox.Text = $"{progress.Percent}% : {progress.Message}";
        }

        private void ShowStatus(string status)
        {
            StatusTextBox.Text = status;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }
    }
}
