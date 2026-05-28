using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using VelEditor.Components;
using VelEditor.Utilities;

namespace VelEditor.GameProject;

[DataContract]
class Scene : ViewModelBase
{
    private string _name;
    [DataMember]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    [DataMember]
    public Project Project { get; private set; }

    private bool _isActive;

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

    }

    [DataMember(Name = nameof(GameEntityList))]
    private readonly ObservableCollection<GameEntity> _gameEntityList = [];
    public ReadOnlyObservableCollection<GameEntity> GameEntityList { get; private set; }

    public ICommand RenameCommand { get; private set; }

    public ICommand AddGameEntityCommand { get; private set; }
    public ICommand RemoveGameEntityCommand { get; private set; }

    private void SetActiveGameEntities(bool isActive)
    {
        foreach (var entity in _gameEntityList)
        {
            entity.IsActive = isActive;
        }
    }

    private void AddGameEntity(GameEntity gameEntity, int index = -1)
    {
        Debug.Assert(!_gameEntityList.Contains(gameEntity));
        gameEntity.IsActive = IsActive;
        if(index == -1)
        {
            _gameEntityList.Add(gameEntity);
        }
        else
        {
            _gameEntityList.Insert(index, gameEntity);
        }
    }
    private void RemoveGameEntity(GameEntity gameEntity)
    {
        Debug.Assert(_gameEntityList.Contains(gameEntity));
        gameEntity.IsActive = false;
        _gameEntityList.Remove(gameEntity);
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_gameEntityList != null)
        {
            GameEntityList = new ReadOnlyObservableCollection<GameEntity>(_gameEntityList);
            OnPropertyChanged(nameof(GameEntityList));
        }

        foreach(var gameEntity in _gameEntityList)
        {
            gameEntity.IsActive = IsActive;
        }

        RenameCommand = new RelayCommand<string>(x =>
        {
            var oldName = _name;
            Name = x;

            Project.UndoRedoManager.Add(new UndoRedoAction(nameof(Name), this,
                oldName, x, $"Rename scene '{oldName}' to '{x}'"));
        }, x => x != _name);

        AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
        {
            AddGameEntity(x);
            var index = _gameEntityList.Count - 1;
            Project.UndoRedoManager.Add(new UndoRedoAction(
                () => RemoveGameEntity(x),
                () => AddGameEntity(x, index),
                $"Add {x.Name} in {Name}"
                ));
        });
        RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
        {
            var index = _gameEntityList.IndexOf(x);
            RemoveGameEntity(x);
            Project.UndoRedoManager.Add(new UndoRedoAction(
                () => AddGameEntity(x, index),
                () => RemoveGameEntity(x),
                $"Remove {x.Name} from {Name}"
                ));
        });
    }

    public Scene(Project project, string name)
    {
        Debug.Assert(project != null);
        Project = project;
        Name = name;
        OnDeserialized(new StreamingContext());
    }
}