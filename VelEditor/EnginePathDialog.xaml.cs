using System;
using System.Collections.Generic;
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

namespace VelEditor
{
    /// <summary>
    /// Interaction logic for EnginePathDialog.xaml
    /// </summary>
    public partial class EnginePathDialog : Window
    {
        public string VelPath { get; private set; }
        public EnginePathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OnOkBtn_Click(object sender, RoutedEventArgs e)
        {
            var path = pathTextBox.Text;
            messageTextBlock.Text = string.Empty;
            VelPath = string.Empty;

            if (string.IsNullOrEmpty(path))
                messageTextBlock.Text = "Invalid path";

            else if(path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                messageTextBlock.Text = "Invalid character(s) used in path";

            else if(!Directory.Exists(Path.Combine(path, @"VelEngine\VelAPI\")))
                messageTextBlock.Text = "Unable to find Vel Engine at the specified path";
            
            if(string.IsNullOrEmpty(messageTextBlock.Text))
            {
                if (!Path.EndsInDirectorySeparator(path)) path += @"\";

                VelPath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
