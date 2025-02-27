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
