using GameEngine.PMR.Process.Transitions;
using UnityEngine;

namespace GameEngine.PMR.Unity.Transitions.Elements
{
    /// <summary>
    /// A transition element specialized in managing the pop and depop animation of a graphic component
    /// </summary>
    public class AnimatedElement : ITransitionElement
    {
        private Animator m_Animator;

        private float m_PopAnimDelay;
        private string m_PopTrigger;
        private float m_DepopAnimDelay;
        private string m_DepopTrigger;

        /// <summary>
        /// Create a new instance of AnimatedElement
        /// </summary>
        /// <param name="animator">The animator component of the animated graphic object</param>
        /// <param name="popTrigger">The trigger parameter of the pop animation</param>
        /// <param name="depopTrigger">The trigger parameter of the depop animation</param>
        /// <param name="popAnimDelay">The delay in starting the pop animation (in seconds)</param>
        /// <param name="depopAnimDelay">The delay in starting the depop animation (in seconds)</param>
        public AnimatedElement(Animator animator, string popTrigger, string depopTrigger, float popAnimDelay = 0, float depopAnimDelay = 0)
        {
            m_Animator = animator;

            m_PopAnimDelay = popAnimDelay;
            m_PopTrigger = popTrigger;
            m_DepopAnimDelay = depopAnimDelay;
            m_DepopTrigger = depopTrigger;
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionEntry()"/>
        /// </summary>
        public void OnStartTransitionEntry()
        {
            if (m_PopAnimDelay <= 0)
                m_Animator.SetTrigger(m_PopTrigger);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionEntry()"/>
        /// </summary>
        public void UpdateTransitionEntry()
        {
            if (m_PopAnimDelay > 0)
            {
                m_PopAnimDelay -= Time.deltaTime;
                if (m_PopAnimDelay <= 0)
                    m_Animator.SetTrigger(m_PopTrigger);
            }
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionEntry()"/>
        /// </summary>
        public void OnFinishTransitionEntry()
        {
            m_Animator.gameObject?.SetActive(true);
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionExit()"/>
        /// </summary>
        public void OnStartTransitionExit()
        {
            if (m_DepopAnimDelay <= 0)
                m_Animator.SetTrigger(m_DepopTrigger);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionExit()"/>
        /// </summary>
        public void UpdateTransitionExit()
        {
            if (m_DepopAnimDelay > 0)
            {
                m_DepopAnimDelay -= Time.deltaTime;
                if (m_DepopAnimDelay <= 0)
                    m_Animator.SetTrigger(m_DepopTrigger);
            }
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionExit()"/>
        /// </summary>
        public void OnFinishTransitionExit()
        {
            m_Animator.gameObject?.SetActive(false);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateRunningTransition(float, string)"/>
        /// </summary>
        public void UpdateRunningTransition(float loadingProgress, string loadingAction) { }
    }
}