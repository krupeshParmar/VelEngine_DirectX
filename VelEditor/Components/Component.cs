using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VelEditor.Components
{
    interface IMSComponent { }
    [DataContract]
    abstract class Component : ViewModelBase
    {
        [DataMember]
        public GameEntity Owner { get; private set; }
        [DataMember]
        public bool AllowMultiples { get; protected set; } = false;
        public ComponentType ComponentType { get; protected set; }
        public abstract string GetName();
        public abstract IMSComponent GetMultiSelectionComponent(MSEntity msEntity);
        public abstract void WriteToBinary(BinaryWriter bw);
        public abstract ComponentDescriptor GetComponentDescriptor();

        public virtual void Load() { }
        public virtual void Unload() { }

        public Component(GameEntity entity)
        {
            Debug.Assert(entity != null);
            Owner = entity;
        }
    }

    abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : Component
    {
        private bool _enableUpdates = true;
        public bool EnableUpdates
        {
            get => _enableUpdates;
        }
        public List<T> SelectedComponents { get; protected set; }

        protected abstract bool UpdateComponents(string propertyName);
        protected abstract bool UpdateMSComponents();

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSComponents();
            _enableUpdates = true;
        }

        public MSComponent(MSEntity msEntity)
        {
        }
    }
}
