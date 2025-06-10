using System;
using System.Diagnostics;
using System.IO;
using VelEditor.ContentToolsAPIStruct;
using VelEditor.DLLWrapper;

namespace VelEditor.Content
{
    static class DefaultAssets
    {
        public static AssetInfo BrdfIntegrationLut { get; private set; }
        public static AssetInfo DefaultGeometry { get; private set; }

        /// <summary>
        ///     Generate default assets if necessary.
        /// </summary>
        public static void GenerateDefaultAssets()
        {
            var defaultAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Resources\DefaultAssets\");
            if (!Directory.Exists(defaultAssetsPath))
            {
                Directory.CreateDirectory(defaultAssetsPath);
            }

            var brdfLutFileName = $@"{defaultAssetsPath}BrdfIntegrationLut.velasset";

            if (!File.Exists(brdfLutFileName))
            {
                ComputeBrdfIntegrationLut(brdfLutFileName);
            }

            var cubeFileName = $@"{defaultAssetsPath}DefaultCube.velasset";

            if (!File.Exists(cubeFileName))
            {
                CreateDefaultCube(cubeFileName);
            }
        }

        private static void ComputeBrdfIntegrationLut(string file)
        {
            try
            {
                var brdfLut = new Texture() { FullPath = file };
                ContentToolsAPI.ComputeBrdfIntegrationLut(brdfLut);
                brdfLut.Save(file);
                BrdfIntegrationLut = Asset.GetAssetInfo(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void CreateDefaultCube(string file)
        {
            try
            {
                var cube = new Geometry();
                var info = new PrimitiveInitInfo()
                {
                    Type = PrimitiveMeshType.Cube,
                };

                ContentToolsAPI.CreatePrimitiveMesh(cube, info);
                cube.Save(file);
                DefaultGeometry = Asset.GetAssetInfo(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
