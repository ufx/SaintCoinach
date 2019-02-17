using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Graphics.Viewer.Interop
{
    public static class FbxExport
    {
        static class Interop
        {
            [DllImport("fbxInterop.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void exportFbx([In, Out] IntPtr[] meshes, int numMeshes,
                                                [In, Out] byte[] skeleton, int skeletonSize,
                                                [In, Out] byte[] animation, int animationSize,
                                                [In, Out] int[] boneMap, int mapLength,
                                                string filename);
        }

        public static void ExportFbx(string filename,
                                        Mesh[] ma,
                                        Skeleton skele,
                                        byte[] anims)
        {
            ModelDefinition thisDefinition = ma[0].Model.Definition;

            int[] boneMap;
            var nameMap = new Dictionary<string, int>();
            for (var i = 0; i < skele.BoneNames.Length; ++i)
                nameMap.Add(skele.BoneNames[i], i);
            boneMap = thisDefinition.BoneNames.Select(n => nameMap[n]).ToArray();

            IntPtr[] meshes = new IntPtr[ma.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                InteropMesh m = new InteropMesh(ma[i]);
                meshes[i] = m._UnmanagedPtr;
            }

            Interop.exportFbx(meshes, meshes.Length,
                skele.File.HavokData, skele.File.HavokData.Length,
                anims, anims.Length,
                boneMap, boneMap.Length,
                filename);
        }
    }
}
