using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace VelEditor.Content
{
    class TextureDimensionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => int.TryParse((string)parameter, out var index) && (int)(value as TextureDimension?) == index;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => int.TryParse((string)parameter, out var index) ? (TextureDimension)index : TextureDimension.Texture2D;
    }

    /// <summary>
    /// Interaction logic for TextureImportSettingsView.xaml
    /// </summary>
    public partial class TextureImportSettingsView : UserControl
    {
        public TextureImportSettingsView()
        {
            InitializeComponent();
        }
    }
}
