using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VelEditor.Content;
using VelEditor.GameProject;
using VelEditor.DLLWrapper;
using System.Runtime.InteropServices;

namespace VelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string VelPath { get; private set; }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            DefaultAssets.GenerateDefaultAssets();
            GetEnginePath();
            var initResult = VelAPI.InitializeEngine();
            if (initResult == EngineAPIStructs.EngineInitError.Succeeded)
            {
                OpenProjectBrowserDialog();
            }
            else
            {
                MessageBox.Show($"{initResult.GetDescription()}", "Engine initialization failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void GetEnginePath()
        {
            var enginePath = Environment.GetEnvironmentVariable("VEL_ENGINE", EnvironmentVariableTarget.User);
            if (enginePath == null || !Directory.Exists(Path.Combine(enginePath, @"VelEngine\VelAPI")))
            {
                var dlg = new EnginePathDialog();
                if(dlg.ShowDialog() == true)
                {
                    VelPath = dlg.VelPath;
                    Environment.SetEnvironmentVariable("VEL_ENGINE", VelPath.ToUpper(), EnvironmentVariableTarget.User);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                VelPath = enginePath;
            }
        }

        private void Shutdown()
        {
            Closing -= OnMainWindowClosing;
            Project.Current?.Unload();
            DataContext = null;
            ContentToolsAPI.ShutDownContentTools();
            VelAPI.ShutdownEngine();
        }

        private void OnMainWindowClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext == null)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Hide();
                OpenProjectBrowserDialog();
                if (DataContext != null)
                {
                    Application.Current.MainWindow.Show();
                }
            }
            else
            {
                Shutdown();
            }
        }

        private void OpenProjectBrowserDialog()
        {
            Project.Current?.Unload();
            var projectBrowser = new ProjectBrowserDialogue();
            if(projectBrowser.ShowDialog() == false || projectBrowser.DataContext == null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                var project = projectBrowser.DataContext as Project;
                Debug.Assert(project != null);
                DataContext = project;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnMainWindowLoaded;
            Closing += OnMainWindowClosing;
        }
    }
}