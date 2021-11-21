using GameEngine.Core.Serialization.Text;
using GameEngine.Core.UnityEditor;
using GameEngine.Core.Utilities.Enums;
using GameEngine.PMR.Unity.Basics.Configuration;
using UnityEditor;
using UnityEngine;

namespace GameEngine.PMR.UnityEditor.Settings
{
    /// <summary>
    /// Editor window for Content Settings
    /// </summary>
    [CustomEditor(typeof(ContentSettings))]
    public class ContentSettingsEditor : Editor
    {
        private GUIStyle m_TitleStyle;

        /// <summary>
        /// Find the unique Content Settings asset defined in the project and display it in the inspector window
        /// </summary>
        [MenuItem("GameEngine/Project Settings/Content Settings", priority = 1)]
        public static void DisplayLogSettings()
        {
            Selection.activeObject = GameEngineSettings.GetOrCreateSettingAsset<ContentSettings>(ContentSettings.ASSET_NAME, SettingsScope.Project);
        }

        /// <summary>
        /// Display a custom inspector for Content Settings
        /// </summary>
        public override void OnInspectorGUI()
        {
            ContentSettings settings = (ContentSettings)target;

            m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
            m_TitleStyle.normal.textColor = new Color(0.25f, 0.75f, 1f);

            EditorGUILayout.LabelField("File Content", m_TitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Common content data loaded from formatted files", MessageType.Info);
            EditorGUILayout.Space();

            settings.Configuration.FileContentPath = EditorGUILayout.TextField("Folder path", settings.Configuration.FileContentPath);
            settings.Configuration.FileContentFormat = (SerializerFormat)EditorGUILayout.EnumPopup("Content format", settings.Configuration.FileContentFormat);
            settings.Configuration.FileEncodingType = (EncodingType)EditorGUILayout.EnumPopup("Content encoding", settings.Configuration.FileEncodingType);

            EditorGUILayout.Space(25);

            EditorGUILayout.LabelField("Unity Content", m_TitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Unity assets and descriptors loaded from asset bundles", MessageType.Info);
            EditorGUILayout.Space();

            settings.Configuration.AssetBundlesPath = EditorGUILayout.TextField("Folder path", settings.Configuration.AssetBundlesPath);
            settings.Configuration.DescriptorsBundle = EditorGUILayout.TextField("Descriptors bundle", settings.Configuration.DescriptorsBundle);
        }
    }
}
