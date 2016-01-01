﻿using System;
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

        private string _filePath = null;

        public MainWindow()
        {
            InitializeComponent();
            _openFileDialog.FileOk += OpenFileDialogOnFileOk;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _openFileDialog.ShowDialog();
        }

        private void OpenFileDialogOnFileOk(object sender, 
            CancelEventArgs cancelEventArgs)
        {
            _filePath = _openFileDialog.FileName;
            TextBlockProjectTitle.Text = _openFileDialog.SafeFileName;
        }

        private async void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            var publisher = new NugetPackagePublisher(_filePath);

            Progress<PublishProgress> progress = new Progress<PublishProgress>(ShowStatus);

            try
            {
                await publisher.Publish(VersionTextBox.Text, ReleaseNotesTextBox.Text,
                    progress, CancellationToken.None);
            }
            catch (Exception ex)
            {
                //StatusTextBox.Text = $"published failed : {ex.Message}";
                MessageBox.Show($"published failed : {ex.Message}");
            }

            //MessageBox.Show("package published successfully");
        }

        private void ShowStatus(PublishProgress progress)
        {
            StatusTextBox.Text = $"{progress.Percent}% : {progress.Message}";
        }
    }
}