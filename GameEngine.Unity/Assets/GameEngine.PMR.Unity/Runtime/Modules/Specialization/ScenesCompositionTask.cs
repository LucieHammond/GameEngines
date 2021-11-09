using GameEngine.PMR.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEngine.PMR.Modules.Specialization
{
    /// <summary>
    /// A module configuration task dedicated to the loading and unloading of the scenes requested by the module rules
    /// </summary>
    public class ScenesCompositionTask : SpecialTask
    {
        private List<string> m_ScenesPool;
        private int m_CurrentPoolIndex;
        private bool m_Running;
        private Action m_AwaitingAction;

        /// <summary>
        /// Create a new instance of ScenesCompositionTask
        /// </summary>
        public ScenesCompositionTask() : base()
        {
            m_Running = false;
        }

        /// <summary>
        /// Get an estimate of the progress of the running task
        /// </summary>
        /// <returns>A floating number between 0 and 1 representing the progress of the task</returns>
        public override float GetProgress()
        {
            return State == SpecialTaskState.InitRunning ? m_CurrentPoolIndex / m_ScenesPool.Count : 0;
        }

        /// <summary>
        /// Launch the initialization phase of the task, where the scenes are loaded
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected override void Initialize(RulesDictionary rules)
        {
            if (m_Running)
            {
                m_AwaitingAction = () => Initialize(rules);
                return;
            }

            m_Running = true;
            m_ScenesPool = GetAllRequiredScenes(rules);
            m_CurrentPoolIndex = 0;

            LoadScenesAsync();
        }

        /// <summary>
        /// Launch the unload phase of the task, where the scenes are unloaded
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected override void Unload(RulesDictionary rules)
        {
            if (m_Running)
            {
                m_AwaitingAction = () => Unload(rules);
                return;
            }

            m_Running = true;
            UnloadScenesAsync();
        }

        /// <summary>
        /// Update the task if necessary
        /// </summary>
        /// <param name="maxDuration">The maximum time (in ms) the update should take</param>
        protected override void Update(int maxDuration)
        {
            if (!m_Running && m_AwaitingAction != null)
            {
                m_AwaitingAction();
                m_AwaitingAction = null;
            }
        }

        private List<string> GetAllRequiredScenes(RulesDictionary rules)
        {
            HashSet<string> requiredScenes = new HashSet<string>();
            foreach (KeyValuePair<Type, GameRule> rule in rules)
            {
                if (rule.Value is ISceneGameRule sceneRule)
                    requiredScenes.UnionWith(sceneRule.RequiredScenes);
            }
            return new List<string>(requiredScenes);
        }

        private void LoadScenesAsync()
        {
            if (State != SpecialTaskState.InitRunning)
            {
                m_Running = false;
                return;
            }

            if (m_CurrentPoolIndex < m_ScenesPool.Count)
            {
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(m_ScenesPool[m_CurrentPoolIndex], LoadSceneMode.Additive);
                loadOperation.completed += (_) =>
                {
                    m_CurrentPoolIndex++;
                    LoadScenesAsync();
                };
            }
            else
            {
                FinishInitialization();
                m_Running = false;
            }
        }

        private void UnloadScenesAsync()
        {
            if (State != SpecialTaskState.UnloadRunning)
            {
                m_Running = false;
                return;
            }

            if (m_CurrentPoolIndex > 0)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(m_ScenesPool[m_CurrentPoolIndex - 1]);
                unloadOperation.completed += (_) =>
                {
                    m_CurrentPoolIndex--;
                    UnloadScenesAsync();
                };
            }
            else
            {
                FinishUnload();
                m_Running = false;
            }
        }
    }
}
