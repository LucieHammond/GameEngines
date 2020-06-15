using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor
{
    public static class SettingsUtils
    {
        public const string ROOT_SETTINGS_FOLDER = "Assets/GameEngineSettings/";

        public static T GetOrCreateSettingAsset<T>(string assetPath, bool isUserSetting) where T : ScriptableObject
        {
            string fullAssetPath = Path.Combine(ROOT_SETTINGS_FOLDER, assetPath);
            T settings = AssetDatabase.LoadAssetAtPath<T>(fullAssetPath);
            if (settings == null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(fullAssetPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullAssetPath));
                }

                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, fullAssetPath);
                AssetDatabase.SaveAssets();

                if (isUserSetting)
                    AddAssetToGitignore(assetPath);
            }

            return settings;
        }

        private static void AddAssetToGitignore(string assetPath)
        {
            string gitignorePath = Path.Combine(ROOT_SETTINGS_FOLDER, ".gitignore");
            using (StreamWriter writer = new StreamWriter(File.Open(gitignorePath, FileMode.Append)))
            {
                writer.WriteLine(assetPath);
            }
        }
    }
}
