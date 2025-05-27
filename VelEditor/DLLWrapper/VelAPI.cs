using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VelEditor.Components;
using VelEditor.EngineAPIStructs;
using VelEditor.GameDev;
using VelEditor.GameProject;
using VelEditor.Utilities;

namespace VelEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new(1,1,1);
    }

    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponent
    {
        public IntPtr ScriptCreator;
    }


    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new();
        public ScriptComponent Script = new();
    }
}

namespace VelEditor.DLLWrapper
{
    static class VelAPI
    {
        private const string _engineDll = "EngineDLL.dll";

        [DllImport(_engineDll, CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string dllPath);

        [DllImport(_engineDll)]
        public static extern int UnloadGameCodeDll();

        [DllImport(_engineDll)]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(_engineDll)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        [DllImport(_engineDll)]
        public static extern int CreateRenderSurface(IntPtr host, int width, int height);

        [DllImport(_engineDll)]
        public static extern void RemoveRenderSurface(int surfaceId);

        [DllImport(_engineDll)]
        public static extern void ResizeRenderSurface(int surfaceId);

        [DllImport(_engineDll)]
        public static extern IntPtr GetWindowHandle(int surfaceId);

        internal static class EntityAPI
        {
            [DllImport(_engineDll)]
            private static extern int CreateGameEntity(GameEntityDescriptor desc);
            public static int CreateGameEntity(GameEntity entity)
            {
                GameEntityDescriptor gameEntityDescriptor = new();
                // transform
                {
                    var c = entity.GetComponent<Transform>();
                    gameEntityDescriptor.Transform.Position = c.Position;
                    gameEntityDescriptor.Transform.Rotation = c.Rotation;
                    gameEntityDescriptor.Transform.Scale = c.Scale;
                }
                // script component
                {
                    // NOTE: we check if project is not null, hence the game code dll has been loaded
                    //       If not then creation of script component is deferred until the dll has been loaded
                    var c = entity.GetComponent<Script>();
                    if (c != null && Project.Current != null)
                    {
                        if (Project.Current.AvailableScripts != null)
                        {
                            if (Project.Current.AvailableScripts.Contains(c.Name))
                            {
                                gameEntityDescriptor.Script.ScriptCreator = GetScriptCreator(c.Name);
                            }
                            else
                            {
                                Logger.Log(MessageType.Error, $"Unable to find script with name {c.Name}. Game Object will be created without the script");
                            }
                        }
                    }
                }

                return CreateGameEntity(gameEntityDescriptor);
            }

            [DllImport(_engineDll)]
            private static extern void RemoveGameEntity(int id);
            public static void RemoveGameEntity(GameEntity entity)
            {
                RemoveGameEntity(entity.EntityId);
            }
        }
    }
}
