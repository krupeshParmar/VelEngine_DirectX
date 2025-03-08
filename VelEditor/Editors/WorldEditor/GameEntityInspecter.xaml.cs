using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VelEditor.Components;
using VelEditor.GameProject;
using VelEditor.Utilities;

namespace VelEditor.Editors
{
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }
    }
    /// <summary>
    /// Interaction logic for GameEntityInspector.xaml
    /// </summary>
    public partial class GameEntityInspecter : UserControl
    {
        private Action _undoAction;
        private string _propertyName;
        public static GameEntityInspecter Instance { get; private set; }
        public GameEntityInspecter()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
            DataContextChanged += (_, __) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetEnabledAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void OnName_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;
            _undoAction = GetRenameAction();
        }

        private void OnName_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyName == nameof(MSEntity.Name) && _undoAction != null) {
                Project.UndoRedoManager.Add(new UndoRedoAction(_undoAction, GetRenameAction(), "Rename game entity"));
                _propertyName = null;
            }
            _undoAction = null;
        }

        private void OnIsEnabledCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            var undoAction = GetEnabledAction();
            var vm = DataContext as MSEntity;
            vm.IsEnabled = (sender as CheckBox).IsChecked == true;
            var redoAction = GetEnabledAction();
            Project.UndoRedoManager.Add(new UndoRedoAction(
                undoAction, redoAction, vm.IsEnabled == true ? "Enable Game Entity" : "Disable Game Entity"
                ));
        }

        private void OnAddComponent_Button_PreviewMouseLBD(object sender, MouseButtonEventArgs e)
        {
            var menu = FindResource("addComponentMenu") as ContextMenu;
            var btn = sender as ToggleButton;
            btn.IsChecked = true;
            menu.Placement = PlacementMode.Bottom;
            menu.PlacementTarget = btn;
            menu.MinWidth = btn.ActualWidth;
            menu.IsOpen = true;
        }
        private void AddComponent(ComponentType componentType, object data)
        {
            var creationFunc = ComponentFactory.GetCreationFunction(componentType);
            var changedEntities = new List<(GameEntity entity, Component component)>();
            var vm = DataContext as MSEntity;
            foreach(var entity in vm.SelectedEntities)
            {
                var component = creationFunc(entity, data);
                if(entity.AddComponent(component))
                {
                    changedEntities.Add((entity,component));
                }
            }

            if(changedEntities.Any())
            {
                vm.Refresh();
                Project.UndoRedoManager.Add(new UndoRedoAction(
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.AddComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    $"Add {componentType} component"
                    ));
            }
        }

        private void OnAddScriptComponent(object sender, RoutedEventArgs e)
        {
            AddComponent(ComponentType.Script, (sender as MenuItem).Header.ToString());
        }
    }
}
