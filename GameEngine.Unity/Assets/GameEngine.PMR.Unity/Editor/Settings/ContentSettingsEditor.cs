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
        [MenuItem("GameEngine/Project Settings/Content Settings", priority = 0)]
        public static void DisplayLogSettings()
        {
            Selection.activeObject = GameEngineConfiguration.GetOrCreateSettingAsset<ContentSettings>(ContentSettings.ASSET_NAME, SettingsScope.Project);
        }

        /// <summary>
        /// Display a custom inspector for Content Settings
        /// </summary>
        public override void OnInspectorGUI()
        {
            ContentSettings settings = (ContentSettings)target;

            EditorGUI.BeginChangeCheck();
            m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
            m_TitleStyle.normal.textColor = new Color(0.25f, 0.75f, 1f);

            DataContentGUI(settings);
            EditorGUILayout.Space(20);

            DescriptorsContentGUI(settings);
            EditorGUILayout.Space(20);

            AssetsContentGUI(settings);
            EditorGUILayout.Space(20);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(settings);
        }

        private void DataContentGUI(ContentSettings settings)
        {
            EditorGUILayout.LabelField("Data Content", m_TitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Common content data loaded from formatted files", MessageType.Info);
            EditorGUILayout.Space();

            settings.Configuration.EnableContentData = EditorGUILayout.Toggle("Enable", settings.Configuration.EnableContentData);
            EditorGUI.BeginDisabledGroup(!settings.Configuration.EnableContentData);
            settings.Configuration.DataContentPath = EditorGUILayout.TextField("Directory path", settings.Configuration.DataContentPath);
            settings.Configuration.FileContentFormat = (SerializerFormat)EditorGUILayout.EnumPopup("File format", settings.Configuration.FileContentFormat);
            settings.Configuration.FileEncodingType = (EncodingType)EditorGUILayout.EnumPopup("File encoding", settings.Configuration.FileEncodingType);
            EditorGUI.EndDisabledGroup();
        }

        private void DescriptorsContentGUI(ContentSettings settings)
        {
            EditorGUILayout.LabelField("Descriptors Content", m_TitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Scriptable objects called descriptors loaded from a descriptor bundle", MessageType.Info);
            EditorGUILayout.Space();

            settings.Configuration.EnableContentDescriptors = EditorGUILayout.Toggle("Enable", settings.Configuration.EnableContentDescriptors);
            EditorGUI.BeginDisabledGroup(!settings.Configuration.EnableContentDescriptors);
            settings.Configuration.DescriptorContentPath = EditorGUILayout.TextField("Directory path", settings.Configuration.DescriptorContentPath);
            settings.Configuration.DescriptorBundleName = EditorGUILayout.TextField("Bundle name", settings.Configuration.DescriptorBundleName);
            EditorGUI.EndDisabledGroup();
        }

        private void AssetsContentGUI(ContentSettings settings)
        {
            EditorGUILayout.LabelField("Assets Content", m_TitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Unity content assets loaded from different asset bundles", MessageType.Info);
            EditorGUILayout.Space();

            settings.Configuration.EnableContentAssets = EditorGUILayout.Toggle("Enable", settings.Configuration.EnableContentAssets);
            EditorGUI.BeginDisabledGroup(!settings.Configuration.EnableContentAssets);
            settings.Configuration.AssetContentPath = EditorGUILayout.TextField("Directory path", settings.Configuration.AssetContentPath);
            EditorGUI.EndDisabledGroup();
        }
    }
}
