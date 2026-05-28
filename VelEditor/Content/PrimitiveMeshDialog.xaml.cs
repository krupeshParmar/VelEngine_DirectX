using Microsoft.Win32;
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
using System.Windows.Shapes;
using VelEditor.ContentToolsAPIStruct;
using VelEditor.DLLWrapper;
using VelEditor.Editors;
using VelEditor.Utilities.Controls;
using VelEditor.GameProject;
using System.Diagnostics;

namespace VelEditor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        private static readonly List<ImageBrush> _texturesList = [];
        private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();

        private void OnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();
        private void OnTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdatePrimitive();
        private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

        private void OnTextureCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            Brush brush = Brushes.White;
            if ((sender as CheckBox).IsChecked == true)
            {
                brush = _texturesList[(int)primitiveTypeComboBox.SelectedItem];
            }

            var vm = DataContext as GeometryEditor;
            foreach (var mesh in vm.mMeshRenderer.Meshes)
            {
                mesh.Diffuse = brush;
            }
        }

        private int Value(Slider slider) => (int)slider.Value;

        private float Value(TextBox textBox, float min)
        {
            float.TryParse(textBox.Text, out float result);
            return Math.Max(result, min);
        }

        private int Value(TextBox textBox, int min)
        {
            int.TryParse(textBox.Text, out int result);
            return Math.Max(result, min);
        }

        private float Value(ScalarBox scalarBox, float min)
        {
            float.TryParse(scalarBox.Value, out var result);
            return Math.Max(result, min);
        }

        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;

            var primitiveType = (PrimitiveMeshType)primitiveTypeComboBox.SelectedItem;
            var info = new PrimitiveInitInfo() {  Type = primitiveType };
            var smoothingAngle = 0;

            switch (primitiveType)
            {
                case PrimitiveMeshType.Plane:
                    {
                        info.SegmentsX = Value(xSliderPlane);
                        info.SegmentsZ = Value(zSliderPlane);
                        info.Size.X = Value(widthScalarBoxPlane,0.001f);
                        info.Size.Z = Value(lengthScalarBoxPlane,0.001f);
                    }
                    break;
                case PrimitiveMeshType.Cube:
                    {
                        info.SegmentsX = Value(xSliderCube);
                        info.SegmentsY = Value(ySliderCube);
                        info.SegmentsZ = Value(zSliderCube);
                        info.Size.X = Value(xTextBoxCube, 0.001f);
                        info.Size.Y = Value(yTextBoxCube, 0.001f);
                        info.Size.Z = Value(zTextBoxCube, 0.001f);
                        info.LOD = Value(lodTextBoxCube, 0);
                    }
                    break;
                case PrimitiveMeshType.UVSphere:
                    {
                        info.SegmentsX = Value(xSliderUVSphere);
                        info.SegmentsY = Value(ySliderUVSphere);
                        info.Size.X = Value(xScalarBoxUVSphere, 0.001f);
                        info.Size.Y = Value(yScalarBoxUVSphere, 0.001f);
                        info.Size.Z = Value(zScalarBoxUVSphere, 0.001f);
                        smoothingAngle = Value(angleSliderUVSphere);
                    }
                    break;
                /*case PrimitiveMeshType.ICOSphere:
                    break;
                case PrimitiveMeshType.Cylinder:
                    break;
                case PrimitiveMeshType.Capsule:
                    break;*/
                default:
                    return;
            }
            var geometry = new Geometry();
            geometry.ImportSettings.SmoothingAngle = smoothingAngle;
            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);
            (DataContext as GeometryEditor).SetAsset(geometry);
            OnTextureCheckbox_Clicked(textureCheckbox, null);
        }

        private static void LoadTextures()
        {
            var uris = new List<Uri>
            {
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/plane_deformation.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/CubeCheckermap.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/plane_deformation.png"),
            };

            _texturesList.Clear();
            foreach (var uri in uris)
            {
                var resource = Application.GetResourceStream(uri);
                if (resource == null) continue;
                using var reader = new BinaryReader(resource.Stream);
                var data = reader.ReadBytes((int)resource.Stream.Length);
                var imageSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);
                imageSource.Freeze();
                var brush = new ImageBrush(imageSource);
                brush.Transform = new ScaleTransform(1, -1, 0.5, 0.5);
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Freeze();
                _texturesList.Add(brush);
            }
        }

        private void OnSave_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveDialog();
            if (dlg.ShowDialog() == true)
            {
                Debug.Assert( !string.IsNullOrEmpty(dlg.SaveFilePath));
                var asset = (DataContext as IAssetEditor).Asset;
                Debug.Assert(asset != null);
                asset.FullPath = dlg.SaveFilePath;
                asset.SaveAsset();
            }
        }

        static PrimitiveMeshDialog()
        {
            LoadTextures();
        }

        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdatePrimitive();
        }
    }
}
