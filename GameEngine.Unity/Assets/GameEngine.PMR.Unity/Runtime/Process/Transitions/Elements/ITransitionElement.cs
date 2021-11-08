namespace GameEngine.PMR.Process.Transitions.Elements
{
    /// <summary>
    /// An interface to implement for defining a custom transition element managed by a ScreenTransition or a WorldTransition
    /// </summary>
    public interface ITransitionElement
    {
        /// <summary>
        /// Called when the transition starts its activation phase (during which it appears with a fade)
        /// </summary>
        void OnStartTransitionActivation();

        /// <summary>
        /// Called when the transition ends its activation phase (during which it appears with a fade)
        /// </summary>
        void OnFinishTransitionActivation();

        /// <summary>
        /// Called at each frame update when the transition is activated (i.e fully displayed)
        /// </summary>
        void UpdateTransition();

        /// <summary>
        /// Called when the transition starts its deactivation phase (during which it disappears with a fade)
        /// </summary>
        void OnStartTransitionDeactivation();

        /// <summary>
        /// Called when the transition ends its deactivation phase (during which it disappears with a fade)
        /// </summary>
        void OnFinishTransitionDeactivation();
    }
}
