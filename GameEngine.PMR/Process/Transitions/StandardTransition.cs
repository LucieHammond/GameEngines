using System.Collections.Generic;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// A simple predefined transition with a scheduled display cycle against which custom variation elements are managed
    /// </summary>
    public class StandardTransition : Transition
    {
        /// <summary>
        /// A list of custom transition elements that should be managed by the transition
        /// </summary>
        protected List<ITransitionElement> m_CustomElements;

        private float m_EntryTimeTotal;
        private float m_DisplayTimeTotal;
        private float m_ExitTimeTotal;

        private float m_EntryTimeLeft;
        private float m_DisplayTimeLeft;
        private float m_ExitTimeLeft;

        /// <summary>
        /// <see cref="Transition.UpdateDuringEntry"/>
        /// </summary>
        public override bool UpdateDuringEntry => false;

        /// <summary>
        /// <see cref="Transition.UpdateDuringExit"/>
        /// </summary>
        public override bool UpdateDuringExit => true;

        /// <summary>
        /// Create an instance of StandardTransition
        /// </summary>
        public StandardTransition()
        {
            m_EntryTimeTotal = 0;
            m_DisplayTimeTotal = 0;
            m_ExitTimeTotal = 0;

            m_CustomElements = new List<ITransitionElement>();
        }

        /// <summary>
        /// Predefine the display, appearance and disappearance times of the transition
        /// </summary>
        /// <param name="displayTime">The minimum time during which the transition must be displayed (in seconds)</param>
        /// <param name="entryTime">The time it should take to make the transition appear (in seconds)</param>
        /// <param name="exitTime">The time it should take to make the transition disappear (in seconds)</param>
        public void SetTransitionTimes(float displayTime, float entryTime, float exitTime)
        {
            m_EntryTimeTotal = entryTime;
            m_DisplayTimeTotal = displayTime;
            m_ExitTimeTotal = exitTime;
        }

        /// <summary>
        /// Add a custom transition element to be managed by the transition
        /// </summary>
        /// <param name="element">The transition element to associate with the transition</param>
        public void AddTransitionElement(ITransitionElement element)
        {
            m_CustomElements.Add(element);
        }

        /// <summary>
        /// <see cref="Transition.Prepare()"/>
        /// </summary>
        protected override void Prepare() 
        {
            MarkReady();
        }

        /// <summary>
        /// <see cref="Transition.Enter()"/>
        /// </summary>
        protected override void Enter()
        {
            m_EntryTimeLeft = m_EntryTimeTotal;
            m_DisplayTimeLeft = m_DisplayTimeTotal;
            m_ExitTimeLeft = m_ExitTimeTotal;

            OnStartEntry();
        }

        /// <summary>
        /// <see cref="Transition.Update()"/>
        /// </summary>
        protected override void Update()
        {
            if (State == TransitionState.Entering)
            {
                UpdateEntry();

                if (m_EntryTimeLeft > 0)
                    m_EntryTimeLeft -= m_Time.DeltaTime;
                else
                    OnFinishEntry();
            }
            else if (State == TransitionState.Running)
            {
                UpdateRunning();

                if (!IsComplete)
                {
                    if (m_DisplayTimeLeft > 0)
                        m_DisplayTimeLeft -= m_Time.DeltaTime;
                    else
                        MarkCompleted();
                }
            }
            else if (State == TransitionState.Exiting)
            {
                UpdateExit();

                if (m_ExitTimeLeft > 0)
                    m_ExitTimeLeft -= m_Time.DeltaTime;
                else
                    OnFinishExit();
            }
        }

        /// <summary>
        /// <see cref="Transition.Exit()"/>
        /// </summary>
        protected override void Exit()
        {
            if (m_DisplayTimeLeft <= 0)
                OnStartExit();
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup() { }

        private void OnStartEntry()
        {
            m_CustomElements.ForEach((element) => element.OnStartTransitionEntry());
        }

        private void UpdateEntry()
        {
            m_CustomElements.ForEach((element) => element.UpdateTransitionEntry());
        }

        private void OnFinishEntry()
        {
            m_CustomElements.ForEach((element) => element.OnFinishTransitionEntry());
            MarkEntered();
        }

        private void UpdateRunning()
        {
            m_CustomElements.ForEach((element) => element.UpdateRunningTransition(m_LoadingProgress, m_LoadingAction));
        }

        private void OnStartExit()
        {
            m_CustomElements.ForEach((element) => element.OnStartTransitionExit());
        }

        private void UpdateExit()
        {
            m_CustomElements.ForEach((element) => element.UpdateTransitionExit());
        }

        private void OnFinishExit()
        {
            m_CustomElements.ForEach((element) => element.OnFinishTransitionExit());
            MarkExited();
        }
    }
}
