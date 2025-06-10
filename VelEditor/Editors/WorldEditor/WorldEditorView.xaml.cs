using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VelEditor.Content;
using VelEditor.GameDev;
using VelEditor.GameProject;
using VelEditor.Utilities;

namespace VelEditor.Editors
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();
            Loaded += OnWorldEditorViewLoaded;
        }

        private void OnWorldEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorViewLoaded;
            var giWindow = new RenderSurfaceView();
            Focus();
        }

        private void OnNewScript_Button_Clicked(object sender, RoutedEventArgs e)
        {
            new NewScriptDialog().ShowDialog();
        }

        private void OnOpenVSProject_Button_Clicked(object sender, RoutedEventArgs e)
        {
           VisualStudio.OpenVisualStudio(Project.Current.Solution);
        }

        private void OnCreatePrimitiveMesh_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrimitiveMeshDialog();
            dlg.ShowDialog();
        }

        private void UnloadAndCloseAllWindows()
        {
            Project.Current?.Unload();

            var mainWindow = Application.Current.MainWindow;

            foreach (Window win in Application.Current.Windows)
            {
                if (win != mainWindow)
                {
                    win.DataContext = null;
                    win.Close();
                }
            }

            mainWindow.DataContext = null;
            mainWindow.Close();
        }

        private void OnNewProject(object sender, ExecutedRoutedEventArgs e)
        {
            ProjectBrowserDialogue.GotoNewProjectTab = true;
            UnloadAndCloseAllWindows();
        }

        private void OnOpenProject(object sender, ExecutedRoutedEventArgs e) => UnloadAndCloseAllWindows();

        private void OnEditorClose(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void OnContentBrowser_Loaded(object sender, RoutedEventArgs e)
            => OnContentBrowser_IsVisibleChanged(sender, default);

        private void OnContentBrowser_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is ContentBrowser contentBrowser &&
                string.IsNullOrEmpty(contentBrowser.SelectedFolder?.Trim()))
            {
                contentBrowser.SelectedFolder = contentBrowser.ContentFolder;
            }
        }
    }
}
