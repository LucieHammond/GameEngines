using GameEngine.Core.Logger;
using GameEngine.Core.Logger.Base;
using GameEngine.Core.Unity.Logger;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor.Settings
{
    /// <summary>
    /// Editor window for Log Settings
    /// </summary>
    [CustomEditor(typeof(LogSettings))]
    public class LogSettingsEditor : Editor
    {
        private const string TAG = "LogSettings";

        private string m_TagToAdd = "";
        private string m_TagToRemove = null;
        private bool m_ShowAllTags = true;

        private BaseLoggerType m_LoggerToAdd;
        private BaseLoggerType? m_LoggerToRemove = null;
        private bool m_ShowAddLoggers = false;

        private GUIStyle m_TitleStyle;

        /// <summary>
        /// Find the unique Log Settings asset defined in the project and display it in the inspector window
        /// </summary>
        [MenuItem("GameEngine/User Settings/Log Settings", priority = 1)]
        public static void DisplayLogSettings()
        {
            Selection.activeObject = GameEngineConfiguration.GetOrCreateSettingAsset<LogSettings>(LogSettings.ASSET_NAME, SettingsScope.User);
        }

        /// <summary>
        /// Display a custom inspector for Log Settings
        /// </summary>
        public override void OnInspectorGUI()
        {
            LogSettings settings = (LogSettings)target;

            EditorGUI.BeginChangeCheck();
            m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
            m_TitleStyle.normal.textColor = new Color(0.25f, 0.75f, 1f);

            EditorGUILayout.LabelField("Unity Logger", m_TitleStyle);
            EditorGUILayout.Space();

            settings.MinLogLevel = (LogLevel)EditorGUILayout.EnumPopup("Min level to log", settings.MinLogLevel);

            EditorGUILayout.Space();
            settings.ActivateFiltering = EditorGUILayout.BeginToggleGroup("Filter on tags", settings.ActivateFiltering);
            TagFilteringGUI(settings);
            EditorGUILayout.EndToggleGroup();

            foreach (KeyValuePair<BaseLoggerType, object[]> logger in settings.AdditionalLoggers)
            {
                EditorGUILayout.Space(25);
                AdditionalLoggerSectionGUI(logger.Key, logger.Value);
            }

            EditorGUILayout.Space(25);
            AddNewLoggerGUI(settings);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(settings);
        }

        private void TagFilteringGUI(LogSettings settings)
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
                    if (settings.TagsFilter.Contains(m_TagToAdd))
                    {
                        Debug.Log($"[{TAG}] Tag \"{m_TagToAdd}\" is already in list");
                    }
                    else
                    {
                        settings.TagsFilter.Add(m_TagToAdd);
                        settings.TagsColors.Add(m_TagToAdd, LogSettings.GetColorForTag(m_TagToAdd));
                    }

                    m_TagToAdd = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AdditionalLoggerSectionGUI(BaseLoggerType loggerType, object[] loggerParameters)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(loggerType.ToString()), m_TitleStyle);
            if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
            {
                m_LoggerToRemove = loggerType;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            int index = 0;
            switch (loggerType)
            {
                case BaseLoggerType.ConsoleLogger:
                case BaseLoggerType.DebugLogger:
                    break;
                case BaseLoggerType.FileLogger:
                    loggerParameters[0] = EditorGUILayout.TextField("Log File Path", (string)loggerParameters[0]);
                    index = 1;
                    break;
                case BaseLoggerType.NetLogger:
                    loggerParameters[0] = EditorGUILayout.TextField("Log API Url", (string)loggerParameters[0]);
                    loggerParameters[1] = EditorGUILayout.TextField("App Version", (string)loggerParameters[1]);
                    loggerParameters[2] = EditorGUILayout.TextField("Environment", (string)loggerParameters[2]);
                    index = 3;
                    break;
                default:
                    throw new NotImplementedException($"Missing implementation for logger type {loggerType}");
            }

            loggerParameters[index] = (LogLevel)EditorGUILayout.EnumPopup("Min level to log", (LogLevel)loggerParameters[index]);
        }

        private void AddNewLoggerGUI(LogSettings settings)
        {
            m_ShowAddLoggers = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowAddLoggers, " Add Additional Logger");
            EditorGUILayout.Space();
            if (m_ShowAddLoggers)
            {
                EditorGUILayout.BeginHorizontal();
                m_LoggerToAdd = (BaseLoggerType)EditorGUILayout.EnumPopup("Logger Type", m_LoggerToAdd);
                if (GUILayout.Button("Add", GUILayout.MaxWidth(50)))
                {
                    if (settings.AdditionalLoggers.ContainsKey(m_LoggerToAdd))
                    {
                        Debug.Log($"[{TAG}] A {m_LoggerToAdd} has already been added");
                    }
                    else
                    {
                        object[] loggerParameters;
                        switch (m_LoggerToAdd)
                        {
                            case BaseLoggerType.ConsoleLogger:
                            case BaseLoggerType.DebugLogger:
                                loggerParameters = new object[1] { LogLevel.Debug };
                                break;
                            case BaseLoggerType.FileLogger:
                                loggerParameters = new object[2] { string.Empty, LogLevel.Debug };
                                break;
                            case BaseLoggerType.NetLogger:
                                loggerParameters = new object[4] { string.Empty, Application.version, "Local", LogLevel.Debug };
                                break;
                            default:
                                throw new NotImplementedException($"Missing implementation for logger type {m_LoggerToAdd}");
                        };
                        settings.AdditionalLoggers.Add(m_LoggerToAdd, loggerParameters);
                        m_LoggerToAdd = default(BaseLoggerType);
                    }
                }
                EditorGUILayout.EndHorizontal();
            
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_LoggerToRemove != null)
            {
                settings.AdditionalLoggers.Remove(m_LoggerToRemove.Value);
                m_LoggerToRemove = null;
            }
        }
    }
}