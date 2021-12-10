using GameEngine.Core.UnityEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using InputSettings = GameEngine.PMR.Unity.Basics.Configuration.InputSettings;

namespace GameEngine.PMR.UnityEditor.Settings
{
    [CustomEditor(typeof(InputSettings))]
    public class InputSettingsEditor : Editor
    {
        private GUIStyle m_TitleStyle;

        [MenuItem("GameEngine/Project Settings/Input Settings", priority = 1)]
        public static void DisplayInputSettings()
        {
            Selection.activeObject = GameEngineConfiguration.GetOrCreateSettingAsset<InputSettings>(InputSettings.ASSET_NAME, SettingsScope.Project);
        }

        public override void OnInspectorGUI()
        {
            InputSettings settings = (InputSettings)target;

            m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
            m_TitleStyle.normal.textColor = new Color(0.25f, 0.75f, 1f);

            EditorGUILayout.LabelField("Input Actions", m_TitleStyle);
            EditorGUILayout.Space();

            settings.Configuration.ActionsAsset = (InputActionAsset)EditorGUILayout
                .ObjectField("Actions Asset", settings.Configuration.ActionsAsset, typeof(InputActionAsset), false);
            settings.Configuration.DefaultActionMap = EditorGUILayout.TextField("Default Action Map", settings.Configuration.DefaultActionMap);
            settings.Configuration.CallSynchronously = EditorGUILayout.Toggle("Synchronous Callbacks", settings.Configuration.CallSynchronously);
        }
    }
}
