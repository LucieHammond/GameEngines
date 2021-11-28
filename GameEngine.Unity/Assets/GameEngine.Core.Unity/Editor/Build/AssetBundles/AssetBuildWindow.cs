using GameEngine.Core.Utilities;
using GameEngine.Core.Utilities.Enums;
using System;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Core.UnityEditor.Build.AssetBundles
{
    /// <summary>
    /// The customized Editor window for building asset bundles
    /// </summary>
    public class AssetBuildWindow : EditorWindow
    {
        private AssetBuildSettings m_BuildSettings;
        private bool[] m_ShowAssetBuild;
        private Vector2 m_ScrollPosition;
        private string m_ScenarioToAdd = "";
        private int m_ScenarioToRemove = -1;

        private bool m_AskBuild;
        private bool m_AskRebuild;
        private bool m_AskClean;

        /// <summary>
        /// Display the AssetBundle build editor interface as a floating window
        /// </summary>
        [MenuItem("GameEngine/Build Tools/Build Asset Bundles", priority = 3)]
        public static void DisplayBuildWindow()
        {
            GetWindow(typeof(AssetBuildWindow), false, "Build Asset Bundles");
        }

        /// <summary>
        /// Load the registered AssetBuild Settings when the window appears
        /// </summary>
        public void OnEnable()
        {
            m_BuildSettings = GameEngineConfiguration.GetEditorSetting<AssetBuildSettings>(AssetBuildSettings.FILE_NAME);
            m_ShowAssetBuild = new bool[m_BuildSettings.CustomBuildMap.Length];
            ResizeWindow();
        }

        /// <summary>
        /// Save the changes in the AssetBuild Settings when the window disappears
        /// </summary>
        public void OnDisable()
        {
            GameEngineConfiguration.SetEditorSetting(AssetBuildSettings.FILE_NAME, m_BuildSettings);
        }

        /// <summary>
        /// Display a custom window for building asset bundles 
        /// </summary>
        public void OnGUI()
        {
            GUIStyle mainStyle = new GUIStyle() { margin = new RectOffset(10, 10, 10, 10) };

            EditorGUILayout.BeginVertical(mainStyle);
            
            AssetsInBuildGUI();
            EditorGUILayout.Space(20);

            BuildScenariosGUI();
            NewScenarioGUI();
            EditorGUILayout.Space(20);

            ActionButtonsGUI();
            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();

            if (m_AskBuild)
            {
                AssetBundleBuilder.BuildAssetBundles(m_BuildSettings);
                AssetDatabase.Refresh();
                m_AskBuild = false;
            }
            if (m_AskRebuild)
            {
                AssetBundleBuilder.RebuildAssetBundles(m_BuildSettings);
                AssetDatabase.Refresh();
                m_AskRebuild = false;
            }
            if (m_AskClean)
            {
                AssetBundleBuilder.CleanAssetBundles(m_BuildSettings);
                AssetDatabase.Refresh();
                m_AskClean = false;
            }
        }

        private void AssetsInBuildGUI()
        {
            GUILayout.Label("Bundles in Build", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_BuildSettings.BuildAllBundles = EditorGUILayout.Toggle(m_BuildSettings.BuildAllBundles, GUILayout.Width(20));
            GUILayout.Label("Include all bundles specified in editor", EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUI.skin.textArea, GUILayout.ExpandHeight(true));
            if (m_BuildSettings.BuildAllBundles)
            {
                foreach (string bundle in AssetDatabase.GetAllAssetBundleNames())
                {
                    EditorGUI.BeginDisabledGroup(m_BuildSettings.BuildAllBundles);
                    EditorGUILayout.BeginFoldoutHeaderGroup(false, bundle);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                CustomBundleMapGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        private void CustomBundleMapGUI()
        {
            ArraySizeField("Custom Bundle Map Size", ref m_BuildSettings.CustomBuildMap, 
                (size) => Array.Resize(ref m_ShowAssetBuild, size));

            for (int i = 0; i < m_BuildSettings.CustomBuildMap.Length; i++)
            {
                if (m_BuildSettings.CustomBuildMap[i] == null)
                    m_BuildSettings.CustomBuildMap[i] = new AssetBuildPlan();
                AssetBuildPlan bundleBuild = m_BuildSettings.CustomBuildMap[i];

                m_ShowAssetBuild[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowAssetBuild[i], bundleBuild.BundleName);
                if (m_ShowAssetBuild[i])
                {
                    bundleBuild.BundleName = EditorGUILayout.TextField("Bundle Name", bundleBuild.BundleName);
                    ArraySizeField("Bundle Size", ref bundleBuild.AssetNames);

                    for (int j = 0; j < bundleBuild.AssetNames.Length; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("-", GUILayout.Width(10));
                        bundleBuild.AssetNames[j] = EditorGUILayout.TextField(bundleBuild.AssetNames[j], GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
                        {
                            string fullPath = EditorUtility.OpenFilePanel("Select asset in project", Application.dataPath, string.Empty);
                            bundleBuild.AssetNames[j] = $"Assets/{PathUtils.GetRelativePath(fullPath, Application.dataPath, true, PathSeparatorType.ForwardSlash)}";
                            GUI.FocusControl("");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void BuildScenariosGUI()
        {
            GUIStyle scenarioStyle = new GUIStyle() { margin = new RectOffset(10, 0, 0, 15) };

            GUILayout.Label("Build Scenarios", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (AssetBuildScenario scenario in m_BuildSettings.BuildScenarios)
            {
                EditorGUILayout.BeginVertical(scenarioStyle);
                scenario.Activated = EditorGUILayout.BeginToggleGroup(scenario.Name, scenario.Activated);
                scenario.OutputPath = EditorGUILayout.TextField("Output Path", scenario.OutputPath);
                scenario.BuildOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("Build Options", scenario.BuildOptions);
                scenario.TargetPlatform = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", scenario.TargetPlatform);
                if (GUILayout.Button("Remove Scenario", GUILayout.MaxWidth(150)))
                {
                    m_ScenarioToRemove = m_BuildSettings.BuildScenarios.IndexOf(scenario);
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }
            if (m_ScenarioToRemove >= 0)
            {
                RemoveScenario(m_ScenarioToRemove);
                m_ScenarioToRemove = -1;
                GUI.FocusControl("");
            }
        }

        private void NewScenarioGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(m_ScenarioToAdd);
            if (GUILayout.Button("Add New Scenario", GUILayout.MaxWidth(200)))
            {
                AddNewScenario(m_ScenarioToAdd);
                m_ScenarioToAdd = string.Empty;
            }
            GUI.enabled = true;
            m_ScenarioToAdd = EditorGUILayout.TextField(" => New Scenario Name:", m_ScenarioToAdd);
            EditorGUILayout.EndHorizontal();
        }

        private void ActionButtonsGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Asset Bundles", GUILayout.MaxWidth(200)))
            {
                m_AskBuild = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Rebuild Asset Bundles", GUILayout.MaxWidth(200)))
            {
                m_AskRebuild = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Clean Asset Bundles", GUILayout.MaxWidth(200)))
            {
                m_AskClean = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        #region utils
        private void ArraySizeField<T>(string label, ref T[] array, Action<int> additionalResize = null)
        {
            int newSize = EditorGUILayout.IntField(label, array.Length);
            if (newSize != array.Length)
            {
                Array.Resize(ref array, newSize);
                additionalResize?.Invoke(newSize);
            }   
        }

        private void AddNewScenario(string scenarioName)
        {
            AssetBuildScenario newScenario = new AssetBuildScenario() { Name = scenarioName, Activated = true };
            m_BuildSettings.BuildScenarios.Add(newScenario);
            ResizeWindow();
        }

        private void RemoveScenario(int scenarioIndex)
        {
            m_BuildSettings.BuildScenarios.RemoveAt(scenarioIndex);
            ResizeWindow();
        }

        private void ResizeWindow()
        {
            minSize = new Vector2(600, 400 + 110 * m_BuildSettings.BuildScenarios.Count);
        }
        #endregion
    }
}
