using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using VelEditor.EngineAPIStructs;
using VelEditor.GameProject;
using VelEditor.Utilities;
using VelEditor.DLLWrapper;
using System.Diagnostics;
using System.Xml;

namespace VelEditor.Components
{
    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponent
    {
        public ulong HashScriptName;
        public IntPtr ScriptCreator;
    }
    [DataContract]
    class Script : Component
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

        public Script(GameEntity entity) : base(entity)
        {
            AllowMultiples = true;
            ComponentType = ComponentType.Script;
        }
        public override string GetName() => Name;

        public override IMSComponent GetMultiSelectionComponent(MSEntity msEntity) => new MSScript(msEntity, Name);

        public override void WriteToBinary(BinaryWriter bw)
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name);
            bw.Write(nameBytes.Length);
            bw.Write(nameBytes);
        }

        public override ComponentDescriptor GetComponentDescriptor()
        {
            ScriptComponent script = new ScriptComponent();
            ComponentDescriptor componentDescriptor = new();
            if (Project.Current != null && Owner != null && Project.Current.AvailableScripts != null)
            {
                if (Project.Current.AvailableScripts.Contains(Name))
                {
                    script.HashScriptName = Hashing.FNV1A(Name);
                    script.ScriptCreator = VelAPI.GetScriptCreator(Name);
                    IntPtr scriptPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ScriptComponent>());
                    Marshal.StructureToPtr(script, scriptPtr, false);

                    componentDescriptor.TypeId = (int)this.ToEnumType();
                    componentDescriptor.Data = scriptPtr;
                }
                else
                {
                    Logger.Log(MessageType.Error, $"Unable to find script with name {Owner.Name}. Game Object will be created without the script");
                }
            }
            
            return componentDescriptor;
        }
    }

    sealed class MSScript : MSComponent<Script>
    {
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

        protected override bool UpdateComponents(string propertyName)
        {
            if(propertyName == nameof(Name))
            {
                SelectedComponents.ForEach(c => c.Name = _name);
                return true;
            }
            return false;
        }

        protected override bool UpdateMSComponents()
        {
            Name = MSEntity.GetMixedValue(SelectedComponents, new Func<Script, string>(x=>x.Name));
            return true;
        }

        public MSScript(MSEntity msEntity, string name) : base(msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntities?.Any() == true);
            SelectedComponents = [.. msEntity.SelectedEntities.Select(entity => entity.GetComponentByName<Script>(name))];
            PropertyChanged += (s, e) => { if (EnableUpdates) UpdateComponents(e.PropertyName); };
            Refresh();            
        }
    }
}
