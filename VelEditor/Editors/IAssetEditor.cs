using VelEditor.Content;

namespace VelEditor.Editors
{
    interface IAssetEditor
    {
        Asset Asset { get; }

        void SetAsset(AssetInfo asset);
    }
}