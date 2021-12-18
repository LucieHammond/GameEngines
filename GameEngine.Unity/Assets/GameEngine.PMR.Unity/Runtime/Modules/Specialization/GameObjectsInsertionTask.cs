using GameEngine.PMR.Modules.Specialization;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Unity.Rules;
using GameEngine.PMR.Unity.Rules.Dependencies;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Unity.Modules.Specialization
{
    /// <summary>
    /// A module configuration task dedicated to the injection of the GameObject dependencies requested by the rules
    /// </summary>
    public class GameObjectsInsertionTask : SpecialTask
    {
        private IEnumerator<KeyValuePair<Type, GameRule>> m_RulesEnumerator;
        private int m_NbRulesCovered;
        private int m_RulesCount;
        private Stopwatch m_UpdateTime;

        /// <summary>
        /// Create a new instance of GameObjectsInsertionTask
        /// </summary>
        public GameObjectsInsertionTask()
        {
            m_UpdateTime = new Stopwatch();
        }

        /// <summary>
        /// Get an estimate of the progress of the running task
        /// </summary>
        /// <returns>A floating number between 0 and 1 representing the progress of the task</returns>
        public override float GetProgress()
        {
            return m_RulesCount > 0 ? (float)m_NbRulesCovered / m_RulesCount : 0;
        }

        /// <summary>
        /// Launch the initialization phase of the task, where the gameobject dependencies are injected to the rules
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected override void Initialize(RulesDictionary rules)
        {
            m_NbRulesCovered = 0;
            m_RulesCount = rules.Count;
            m_RulesEnumerator = rules.GetEnumerator();
        }

        /// <summary>
        /// Launch the unload phase of the task, which does nothing
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected override void Unload(RulesDictionary rules)
        {
            FinishUnload();
        }

        /// <summary>
        /// Update the task
        /// </summary>
        /// <param name="maxDuration">The maximum time (in ms) the update should take</param>
        protected override void Update(int maxDuration)
        {
            m_UpdateTime.Restart();

            do
            {
                if (State == SpecialTaskState.InitRunning)
                    InsertGameObjects();
            }
            while (m_UpdateTime.ElapsedMilliseconds < maxDuration);

            m_UpdateTime.Stop();
        }

        private void InsertGameObjects()
        {
            if (!m_RulesEnumerator.MoveNext())
            {
                FinishInitialization();
                return;
            }

            m_NbRulesCovered++;
            GameRule rule = m_RulesEnumerator.Current.Value;

            if (rule is ISceneGameRule)
                ObjectDependencyOperations.InjectObjectDependencies(rule);
        }
    }
}
