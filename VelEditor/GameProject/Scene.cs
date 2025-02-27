using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VelEditor.Components;
using VelEditor.Utilities;

namespace VelEditor.GameProject
{
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
        private readonly ObservableCollection<GameEntity> _gameEntityList = new ObservableCollection<GameEntity>();
        public ReadOnlyObservableCollection<GameEntity> GameEntityList { get; private set; }

        public ICommand AddGameEntityCommand { get; private set; }
        public ICommand RemoveGameEntityCommand { get; private set; }

        private void AddGameEntity(GameEntity gameEntity)
        {
            Debug.Assert(!_gameEntityList.Contains(gameEntity));
            _gameEntityList.Add(gameEntity);
        }
        private void RemoveGameEntity(GameEntity gameEntity)
        {
            Debug.Assert(_gameEntityList.Contains(gameEntity));
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

            AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                AddGameEntity(x);
                var index = _gameEntityList.Count - 1;
                Project.UndoRedoManager.Add(new UndoRedoAction(
                    () => RemoveGameEntity(x),
                    () => _gameEntityList.Insert(index, x),
                    $"Add {x.Name} in {Name}"
                    ));
            });
            RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                var sceneIndex = _gameEntityList.IndexOf(x);
                RemoveGameEntity(x);
                Project.UndoRedoManager.Add(new UndoRedoAction(
                    () => _gameEntityList.Insert(sceneIndex, x),
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
}