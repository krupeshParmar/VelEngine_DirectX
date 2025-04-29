using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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

        private void OnNewProject(object sender, ExecutedRoutedEventArgs e)
        {
            ProjectBrowserDialogue.GotoNewProjectTab = true;
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void OnOpenProject(object sender, ExecutedRoutedEventArgs e)
        {
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void OnEditorClose(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
