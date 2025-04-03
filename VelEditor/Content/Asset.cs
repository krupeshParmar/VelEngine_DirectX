using System.Diagnostics;
using System.IO;

namespace VelEditor.Content
{
    enum AssetType
    {
        Unkonwn,
        Animation,
        Audio,
        Material,
        Mesh,
        Skeleton,
        Texture
    }

    abstract class Asset : ViewModelBase
    {
        public static string AssetFileExtension = ".velasset";
        public AssetType Type { get; private set; }

        public byte[] Icon { get; protected set; }
        public string SourcePath { get; protected set; }
        public Guid GUID { get; protected set; } = Guid.NewGuid();
        public DateTime ImportDate { get; protected set; }
        public byte[] Hash { get; protected set; }

        public abstract IEnumerable<string> Save(string file);

        protected void WriteAssetFileHeader(BinaryWriter writer)
        {
            var id = GUID.ToByteArray();
            var importDate = DateTime.Now.ToBinary();
            writer.BaseStream.Position = 0;

            writer.Write((int)Type);
            writer.Write(id.Length);
            writer.Write(id);
            writer.Write(importDate);

            // asset hash is optional
            if(Hash?.Length > 0)
            {
                writer.Write(Hash.Length);
                writer.Write(Hash);
            }
            else
            {
                writer.Write(0);
            }
            writer.Write(SourcePath ?? "");
            writer.Write(Icon.Length);
            writer.Write(Icon);
        }

        public Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.Unkonwn);
            Type = type;
        }
    }
}
