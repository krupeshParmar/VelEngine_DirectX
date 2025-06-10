using VelEditor.Content;
using System;
using System.Threading.Tasks;
namespace VelEditor.Editors
{
    enum AssetEditorState
    {
        Done = 0,
        Importing,
        Processing,
        Loading,
        Saving,
    }

    interface IAssetEditor
    {
        AssetEditorState State { get; }
        Asset Asset { get; }
        bool CheckAssetGuid(Guid guid);
        Task SetAsset(AssetInfo info);
    }
}