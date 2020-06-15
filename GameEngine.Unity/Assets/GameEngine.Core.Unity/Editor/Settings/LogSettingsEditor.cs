using GameEngine.Core.Logger;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor.Settings
{
    [CustomEditor(typeof(LogSettings))]
    public class LogSettingsEditor : Editor
    {
        public const string ASSET_PATH = "Resources/LogSettings.asset";

        private string m_TagToAdd = "";
        private string m_TagToRemove = null;
        private bool m_ShowAllTags = true;

        [MenuItem("GameEngine/User Preferences/LogSettings", priority = 1)]
        public static void DisplayLogSettings()
        {
            Selection.activeObject = SettingsUtils.GetOrCreateSettingAsset<LogSettings>(ASSET_PATH, true); ;
        }

        public override void OnInspectorGUI()
        {
            LogSettings settings = (LogSettings)target;

            EditorGUILayout.Space();
            settings.MinLogLevel = (LogLevel)EditorGUILayout.EnumPopup("Min level to log", settings.MinLogLevel);
            
            EditorGUILayout.Space();
            settings.ActivateFiltering = EditorGUILayout.BeginToggleGroup("Filter on tags", settings.ActivateFiltering);
            TagFilterSectionGUI(settings);
            EditorGUILayout.EndToggleGroup();
        }

        private void TagFilterSectionGUI(LogSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();

            m_ShowAllTags = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowAllTags, "Allowed tags");
            if (m_ShowAllTags)
            {
                TagsListGUI(settings);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            NewTagGUI(settings);

            if (settings.ActivateFiltering && settings.TagsFilter.Count == 0)
                EditorGUILayout.HelpBox("The list of allowed tags is empty : no logs will be displayed in console", MessageType.Warning);
        }

        private void TagsListGUI(LogSettings settings)
        {
            foreach (string tag in settings.TagsFilter)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField(tag);
                settings.TagsColors[tag] = EditorGUILayout.ColorField(settings.TagsColors[tag]);
                if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
                {
                    m_TagToRemove = tag;
                    GUI.FocusControl("");
                }
                EditorGUILayout.EndHorizontal();
            }
            if (m_TagToRemove != null)
            {
                settings.TagsFilter.Remove(m_TagToRemove);
                settings.TagsColors.Remove(m_TagToRemove);
                m_TagToRemove = null;
            }
        }

        private void NewTagGUI(LogSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New allowed tag :", GUILayout.Width(120));
            m_TagToAdd = EditorGUILayout.TextField(m_TagToAdd);
            if (GUILayout.Button("+", GUILayout.MaxWidth(40)))
            {
                if (m_TagToAdd.Length > 0)
                {
                    settings.TagsFilter.Add(m_TagToAdd);
                    settings.TagsColors.Add(m_TagToAdd, LogSettings.GetColorForTag(m_TagToAdd));
                    m_TagToAdd = "";
                }
            }
            GUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
        }
    }
}
