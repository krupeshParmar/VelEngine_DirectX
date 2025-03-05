using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VelEditor.GameDev;
using VelEditor.Utilities;

namespace VelEditor.GameProject
{
    [DataContract(Name = "Game")]
    class Project : ViewModelBase
    {
        public static string Extension { get; } = ".vel";
        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $@"{Path}{Name}{Extension}";
        public string SolutionName => $@"{Path}{Name}.sln";

        [DataMember (Name = "Scenes")]
        private ObservableCollection<Scene> _scenesList = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> ScenesList { get; private set; }

        private Scene _activeScene;

        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if(_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public static Project? Current => Application.Current.MainWindow.DataContext as Project;

        public static UndoRedo UndoRedoManager { get; } = new UndoRedo();

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ICommand AddSceneCommand {  get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }

        private void AddSceneInternal(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenesList.Add(new Scene(this, sceneName));
        }

        private void RemoveSceneInternal(Scene scene)
        {
            Debug.Assert(_scenesList.Contains(scene));
            _scenesList.Remove(scene);
        }

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }

        public void Unload()
        {
            VisualStudio.CloseVisualStudio();
            UndoRedoManager.Reset();
        }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"{project.Name} saved to {project.FullPath}");
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if(_scenesList != null)
            {
                ScenesList = new ReadOnlyObservableCollection<Scene>(_scenesList);
                OnPropertyChanged(nameof(ScenesList));
            }
            ActiveScene = ScenesList.FirstOrDefault(x => x.IsActive);

            AddSceneCommand = new RelayCommand<object>(x =>
            {
                AddSceneInternal($"New Scene {_scenesList.Count}");
                var newScene = _scenesList.Last();
                var sceneIndex = _scenesList.Count - 1;
                UndoRedoManager.Add(new UndoRedoAction(
                    () => RemoveSceneInternal(newScene),
                    () => _scenesList.Insert(sceneIndex, newScene),
                    $"Add {newScene.Name}"
                    ));
            });
            RemoveSceneCommand = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenesList.IndexOf(x);
                RemoveSceneInternal(x);
                UndoRedoManager.Add(new UndoRedoAction(
                    () => _scenesList.Insert(sceneIndex, x),
                    () => RemoveSceneInternal(x),
                    $"Remove {x.Name}"
                    ));
            }, x => !x.IsActive);

            UndoCommand = new RelayCommand<object>(x => UndoRedoManager.Undo());
            RedoCommand = new RelayCommand<object>(x => UndoRedoManager.Redo());
            SaveCommand = new RelayCommand<object>(x => Save(this));
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
        }
    }
}
