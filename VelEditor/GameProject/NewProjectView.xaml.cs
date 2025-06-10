using System.Windows;
using System.Windows.Controls;

namespace VelEditor.GameProject
{
    /// <summary>
    /// Interaction logic for NewProjectView.xaml
    /// </summary>
    public partial class NewProjectView : UserControl
    {
        public NewProjectView()
        {
            InitializeComponent();
        }

        private void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as CreateNewProject;
            var projectPath = vm.CreateProject(templateListBox.SelectedItem as ProjectTemplate);
            bool dialogueResult = false;
            var win = Window.GetWindow(this);
            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogueResult = true;
                var project = OpenProject.Open(new ProjectData() { ProjectName = vm.ProjectName, ProjectPath = projectPath});
                win.DataContext = project;
            }
            win.DialogResult = dialogueResult;
            win.Close();
        }
    }
}
