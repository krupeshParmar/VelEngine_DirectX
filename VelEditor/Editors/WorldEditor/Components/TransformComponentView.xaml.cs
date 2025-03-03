using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VelEditor.Components;
using VelEditor.GameProject;
using VelEditor.Utilities;

namespace VelEditor.Editors
{
    public partial class TransformComponentView : UserControl
    {
        private Action? _undoAction = null;
        private bool _propertyChanged = false;
        public TransformComponentView()
        {
            InitializeComponent();
            Loaded += OnTransformComponentViewLoaded;
        }

        private void OnTransformComponentViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnTransformComponentViewLoaded;
            (DataContext as MSTransform).PropertyChanged += (s, e) => _propertyChanged = true;
        }

        private Action GetPositionAction() => GetAction((x) => (x, x.Position), (x) => x.transform.Position = x.Item2);
        private Action GetRotationAction() => GetAction((x) => (x, x.Rotation), (x) => x.transform.Rotation = x.Item2);
        private Action GetScaleAction() => GetAction((x) => (x, x.Scale), (x) => x.transform.Scale = x.Item2);

        private void RecordAction(Action redoAction, string name)
        {
            if (_undoAction == null || redoAction == null || !_propertyChanged)
                return;

            _propertyChanged = false;
            Project.UndoRedoManager.Add(new UndoRedoAction(_undoAction, redoAction, name));
        }

        private Action? GetAction(Func<Transform, (Transform transform, Vector3)> selector,
            Action<(Transform transform, Vector3)> forEachAtion)
        {
            if (!(DataContext is MSTransform vm))
            {
                _propertyChanged = false;
                return null;
            }
            var selection = vm.SelectedComponents.Select(x=>selector(x)).ToList();
            var action = new Action(() =>
            {
                selection.ForEach(x=>forEachAtion(x));
                (GameEntityInspecter.Instance.DataContext as MSEntity)?.GetMSComponent<MSTransform>().Refresh();
            });
            return action;
        }

        private void OnPosition_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetPositionAction();
        }

        private void OnPosition_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs e)
        {
            RecordAction(GetPositionAction(), "Position changed");
        }

        private void OnPosition_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyChanged && _undoAction != null)
            {
                OnPosition_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }

        private void OnRotation_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetRotationAction();
        }

        private void OnRotation_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs e)
        {
            RecordAction(GetRotationAction(), "Rotation changed");
        }

        private void OnRotation_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyChanged && _undoAction != null)
            {
                OnRotation_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }

        private void OnScale_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetScaleAction();
        }

        private void OnScale_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs e)
        {
            RecordAction(GetScaleAction(), "Scale changed");
        }

        private void OnScale_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyChanged && _undoAction != null)
            {
                OnScale_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }
    }
}
