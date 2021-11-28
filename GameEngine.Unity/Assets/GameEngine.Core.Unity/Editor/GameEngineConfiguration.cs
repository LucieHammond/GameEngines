using GameEngine.Core.Utilities;
using GameEngine.Core.Utilities.Enums;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor
{
    public static class GameEngineConfiguration
    {
        private const string TAG = "GameEngineConfiguration";

        private const string SETTINGS_FOLDER_DEFAULT = "Assets/Packages/GameEngine";
        private const string ROOT_FILE_NAME = "gameengine_settings_root";

        private static string m_SettingsFolder;

        static GameEngineConfiguration()
        {
            FindSettingsFolder();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Register a custom project setting provider for the GameEngine framework package
        /// </summary>
        /// <returns>The GameEngine setting provider</returns>
        [SettingsProvider]
        public static SettingsProvider GameEngineSettingsProvider()
        {
            FindSettingsFolder();
            string folderPath = m_SettingsFolder;
            SettingsProvider provider = new SettingsProvider("Project/GameEngine", SettingsScope.Project)
            {
                label = "Game Engine",

                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Change Settings Location", EditorStyles.boldLabel);
                    folderPath = EditorGUILayout.TextField("Settings Folder Path", folderPath);
                    EditorGUILayout.HelpBox("This is the folder in which are stored all the GameEngine settings for your project.\n" +
                        "Applying a change will move all settings to the new location without loss.", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Apply Change"))
                    {
                        if (TryMoveSettingsFolder(folderPath))
                            AssetDatabase.Refresh();

                        folderPath = m_SettingsFolder;
                        GUI.FocusControl("");
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Ignore User Settings", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("You should add the following lines to your .gitignore file:");
                    string gitignoreLines = "# GameEngine user settings" +
                        $"\n{m_SettingsFolder}/UserSettings/" +
                        $"\n{m_SettingsFolder}/UserSettings.meta";
                    EditorGUILayout.TextArea(gitignoreLines, EditorStyles.helpBox, GUILayout.Width(350));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Append to .gitignore"))
                    {
                        UpdateGitignoreFile(gitignoreLines);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                },

                keywords = new HashSet<string>(new[] { "GameEngine", "Settings", "Package" })
            };

            return provider;
        }

        /// <summary>
        /// Find or create a specified setting asset in the GameEngine setting folder and return its content once loaded
        /// </summary>
        /// <typeparam name="T">The type of the setting asset to load</typeparam>
        /// <param name="assetName">The name of the setting asset</param>
        /// <param name="assetScope">The scope of the setting asset (user or project)</param>
        /// <returns>The content of the setting asset that has been loaded</returns>
        public static T GetOrCreateSettingAsset<T>(string assetName, SettingsScope assetScope) where T : ScriptableObject
        {
            string fullAssetPath = Path.Combine(m_SettingsFolder, $"{assetScope}Settings", "Resources", $"{assetName}.asset");
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
            }

            return settings;
        }

        /// <summary>
        /// Read and return the information stored for a given editor setting in the GameEngine setting folder
        /// </summary>
        /// <typeparam name="T">The type of the requested editor setting</typeparam>
        /// <param name="fileName">The name of the requested editor setting</param>
        /// <returns>The content of the setting file, converted into an instance of type T</returns>
        public static T GetEditorSetting<T>(string fileName) where T: new()
        {
            string fullFilePath = Path.Combine(m_SettingsFolder, $"Editor", "Resources", $"{fileName}.json");
            if (!File.Exists(fullFilePath))
                return new T();

            string jsonSetting = File.ReadAllText(fullFilePath);
            return JsonUtility.FromJson<T>(jsonSetting);
        }

        /// <summary>
        /// Write or replace the information stored for a given editor setting in the GameEngine setting folder
        /// </summary>
        /// <typeparam name="T">The type of the given editor setting</typeparam>
        /// <param name="dataName">The name of the given editor setting</param>
        /// <param name="data">The setting object to be stored</param>
        public static void SetEditorSetting<T>(string fileName, T setting) where T : new()
        {
            string fullFilePath = Path.Combine(m_SettingsFolder, $"Editor", "Resources", $"{fileName}.json");
            if (!File.Exists(fullFilePath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(fullFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));

                File.Create(fullFilePath).Close();
            }

            string jsonSetting = JsonUtility.ToJson(setting);
            File.WriteAllText(fullFilePath, jsonSetting);
            AssetDatabase.Refresh();
        }

        #region private
        private static void FindSettingsFolder()
        {
            string[] searchResult = Directory.GetFiles("Assets", ROOT_FILE_NAME, SearchOption.AllDirectories);
            if (searchResult.Length == 0)
            {
                InitializeSettingsFolder(SETTINGS_FOLDER_DEFAULT);
                m_SettingsFolder = SETTINGS_FOLDER_DEFAULT;
            }
            else
            {
                m_SettingsFolder = PathUtils.Normalize(Path.GetDirectoryName(searchResult[0]), false, PathSeparatorType.ForwardSlash, true);
                if (searchResult.Length > 1)
                {
                    Debug.LogWarning($"[{TAG}] Duplicate file {ROOT_FILE_NAME} found in Assets ({string.Join(", ", searchResult)})." +
                        $"\nPlease keep only one settings folder for the GameEngine package with {ROOT_FILE_NAME} file at root." +
                        $"\nOtherwise you may face unwanted behaviour when loading and storing settings.");
                }
            }
        }

        private static bool TryMoveSettingsFolder(string folderPath)
        {
            string newSettingsFolder = PathUtils.Normalize(folderPath, false, PathSeparatorType.ForwardSlash, true);
            if (newSettingsFolder != m_SettingsFolder)
            {
                if (newSettingsFolder.StartsWith("Assets/"))
                {
                    if (!Directory.Exists(newSettingsFolder + "/../"))
                        Directory.CreateDirectory(newSettingsFolder + "/../");

                    if (Directory.Exists(newSettingsFolder))
                        Directory.Delete(newSettingsFolder, true);

                    Directory.Move(m_SettingsFolder, newSettingsFolder);
                    File.Move(m_SettingsFolder + ".meta", newSettingsFolder + ".meta");
                    m_SettingsFolder = newSettingsFolder;

                    Debug.Log($"[{TAG}] Settings folder was changed to {newSettingsFolder}");
                    return true;
                }

                Debug.LogError($"[{TAG}] The given path \"{newSettingsFolder}\" is invalid (should be inside Assets/)");
            }

            Debug.Log($"[{TAG}] Settings folder didn't change");
            return false;
        }

        private static void InitializeSettingsFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);

            File.Create(Path.Combine(path, ROOT_FILE_NAME)).Close();
        }

        private static void UpdateGitignoreFile(string ignoreLines)
        {
            string gitignorePath = Path.Combine(".gitignore");
            using (StreamWriter writer = new StreamWriter(File.Open(gitignorePath, FileMode.Append)))
            {
                writer.WriteLine();
                writer.Write(ignoreLines);
            }

            Debug.Log($"[{TAG}] User Settings folder was successfully added to your .gitignore");
        }
        #endregion
    }
}
