using GameEngine.Core.UnityEditor;
using GameEngine.PMR.Unity.Basics.Configuration;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameEngine.PMR.UnityEditor.Settings
{
    [CustomEditor(typeof(InputsSettings))]
    public class InputsSettingsEditor : Editor
    {
        private GUIStyle m_TitleStyle;

        [MenuItem("GameEngine/Project Settings/Inputs Settings", priority = 1)]
        public static void DisplayInputSettings()
        {
            Selection.activeObject = GameEngineConfiguration.GetOrCreateSettingAsset<InputsSettings>(InputsSettings.ASSET_NAME, SettingsScope.Project);
        }

        public override void OnInspectorGUI()
        {
            InputsSettings settings = (InputsSettings)target;

            EditorGUI.BeginChangeCheck();
            m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
            m_TitleStyle.normal.textColor = new Color(0.25f, 0.75f, 1f);

            EditorGUILayout.LabelField("Input Actions", m_TitleStyle);
            EditorGUILayout.Space();

            settings.Configuration.ActionsAsset = (InputActionAsset)EditorGUILayout
                .ObjectField("Actions Asset", settings.Configuration.ActionsAsset, typeof(InputActionAsset), false);
            settings.Configuration.DefaultActionMap = EditorGUILayout.TextField("Default Action Map", settings.Configuration.DefaultActionMap);
            settings.Configuration.CallSynchronously = EditorGUILayout.Toggle("Synchronous Callbacks", settings.Configuration.CallSynchronously);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(settings);
        }
    }
}
