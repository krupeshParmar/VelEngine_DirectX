using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VelEditor.Components
{
    enum ComponentType : int
    {
        Transform = 0,
        Script = 1,
        Geometry = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    class ComponentDescriptor
    {
        public int TypeId = -1;
        public IntPtr Data;
    }

    static class ComponentFactory
    {
        private static readonly Func<GameEntity, object, Component>[] _function =
            new Func<GameEntity, object, Component>[]
            {
                (entity, data) => new Transform(entity),
                (entity, data) => new Script(entity){Name = (string)data},
            };

        public static Func<GameEntity, object, Component> GetCreationFunction(ComponentType componentType)
        {
            Debug.Assert((int)componentType < _function.Length);
            return _function[(int)componentType];
        }

        public static ComponentType ToEnumType(this Component component)
        {
            return component switch
            {
                Transform => ComponentType.Transform,
                Script => ComponentType.Script,
                Geometry => ComponentType.Geometry,
                _ => throw new ArgumentException("Component type not found"),
            };
        }
    }
}
