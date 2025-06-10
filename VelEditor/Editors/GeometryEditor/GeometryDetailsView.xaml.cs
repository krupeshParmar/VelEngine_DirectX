using System.Windows;
using System.Windows.Controls;

namespace VelEditor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryDetailsView.xaml
    /// </summary>
    public partial class GeometryDetailsView : UserControl
    {
        public GeometryDetailsView()
        {
            InitializeComponent();
        }
        private void OnHighlight_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as GeometryEditor;
            foreach (var m in vm.mMeshRenderer.Meshes)
            {
                m.IsHighlighted = false;
            }

            var checkBox = sender as CheckBox;
            (checkBox.DataContext as MeshRendererVertexData).IsHighlighted = checkBox.IsChecked == true;
        }

        private void OnIsolate_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as GeometryEditor;
            foreach (var m in vm.mMeshRenderer.Meshes)
            {
                m.IsIsolated = false;
            }

            var checkBox = sender as CheckBox;
            var mesh = checkBox.DataContext as MeshRendererVertexData;
            mesh.IsIsolated = checkBox.IsChecked == true;

            if (Tag is GeometryView geometryView)
            {
                geometryView.SetGeometry(mesh.IsIsolated ? vm.mMeshRenderer.Meshes.IndexOf(mesh) : -1);
            }

        }
    }
}
