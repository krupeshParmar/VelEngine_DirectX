using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VelEditor.Utilities.Controls
{
    public enum EVectorType
    { 
        Vector2, Vector3, Vector4,
    }

    class VectorBox : Control
    {
        public string X
        {
            get => (string)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(string), typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Y
        {
            get => (string)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(nameof(Y), typeof(string), typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Z
        {
            get => (string)GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }
        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register(nameof(Z), typeof(string), typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string W
        {
            get => (string)GetValue(WProperty);
            set => SetValue(WProperty, value);
        }
        public static readonly DependencyProperty WProperty =
            DependencyProperty.Register(nameof(W), typeof(string), typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Multiplier
        {
            get => (double)GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }
        public static readonly DependencyProperty MultiplierProperty =
            DependencyProperty.Register(nameof(Multiplier), typeof(double), typeof(VectorBox),
                new PropertyMetadata(1.0));

        public EVectorType VectorType
        {
            get => (EVectorType)GetValue(VectorTypeProperty);
            set => SetValue(VectorTypeProperty, value);
        }
        public static readonly DependencyProperty VectorTypeProperty =
            DependencyProperty.Register(nameof(VectorType), typeof(EVectorType), typeof(VectorBox),
                new PropertyMetadata(EVectorType.Vector3));
        public Orientation orientation
        {
            get => (Orientation)GetValue(orientationProperty);
            set => SetValue(orientationProperty, value);
        }
        public static readonly DependencyProperty orientationProperty =
            DependencyProperty.Register(nameof(orientation), typeof(Orientation), typeof(VectorBox),
                new PropertyMetadata(Orientation.Horizontal));


        static VectorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VectorBox), new FrameworkPropertyMetadata(typeof(VectorBox)));
        }
    }
}
