using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VelEditor.GameProject
{
    /// <summary>
    /// Interaction logic for OpenProjectView.xaml
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var item = projectListBox.ItemContainerGenerator
                .ContainerFromIndex(projectListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void OnListBoxItem_Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }

        private void OpenSelectedProject()
        {
            var project = OpenProject.Open(projectListBox.SelectedItem as ProjectData);
            bool dialogueResult = false;
            var win = Window.GetWindow(this);
            if (project != null)
            {
                dialogueResult = true;
                win.DataContext = project;
            }
            win.DialogResult = dialogueResult;
            win.Close();
        }
    }
}
