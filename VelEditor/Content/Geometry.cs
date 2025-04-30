using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VelEditor.Utilities;

namespace VelEditor.Content
{
    enum PrimitiveMeshType
    {
        Plane,
        Cube,
        UVSphere,
        ICOSphere,
        Cylinder,
        Capsule
    }

    class Mesh : ViewModelBase
    {
        private int _vertexSize;
        public int VertexSize
        {
            get => _vertexSize;
            set
            {
                if (_vertexSize != value)
                {
                    _vertexSize = value;
                    OnPropertyChanged(nameof(VertexSize));
                }
            }
        }
        private int _vertexCount;
        public int VertexCount
        {
            get => _vertexCount;
            set
            {
                if (_vertexCount != value)
                {
                    _vertexCount = value;
                    OnPropertyChanged(nameof(VertexCount));
                }
            }
        }
        private int _indexSize;
        public int IndexSize
        {
            get => _indexSize;
            set
            {
                if (_indexSize != value)
                {
                    _indexSize = value;
                    OnPropertyChanged(nameof(IndexSize));
                }
            }
        }
        private int _indexCount;
        public int IndexCount
        {
            get => _indexCount;
            set
            {
                if (_indexCount != value)
                {
                    _indexCount = value;
                    OnPropertyChanged(nameof(IndexCount));
                }
            }
        }
        public byte[] Vertices { get; set; }
        public byte[] Indices { get; set; }
    }

    class MeshLOD : ViewModelBase
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

        private float _lodThreshold;
        public float LodThreshold
        {
            get => _lodThreshold;
            set
            {
                if (_lodThreshold != value)
                {
                    _lodThreshold = value;
                    OnPropertyChanged(nameof(LodThreshold));
                }
            }
        }


        public ObservableCollection<Mesh> MeshesList { get; } = new ObservableCollection<Mesh>();
    }

    class LODGroup : ViewModelBase
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
        public ObservableCollection<MeshLOD> LODsList { get; } = new ObservableCollection<MeshLOD>();
    }

    class GeometryImportSettings : ViewModelBase
    {
        private float _smoothingAngle;
        public float SmoothingAngle
        {
            get => _smoothingAngle;
            set
            {
                if (_smoothingAngle != value)
                {
                    _smoothingAngle = value;
                    OnPropertyChanged(nameof(SmoothingAngle));
                }
            }
        }

        private bool _calculateNormals;
        public bool CalculateNormals
        {
            get => _calculateNormals;
            set
            {
                if (_calculateNormals != value)
                {
                    _calculateNormals = value;
                    OnPropertyChanged(nameof(CalculateNormals));
                }
            }
        }


        private bool _calculateTangents;
        public bool CalculateTangents
        {
            get => _calculateTangents;
            set
            {
                if (_calculateTangents != value)
                {
                    _calculateTangents = value;
                    OnPropertyChanged(nameof(CalculateTangents));
                }
            }
        }


        private bool _reverseHandedness;
        public bool ReverseHandedness
        {
            get => _reverseHandedness;
            set
            {
                if (_reverseHandedness != value)
                {
                    _reverseHandedness = value;
                    OnPropertyChanged(nameof(ReverseHandedness));
                }
            }
        }


        private bool _importEmbededTextures;
        public bool ImportEmbededTextures
        {
            get => _importEmbededTextures;
            set
            {
                if (_importEmbededTextures != value)
                {
                    _importEmbededTextures = value;
                    OnPropertyChanged(nameof(ImportEmbededTextures));
                }
            }
        }


        private bool _importAnimations;
        public bool ImportAnimations
        {
            get => _importAnimations;
            set
            {
                if (_importAnimations != value)
                {
                    _importAnimations = value;
                    OnPropertyChanged(nameof(ImportAnimations));
                }
            }
        }

        public GeometryImportSettings()
        {
            SmoothingAngle = 178f;
            CalculateNormals = false;
            CalculateTangents = false;
            ReverseHandedness = false;
            ImportEmbededTextures = true;
            ImportAnimations = true;
        }

        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(SmoothingAngle);
            writer.Write(CalculateNormals);
            writer.Write(CalculateTangents);
            writer.Write(ReverseHandedness);
            writer.Write(ImportEmbededTextures);
            writer.Write(ImportAnimations);
        }
    }


    class Geometry : Asset
    {
        public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();

        private readonly List<LODGroup> _lodGroups = new List<LODGroup>();

        public LODGroup? GetLODGroup(int lodGroup = 0)
        {
            Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);
            return _lodGroups.Any() ? _lodGroups[lodGroup] : null;
        }

        public void FromRawData(byte[] data)
        {
            Debug.Assert(data?.Length > 0);

            _lodGroups.Clear();
            using var reader = new BinaryReader(new MemoryStream(data));
            // skip sceje name string
            var s = reader.ReadInt32();
            reader.BaseStream.Position += s;
            // get number of lods
            var numLodGroups = reader.ReadInt32();
            Debug.Assert(numLodGroups > 0);

            for (int i = 0; i < numLodGroups; ++i)
            {
                // get LOD group's name
                s = reader.ReadInt32();
                string lodGroupName;
                if (s > 0)
                {
                    var nameBytes = reader.ReadBytes(s);
                    lodGroupName = Encoding.UTF8.GetString(nameBytes);
                }
                else
                {
                    lodGroupName = $"lod_{ContentHelper.GetRandomString()}";
                }

                // get number of meshes in this lod group
                var numMeshes = reader.ReadInt32();
                Debug.Assert(numMeshes > 0);
                var lodsList = ReadMeshLods(numMeshes, reader);

                var lodGroup = new LODGroup() { Name = lodGroupName };
                lodsList.ForEach(l => lodGroup.LODsList.Add(l));

                _lodGroups.Add(lodGroup);
            }
        }

        private List<MeshLOD> ReadMeshLods(int numMeshes, BinaryReader reader)
        {
            var lodIds = new List<int>();
            var lodList = new List<MeshLOD>();
            for(int i = 0; i < numMeshes; ++i)
            {
                ReadMeshes(reader, lodIds, lodList);
            }
            return lodList;
        }

        private void ReadMeshes(BinaryReader reader, List<int> lodIdsList, List<MeshLOD> lodList)
        {
            // get mesh's name
            var s = reader.ReadInt32();
            string meshName;
            if (s > 0)
            {
                var nameBytes = reader.ReadBytes(s);
                meshName = Encoding.UTF8.GetString(nameBytes);
            }
            else
            {
                meshName = $"mesh_{ContentHelper.GetRandomString()}";
            }

            var mesh = new Mesh();

            var lodId = reader.ReadInt32();
            mesh.VertexSize = reader.ReadInt32();
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize = reader.ReadInt32();
            mesh.IndexCount = reader.ReadInt32();
            var lodThreshold = reader.ReadSingle();

            var vertexBufferSize = mesh.VertexSize * mesh.VertexCount;
            var indexBufferSize = mesh.IndexSize * mesh.IndexCount;

            mesh.Vertices = reader.ReadBytes(vertexBufferSize);
            mesh.Indices = reader.ReadBytes(indexBufferSize);

            MeshLOD lod;
            if (ID.IsValid(lodId) && lodIdsList.Contains(lodId))
            {
                lod = lodList[lodIdsList.IndexOf(lodId)];
                Debug.Assert(lod != null);
            }
            else
            {
                lodIdsList.Add(lodId);
                lod = new MeshLOD() { Name = meshName, LodThreshold = lodThreshold };
                lodList.Add(lod);
            }
            lod.MeshesList.Add(mesh);
        }

        public override IEnumerable<string> Save(string file)
        {
            Debug.Assert(_lodGroups.Any());
            var savedFiles = new List<string>();
            if(!_lodGroups.Any()) return savedFiles;

            var path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
            var fileName = Path.GetFileNameWithoutExtension(file);

            try
            {
                foreach (var lodgroup in _lodGroups)
                {
                    Debug.Assert(lodgroup.LODsList.Any());
                    // use the name of most detailed LOD for file name
                    var meshFileName = ContentHelper.SanitizeFileName(_lodGroups.Count > 1 ?
                                            path + fileName + "_" + lodgroup.LODsList[0].Name + AssetFileExtension :
                                            path + fileName + AssetFileExtension);
                    GUID = Guid.NewGuid();
                    byte[] data = null;
                    using (var writer = new BinaryWriter(new MemoryStream()))
                    {
                        writer.Write(lodgroup.Name);
                        writer.Write(lodgroup.LODsList.Count);
                        var hashes = new List<byte>();
                        foreach (var lod in lodgroup.LODsList)
                        {
                            LODTOBinary(lod, writer, out var hash);
                            hashes.AddRange(hash);
                        }

                        Hash = ContentHelper.ComputeHash(hashes.ToArray());
                        data = (writer.BaseStream as MemoryStream).ToArray();
                        Icon = GenerateIcon(lodgroup.LODsList[0]);
                    }
                    Debug.Assert(data?.Length > 0);

                    using(var writer = new BinaryWriter(File.Open(meshFileName, FileMode.Create, FileAccess.Write)))
                    {
                        WriteAssetFileHeader(writer);
                        ImportSettings.ToBinary(writer);
                        writer.Write(data.Length);
                        writer.Write(data);
                    }

                    savedFiles.Add(meshFileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MessageType.Error, $"Failed to save geometry file {fileName}.\n{ex.Message}");
            }
            return savedFiles;
        }

        private void LODTOBinary(MeshLOD lod, BinaryWriter writer, out byte[] hash)
        {
            writer.Write(lod.Name);
            writer.Write(lod.LodThreshold);
            writer.Write(lod.MeshesList.Count);

            var meshDataBegin = writer.BaseStream.Position;
            foreach (var mesh in lod.MeshesList)
            {
                writer.Write(mesh.VertexSize);
                writer.Write(mesh.VertexCount);
                writer.Write(mesh.IndexSize);
                writer.Write(mesh.IndexCount);
                writer.Write(mesh.Vertices);
                writer.Write(mesh.Indices);
            }

            var meshDataSize = writer.BaseStream.Position - meshDataBegin;
            Debug.Assert(meshDataSize > 0);
            var buffer = (writer.BaseStream as MemoryStream).ToArray();
            hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize);
        }

        private byte[] GenerateIcon(MeshLOD lod)
        {
            var width = 90 * 4;

            BitmapSource bmp = null;

            // NOTE: it's not good practive to use a WPF control (view) in the ViewModel.
            // But in this case, we need to render the mesh to a bitmap.
            // We can use a Dispatcher to run the code on the UI thread.
            Application.Current.Dispatcher.Invoke(() =>
            {
                bmp = Editors.GeometryView.RenderToBitmap(new Editors.MeshRenderer(lod, null), width, width);
                bmp = new TransformedBitmap(bmp, new ScaleTransform(0.25, 0.25, 0.5, 0.5));
            });

            using var memStream = new MemoryStream();
            memStream.SetLength(0);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(memStream);

            return memStream.ToArray();
        }

        public Geometry() : base(AssetType.Mesh) { }
    }
}
