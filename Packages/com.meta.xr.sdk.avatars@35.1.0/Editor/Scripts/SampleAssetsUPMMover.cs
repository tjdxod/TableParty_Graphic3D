// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Oculus.Avatar2
{
    // ScriptableObject classname must match the filename for it to get the path correctly
    public class SampleAssetsUPMMover : ScriptableObject
    {
        private static string SampleAssetsDest => Path.Combine(Application.dataPath, "Oculus", "Avatar2_SampleAssets", "SampleAssets", "SampleAssetsUnzipped");

        public static string GetBuildNumber()
        {
            var filePath = GetBuildNumberFilePath();
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Could not find build number at path {filePath}");
                return "error";
            }

            try
            {
                return File.ReadAllText(filePath).Trim();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return "error";
        }

        public static async void CopyFilesOver()
        {
            await Task.Yield();
            if (!CopyFiles())
            {
                return;
            }
            await Task.Yield();
            File.WriteAllText(GetVersionFilePath(), $"{GetBuildNumber()}\n");

            await Task.Yield();
            AssetDatabase.Refresh();
        }


        private static bool CopyFiles()
        {
            string sourcePath = Path.Combine(AssetsPathFinderHelper.GetSampleAssetsPackagePath(), "SampleAssetsUnzipped");
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            try
            {
                if (!sourceDir.Exists)
                {
                    Debug.LogError("SampleAssetsUPMMover failed to find path: " + sourcePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SampleAssetsUPMMover failed to find path: " + sourcePath + "\n\n" + e);
            }
            try
            {
                if (Directory.Exists(SampleAssetsDest))
                {
                    Debug.LogWarning("SampleAssetsUPMMover is deleting the old version of SampleAssets");
                    Directory.Delete(SampleAssetsDest, true);
                }

                CopyFilesRecursively(sourceDir, Directory.CreateDirectory(SampleAssetsDest));
            }
            catch (Exception e)
            {
                Debug.LogError("SampleAssetsUPMMover was unable to copy `" + sourcePath + "` to `" + SampleAssetsDest + "`." + "\n\n" + e);
                return false;
            }

            if (!Directory.Exists(SampleAssetsDest))
            {
                Debug.LogError("SampleAssetsUPMMover was unable to copy `" + sourcePath + "` to `" + SampleAssetsDest + "`.");
                return false;
            }
            return true;
        }

        public static bool DoesFileVersionMatch()
        {
            var textAsset = GetVersionFilePath();
            if (!File.Exists(textAsset))
            {
                return false;
            }

            try
            {
                return File.ReadAllText(textAsset).Trim() == GetBuildNumber();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                if (file.Extension == ".meta")
                {
                    continue;
                }

                Debug.Log("SampleAssetsUPMMover is copying " + file.FullName + " over to your Assets folder");
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        private static void DeleteFoldersRecursively(DirectoryInfo directory)
        {
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                DeleteFoldersRecursively(dir);
            }
            string metafile = directory.FullName + ".meta";
            if (File.Exists(metafile))
            {
                File.Delete(metafile);
            }
            directory.Delete();
        }

        private static string GetBuildNumberFilePath()
        {
            return Path.Combine("Packages", AssetsPathFinderHelper.sampleAssetsPackageName, "build_number.txt");
        }

        private static string GetVersionFilePath()
        {
            return Path.Combine(SampleAssetsDest, ".avatar_sdk_assets_version.txt");
        }
    }

    // Check for changes whenever Unity loads up this script.
    [InitializeOnLoad]
    public class SampleAssetsStartupTrigger
    {
        static SampleAssetsStartupTrigger()
        {
            // defer until the scene has loaded
            EditorApplication.update += RunOnce;
        }

        static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            if (!SampleAssetsUPMMover.DoesFileVersionMatch())
            {
                SampleAssetsUPMMover.CopyFilesOver();
            }
        }
    }
}
