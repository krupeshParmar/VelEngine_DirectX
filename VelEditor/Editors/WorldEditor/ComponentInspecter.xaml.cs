using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace VelEditor.Editors
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
