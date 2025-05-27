using VelEditor.Content;
using System;
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
        Guid AssetGuid { get; }
        Asset Asset { get; }

        void SetAsset(AssetInfo info);
    }
}