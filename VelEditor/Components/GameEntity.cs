using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VelEditor.DLLWrapper;
using VelEditor.GameProject;
using VelEditor.Utilities;

namespace VelEditor.Components
{
    [DataContract]
    [KnownType(typeof(Transform))]
    [KnownType(typeof(Script))]
    [KnownType(typeof(Geometry))]
    class GameEntity : ViewModelBase
    {
        private IdType _entityId = ID.INVALID_ID;

        public IdType EntityId
        {
            get => _entityId;
            set
            {
                if (_entityId != value)
                {
                    _entityId = value;
                    OnPropertyChanged(nameof(EntityId));
                }
            }
        }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if(_isActive)
                    {
                        _componentsList.ToList().ForEach(x => x.Load());
                        EntityId = VelAPI.EntityAPI.CreateGameEntity(this);
                        Debug.Assert(ID.IsValid(_entityId));
                    }
                    else if(ID.IsValid(EntityId))
                    {
                        _componentsList.ToList().ForEach(x => x.Load());
                        VelAPI.EntityAPI.RemoveGameEntity(this);
                        EntityId = ID.INVALID_ID;
                    }
                    OnPropertyChanged(nameof(_isActive));
                }
            }
        }

        private bool _isEnabled = true;
        [DataMember]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }
        private string _name;
        [DataMember]
        public string Name 
        {
            get => _name;
            set {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            } 
        }

        [DataMember]
        public Scene ParentScene { get; private set; }

        public double positionX;

        [DataMember(Name = nameof(ComponentsList))]
        private readonly ObservableCollection<Component> _componentsList = [];
        public ReadOnlyObservableCollection<Component> ComponentsList { get; private set; }

        public Component GetComponent(Type type) => ComponentsList.FirstOrDefault(c => type.IsAssignableFrom(c.GetType()));
        public T GetComponent<T>() where T : Component => GetComponent(typeof(T)) as T;

        public T GetComponentByName<T>(string name) where T : Component => ComponentsList.OfType<T>().FirstOrDefault(c => c.GetName() == name);

        public IEnumerable<Component> GetComponents(Type type) => ComponentsList.Where(c => type.IsAssignableFrom((Type)c.GetType()));
        public IEnumerable<T> GetComponents<T>() where T : Component => ComponentsList.OfType<T>();

        public bool AddComponent(Component component)
        {
            Debug.Assert(component != null);
            if(component.AllowMultiples || !ComponentsList.Any(x=>x.GetType() == component.GetType()))
            {
                // Adding a component to an inactive entity should not activate it.
                var wasActive = IsActive;
                IsActive = false;
                _componentsList.Add(component);
                IsActive = wasActive;
                return true;
            }
            Logger.Log(MessageType.Warning, $"Entity {Name} already has {component.GetType().Name} component.");
            return false;
        }

        public void RemoveComponent(Component component)
        {
            Debug.Assert(component != null);
            if (component is Transform) return;     // Cannot remove Transform component
            if (!ComponentsList.Contains(component))
            {
                IsActive = false;
                _componentsList.Remove(component);
                IsActive = true;
                return;
            }
            Logger.Log(MessageType.Warning, $"Entity {Name} does not have {component.GetType().Name} component");
        }

        [OnDeserialized]
        void OnDersialized(StreamingContext context)
        {
            if (_componentsList != null)
            {
                ComponentsList = new ReadOnlyObservableCollection<Component>(_componentsList);
                OnPropertyChanged(nameof(ComponentsList));
            }
        }

        public GameEntity(Scene scene)
        {
            Debug.Assert(scene != null);
            ParentScene = scene;
            _componentsList.Add(new Transform(this));
            OnDersialized(new StreamingContext());
        }
    }

    abstract class MSEntity : ViewModelBase
    {
        // Enables updates to selected entities
        private bool _enableUpdates = true;
        private bool? _isEnabled;
        public bool? IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private string _name;
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

        private readonly ObservableCollection<IMSComponent> _components = [];
        public ReadOnlyObservableCollection<IMSComponent> ComponentsList { get; }

        public T GetMSComponent<T>() where T : IMSComponent
        {
            return (T)ComponentsList.FirstOrDefault(x => x.GetType() == typeof(T));
        }

        public List<GameEntity> SelectedEntities { get; }

        private void MakeComponentList()
        {
            _components.Clear();
            var firstEntity = SelectedEntities.FirstOrDefault();
            if (firstEntity == null) return;

            foreach(var component in firstEntity.ComponentsList)
            {
                var type = component.GetType();
                if (type.Equals(typeof(Script)))
                    continue;
                if (!SelectedEntities.Skip(1).Any(entity => entity.GetComponent(type) == null))
                {
                    Debug.Assert(ComponentsList.FirstOrDefault(x => x.GetType() == type) == null);
                    _components.Add(component.GetMultiSelectionComponent(this));
                }
            }

            foreach(var component in firstEntity.GetComponents(typeof(Script)))
            {
                _components.Add(component.GetMultiSelectionComponent(this));
            }
        }

        public static int? GetMixedValue<T>(List<T> objects, Func<T, int> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => value != getProperty(x)) ? null : value;
        }

        public static float? GetMixedValue<T>(List<T> objects, Func<T, float> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => !getProperty(x).IsTheSameAs(value)) ? null : value; 
        }

        public static bool? GetMixedValue<T>(List<T> objects, Func<T, bool> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => getProperty(x) != value) ? null :  value;
        }

        public static string GetMixedValue<T>(List<T> objects, Func<T, string> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => value != getProperty(x)) ? null :  value;
        }

        protected virtual bool UpdateGameEntities(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IsEnabled): SelectedEntities.ForEach(x => x.IsEnabled = IsEnabled.Value); return true;
                case nameof(Name): SelectedEntities.ForEach(x => x.Name = Name); return true;
            }
            return false;
        }

        protected virtual bool UpdateMSGameEntity()
        {
            IsEnabled = GetMixedValue(SelectedEntities, new Func<GameEntity, bool>(x => x.IsEnabled));
            Name = GetMixedValue(SelectedEntities, new Func<GameEntity, string>(x => x.Name));

            return true;
        }

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSGameEntity();
            MakeComponentList();
            _enableUpdates = true;
        }

        public MSEntity(List<GameEntity> entities)
        {
            Debug.Assert(entities?.Any() == true);
            ComponentsList = new ReadOnlyObservableCollection<IMSComponent>(_components);
            SelectedEntities = entities;
            PropertyChanged += (s, e) => { if(_enableUpdates) UpdateGameEntities(e.PropertyName); };
        }
    }

    class MsGameEntity : MSEntity
    {
        public MsGameEntity(List<GameEntity> entities) : base(entities)
        {
            Refresh();
        }
    }

}
