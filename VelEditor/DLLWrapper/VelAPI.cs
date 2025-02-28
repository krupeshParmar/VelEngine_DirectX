using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VelEditor.Components;
using VelEditor.EngineAPIStructs;

namespace VelEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new Vector3(1,1,1);
    }


    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new TransformComponent();
    }
}

namespace VelEditor.DLLWrapper
{
    static class VelAPI
    {
        private const string _dllName = "EngineDLL.dll";

        [DllImport(_dllName)]
        private static extern int CreateGameEntity(GameEntityDescriptor desc);
        public static int CreateGameEntity(GameEntity entity)
        {
            GameEntityDescriptor gameEntityDescriptor = new GameEntityDescriptor();
            // transform
            {
                var c = entity.GetComponent<Transform>();
                gameEntityDescriptor.Transform.Position = c.Position;
                gameEntityDescriptor.Transform.Rotation = c.Rotation;
                gameEntityDescriptor.Transform.Scale = c.Scale;
            }
            return CreateGameEntity(gameEntityDescriptor);
        }

        [DllImport(_dllName)]
        private static extern void RemoveGameEntity(int id);
        public static void RemoveGameEntity(GameEntity entity)
        {
            RemoveGameEntity(entity.EntityId);
        }
    }
}
