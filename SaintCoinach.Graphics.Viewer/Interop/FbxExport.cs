using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;

namespace SaintCoinach.Graphics.Viewer.Interop
{
    public static class FbxExport
    {
        static class Interop
        {
            [DllImport("fbxInterop.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void exportFbx([In, Out] IntPtr[] meshes, int numMeshes,
                                                [In, Out] byte[] skeleton, int skeletonSize,
                                                [In, Out] byte[] animation, int animationSize, [In, Out] string[] animNames,
                                                [In, Out] int[] boneMap, int mapLength,
                                                string filename, int mode);
        }

        public static void ExportFbx(string fileName,
                                        Mesh[] ma,
                                        Skeleton skele,
                                        PapFile pap,
                                        int mode = 0)
        {
            System.Diagnostics.Trace.WriteLine("Begin export");
            ModelDefinition thisDefinition = ma[0].Model.Definition;

            // Create bonemap in the same manner that hkAnimationInterop does
            var nameMap = new Dictionary<string, int>();
            for (var i = 0; i < skele.BoneNames.Length; ++i)
                nameMap.Add(skele.BoneNames[i], i);
            var boneMap = thisDefinition.BoneNames.Select(n => nameMap[n]).ToArray();

            // Get mesh ptrs
            IntPtr[] meshes = new IntPtr[ma.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                InteropMesh m = new InteropMesh(ma[i]);
                meshes[i] = m._UnmanagedPtr;
            }

            // Null pap handling
            byte[] anims = pap == null ? new byte[0] : pap.HavokData;
            string[] animNames = pap == null ? new string[0] : pap.Animations.Select(_ => _.Name).ToArray();
            
            Interop.exportFbx(meshes, meshes.Length,
                skele.File.HavokData, skele.File.HavokData.Length,
                anims, anims.Length, animNames,
                boneMap, boneMap.Length,
                fileName, mode);

            System.Diagnostics.Trace.WriteLine("Finish export");

            //todo: materials/textures
        }
    }
}
