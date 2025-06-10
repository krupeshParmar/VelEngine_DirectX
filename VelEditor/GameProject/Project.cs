using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VelEditor.Components;
using VelEditor.DLLWrapper;
using VelEditor.GameDev;
using VelEditor.Utilities;

namespace VelEditor.GameProject
{

    [DataContract(Name = "Game")]
    class Project : ViewModelBase
    {
        public static string Extension => ".vel";
        [DataMember]
        public string Name { get; private set; } = "New Project";

        /// <summary>
        /// Gets the root folder that contains the current project.
        /// </summary>
        [DataMember]
        public string Path { get; private set; }
        /// <summary>
        /// Gets the full path of the current Vel project file, including its file name and extension.
        /// </summary>
        public string FullPath => $@"{Path}{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";
        public string ContentPath => $@"{Path}Content\";
        public string TempFolder => $@"{Path}.Vel\Temp\";

        private int _buildConfig;
        [DataMember]
        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if(_buildConfig != value)
                {
                    _buildConfig = value;
                    OnPropertyChanged(nameof(BuildConfig));
                }
            }
        }

        public BuildConfiguration StandAloneBuildConfig => BuildConfig == 0 ? BuildConfiguration.Debug : BuildConfiguration.Release;
        public BuildConfiguration DLLBuildConfig => BuildConfig == 0 ? BuildConfiguration.DebugEditor : BuildConfiguration.ReleaseEditor;

        private string[] _availableScripts;
        public string[] AvailableScripts
        {
            get => _availableScripts;
            private set
            {
                if(_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
                }
            }
        }

        [DataMember (Name = nameof(ScenesList))]
        private ObservableCollection<Scene> _scenesList = new();
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

        public static Project? Current => Application.Current.MainWindow?.DataContext as Project;

        public static UndoRedo UndoRedoManager { get; } = new UndoRedo();

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ICommand AddSceneCommand {  get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand DebugStartCommand { get; private set; }
        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }
        public ICommand DebugStopCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }

        private void SetCommands()
        {
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

            UndoCommand = new RelayCommand<object>(x => UndoRedoManager.Undo(), x => UndoRedoManager.UndoList.Any());
            RedoCommand = new RelayCommand<object>(x => UndoRedoManager.Redo(), x => UndoRedoManager.RedoList.Any());
            SaveCommand = new RelayCommand<object>(x => Save(this));
            BuildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDll(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStartCommand = new RelayCommand<object>(async x => await RunGame(true), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(async x => await RunGame(false), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStopCommand = new RelayCommand<object>(async x => await StopGame(), x => VisualStudio.IsDebugging());

            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
            OnPropertyChanged(nameof(BuildCommand));
        }

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
            UnloadGameCodeDll_Internal();
            Task.Run(VisualStudio.CloseVisualStudio);
            UndoRedoManager.Reset();
            Logger.Clear();
            DeleteTempFolder();
        }

        private void DeleteTempFolder()
        {
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }
        }

        private static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"{project.Name} saved to {project.FullPath}");
        }

        private void SaveToBinary()
        {
            var configName = VisualStudio.GetConfigurationName(StandAloneBuildConfig);
            var bin = $@"{Path}x64\{configName}\game.bin";

            using (var bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(ActiveScene.GameEntityList.Count);
                foreach(var entity in ActiveScene.GameEntityList)
                {
                    bw.Write(0);        // entity type reserved for later
                    bw.Write(entity.ComponentsList.Count);
                    foreach(var component in entity.ComponentsList)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WriteToBinaray(bw);
                    }
                }
            }
        }

        private async Task RunGame(bool debug)
        {
            await Task.Run(() => VisualStudio.BuildSolution(this, StandAloneBuildConfig, debug));

            while (!VisualStudio.BuildDone)
            {
                // Waiting for the build to finish
            }

            if (VisualStudio.BuildSucceeded)
            {
                SaveToBinary();
                await Task.Run(() => VisualStudio.Run(this, StandAloneBuildConfig, debug));
            }
        }

        private async Task StopGame() => await Task.Run(VisualStudio.Stop);

        private async Task BuildGameCodeDll(bool showVSWindow = true)
        {
            try
            {
                UnloadGameCodeDll_Internal();
                // Build the game code dll
                await Task.Run(() =>
                    {
                        VisualStudio.BuildSolution(this, DLLBuildConfig, showVSWindow);
                    }
                );

                // NOTE: This is too risky....
                while (!VisualStudio.BuildDone)
                {
                    // Waiting for the build to finish
                }

                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDll_Internal();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MessageType.Error, $"Failed to build the game solution: {ex.Message}");
            }
        }

        public void LoadGameCodeDll()
        {
            LoadGameCodeDll_Internal();
        }

        private void LoadGameCodeDll_Internal()
        {
            var configName = VisualStudio.GetConfigurationName(DLLBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";
            AvailableScripts = null;

            if(File.Exists(dll) && VelAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = VelAPI.GetScriptNames();
                ActiveScene.GameEntityList.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, "Game code DLL successfully loaded");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Failed to load game code DLL. Try to build the game project first!");
            }
        }

        private void UnloadGameCodeDll_Internal()
        {
            ActiveScene.GameEntityList.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = false);
            if(VelAPI.UnloadGameCodeDll() != 0)
            {
                Logger.Log(MessageType.Info, "Game code DLL unloaded.");
                AvailableScripts = null;
            }
        }

        [OnDeserialized]
        private async void OnDeserialized(StreamingContext context)
        {
            if(_scenesList != null)
            {
                ScenesList = new ReadOnlyObservableCollection<Scene>(_scenesList);
                OnPropertyChanged(nameof(ScenesList));
            }
            ActiveScene = _scenesList.FirstOrDefault(x => x.IsActive);
            Debug.Assert(ActiveScene != null);

            await BuildGameCodeDll(false);
            SetCommands();
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;
            Debug.Assert(File.Exists((Path + Name + Extension).ToLower()));
            OnDeserialized(new StreamingContext());
        }
    }
}
