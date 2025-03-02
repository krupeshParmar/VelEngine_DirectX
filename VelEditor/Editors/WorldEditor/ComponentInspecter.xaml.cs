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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VelEditor.Editors.WorldEditor
{
    [ContentProperty("ComponentContent")]
    public partial class ComponentInspecter : UserControl
    {
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ComponentInspecter));

        public FrameworkElement ComponentContent
        {
            get { return (FrameworkElement)GetValue(ComponentContentProperty); }
            set { SetValue(ComponentContentProperty, value); }
        }

        public static readonly DependencyProperty ComponentContentProperty =
            DependencyProperty.Register(nameof(ComponentContent), typeof(FrameworkElement), typeof(ComponentInspecter));



        public ComponentInspecter()
        {
            InitializeComponent();
        }
    }
}
