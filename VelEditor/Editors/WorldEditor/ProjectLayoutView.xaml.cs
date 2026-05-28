using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VelEditor.Components;
using VelEditor.GameProject;
using VelEditor.Utilities;
using VelEditor.Content;

namespace VelEditor.Editors;

/// <summary>
/// Interaction logic for ProjectLayoutView.xaml
/// </summary>
public partial class ProjectLayoutView : UserControl
{
    private List<int> _previousSelectedIndices = [];
    public ProjectLayoutView()
    {
        InitializeComponent();
    }
    private void OnRenameScene_Button_Click(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)(sender as Button).Tag;
        textBox.Visibility = Visibility.Visible;
        textBox.Focus();
    }

    private void OnAddGameEntity_Button_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var vm = btn.DataContext as Scene;
        vm.AddGameEntityCommand.Execute(new GameEntity(vm) { Name = "Empty Game Object"});                                         
    }

    private void OnGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listbox = sender as ListBox;
        var vm = listbox.DataContext as Scene;

        var newSelection = listbox.SelectedItems.Cast<GameEntity>().ToList();
        var newSelectionIndices = newSelection.Select(x => vm.GameEntityList.IndexOf(x)).ToList();
        var previousSelectedIndices = _previousSelectedIndices.ToList();
        _previousSelectedIndices = [.. newSelectionIndices];

        Project.UndoRedoManager.Add(new UndoRedoAction(
            () =>       // undo action
            {
                listbox.UnselectAll();
                previousSelectedIndices.ForEach(x => (listbox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
            },
            () =>      // redo action
            {
                listbox.UnselectAll();
                newSelection.ForEach(x => (listbox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
            },
            "Selection changed"
            ));

        MsGameEntity msEntity = null;
        if(newSelection.Any())
        {
            msEntity = new MsGameEntity(newSelection);
        }
        GameEntityInspecter.Instance.DataContext = msEntity;
    }

    private async void OnGameEntities_ListBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && sender is FrameworkElement { DataContext: Scene scene } && scene.IsActive)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var fileList = files?
                .Where(x => Path.GetExtension(x).ToLower() == Asset.AssetFileExtension && Asset.TryGetAssetInfo(x)?.Type == AssetType.Mesh)
                .ToList();
            List<GameEntity> entities = [];

            await Task.Run(() =>
            {
                foreach (var file in fileList)
                {
                    Debug.Assert(!string.IsNullOrEmpty(file.Trim()));
                    var assetInfo = Asset.TryGetAssetInfo(file);
                    if (assetInfo != null)
                    {
                        var entity = new GameEntity(scene) { Name = assetInfo.FileName.Trim() };
                        // NOTE: adding an entity to an active scene will automatically set its IsActive to true.
                        //       However, setting it to true here will create and upload entity resources without blocking the UI thread.
                        //       Also we can't add components to inactive entities, since they don't exist in the engine.
                        entity.IsActive = true;
                        entity.AddComponent(new Components.Geometry(entity, assetInfo));
                        entities.Add(entity);
                    }
                }
            });

            entities.ForEach(entity => scene.AddGameEntityCommand.Execute(entity));
        }
    }

    private void OnGameEntities_ListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // TODO: remove entities.
        }
    }

    private void OnGameEntities_ListBox_Loaded(object sender, RoutedEventArgs e)
    {
        var gameEntityListBox = sender as ListBox;
        if (gameEntityListBox.IsEnabled)
        {
            if (gameEntityListBox.Items.Count > 0)
            {
                gameEntityListBox.SelectedIndex = 0;
                var item = gameEntityListBox.ItemContainerGenerator
                    .ContainerFromIndex(gameEntityListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            }
            else
            {
                GameEntityInspecter.Instance.DataContext = null;
            }
        }
    }
}
