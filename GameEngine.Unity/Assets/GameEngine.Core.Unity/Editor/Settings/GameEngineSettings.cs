using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor.Settings
{
    public static class GameEngineSettings
    {
        public const string SETTINGS_FOLDER_DEFAULT = "Assets/GameEngineSettings/";
        public const string ROOT_FILE_NAME = "gameengine_settings";

        private static string m_SettingsFolder;

        static GameEngineSettings()
        {
            FindSettingsFolder();
            AssetDatabase.Refresh();
        }

        [SettingsProvider]
        public static SettingsProvider GameEngineSettingsProvider()
        {
            FindSettingsFolder();
            string displayedFolder = m_SettingsFolder;
            SettingsProvider provider = new SettingsProvider("Project/GameEngine", SettingsScope.Project)
            {
                label = "Game Engine",

                guiHandler = (searchContext) =>
                {
                    displayedFolder = EditorGUILayout.TextField("Settings Folder Path", displayedFolder);
                    EditorGUILayout.HelpBox("All the GameEngine settings for your project are stored in a single folder (inside Assets/). " +
                        "If you change the path of the settings folder and click 'Apply change', the folder will be moved automatically " +
                        "to the new location.", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Apply Change") && displayedFolder != m_SettingsFolder)
                    {
                        MoveSettingsFolder(m_SettingsFolder, displayedFolder);
                        m_SettingsFolder = displayedFolder;
                        AssetDatabase.Refresh();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                },

                keywords = new HashSet<string>(new[] { "GameEngine", "Settings", "Folder", "Path" })
            };

            return provider;
        }

        public static T GetOrCreateSettingAsset<T>(string assetPath, SettingsScope scope) where T : ScriptableObject
        {
            string fullAssetPath = Path.Combine(m_SettingsFolder, assetPath);
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

                if (scope == SettingsScope.User)
                    AddAssetToGitignore(assetPath);
            }

            return settings;
        }

        private static void FindSettingsFolder()
        {
            string[] searchResult = Directory.GetFiles("Assets", ROOT_FILE_NAME, SearchOption.AllDirectories);
            if (searchResult.Length == 0)
            {
                m_SettingsFolder = SETTINGS_FOLDER_DEFAULT;
                Directory.CreateDirectory(m_SettingsFolder);
                File.Create(Path.Combine(m_SettingsFolder, ROOT_FILE_NAME)).Close();
            }
            else
            {
                m_SettingsFolder = Path.GetDirectoryName(searchResult[0]);
                if (searchResult.Length > 1)
                {
                    Debug.LogWarning($"[GameEngine] Duplicate file {ROOT_FILE_NAME} found in Assets ({string.Join(", ", searchResult)})." +
                        $"\nPlease keep only one Settings Folder for GameEngine package with {ROOT_FILE_NAME} file at root." +
                        $"\nOtherwise you may face unwanted behaviour when loading and storing settings.");
                }
            }
        }

        private static void MoveSettingsFolder(string oldPath, string newPath)
        {
            if (!newPath.StartsWith("Assets\\") && !newPath.StartsWith("Assets/"))
                throw new ArgumentException("Folder path should be inside Assets/");

            if (Directory.Exists(oldPath) && !Directory.Exists(newPath))
            {
                Directory.Move(oldPath, newPath);
            }
            else
            {
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
                if (Directory.Exists(oldPath))
                    Directory.Delete(oldPath, true);

                File.Create(Path.Combine(newPath, ROOT_FILE_NAME)).Close();
            }
        }

        private static void AddAssetToGitignore(string assetPath)
        {
            string gitignorePath = Path.Combine(m_SettingsFolder, ".gitignore");
            using (StreamWriter writer = new StreamWriter(File.Open(gitignorePath, FileMode.Append)))
            {
                writer.WriteLine(assetPath);
            }
        }
    }
}
