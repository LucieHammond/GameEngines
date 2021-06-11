using GameEngine.PMR.Modules.Transitions;
using System;

namespace GameEnginesTest.Tools.Mocks.Spies
{
    public class SpyTransitionActivity : TransitionActivity
    {
        public int InitializeCallCount { get; private set; }
        public int StartCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int StopCallCount { get; private set; }
        public int CleanupCallCount { get; private set; }

        public Action OnInitialize;
        public Action OnStart;
        public Action OnUpdate;
        public Action OnStop;
        public Action OnCleanup;

        public float LoadingProgress => m_LoadingProgress;

        public string LoadingAction => m_LoadingAction;

        public bool UseDefaultReport { get => m_UseDefaultReport; set => m_UseDefaultReport = value; }

        public SpyTransitionActivity() : base()
        {
            ResetCount();
        }

        public void ResetCount()
        {
            InitializeCallCount = 0;
            StartCallCount = 0;
            UpdateCallCount = 0;
            StopCallCount = 0;
            CleanupCallCount = 0;
        }

        public void SetAutomaticCompletion()
        {
            OnStart += MarkStartCompleted;
            OnStop += MarkStopCompleted;
        }

        public void CallMarkStartCompleted()
        {
            MarkStartCompleted();
        }

        public void CallMarkStopCompleted()
        {
            MarkStopCompleted();
        }

        protected override void Initialize()
        {
            InitializeCallCount++;
            OnInitialize?.Invoke();
        }

        protected override void Start()
        {
            StartCallCount++;
            OnStart?.Invoke();
        }

        protected override void Update()
        {
            UpdateCallCount++;
            OnUpdate?.Invoke();
        }

        protected override void Stop()
        {
            StopCallCount++;
            OnStop?.Invoke();
        }

        protected override void Cleanup()
        {
            CleanupCallCount++;
            OnCleanup?.Invoke();
        }
    }
}
