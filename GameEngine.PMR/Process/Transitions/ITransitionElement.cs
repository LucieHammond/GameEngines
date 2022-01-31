namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// An interface to implement for defining a custom transition element handled by the StandardTransition
    /// </summary>
    public interface ITransitionElement
    {
        /// <summary>
        /// Called when the transition starts its entry phase
        /// </summary>
        void OnStartTransitionEntry();

        /// <summary>
        /// Called at each frame update during the transition entry phase
        /// </summary>
        void UpdateTransitionEntry();

        /// <summary>
        /// Called when the transition ends its entry phase
        /// </summary>
        void OnFinishTransitionEntry();

        /// <summary>
        /// Called when the transition starts its exit phase
        /// </summary>
        void OnStartTransitionExit();

        /// <summary>
        /// Called at each frame update during the transition exit phase
        /// </summary>
        void UpdateTransitionExit();

        /// <summary>
        /// Called when the transition ends its exit phase
        /// </summary>
        void OnFinishTransitionExit();

        /// <summary>
        /// Called at each frame update when the transition is activated (i.e fully displayed)
        /// </summary>
        /// <param name="loadingProgress">The current loading progress, as a float number between 0 and 1</param>
        /// <param name="loadingAction">The current loading action</param>
        void UpdateRunningTransition(float loadingProgress, string loadingAction);
    }
}
