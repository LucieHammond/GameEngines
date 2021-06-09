﻿using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Rules;
using System;

namespace GameEnginesTest.Tools.Mocks.Spies
{
    public class SpyGameRule : GameRule
    {
        public int InitializeCallCount { get; private set; }
        public int UnloadCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int OnQuitCallCount { get; private set; }

        public Action OnInitialize;
        public Action OnUnload;
        public Action OnUpdate;
        public Action OnOnQuit;

        public GameProcess Process => m_Process;

        public GameModule CurrentModule => m_CurrentModule;

        public SpyGameRule() : base()
        {
            InitializeCallCount = 0;
            UnloadCallCount = 0;
            UpdateCallCount = 0;
            OnQuitCallCount = 0;
        }

        public void CallMarkInitialized()
        {
            MarkInitialized();
        }

        public void CallMarkUnloaded()
        {
            MarkUnloaded();
        }

        public void CallMarkError()
        {
            MarkError();
        }

        protected override void Initialize()
        {
            InitializeCallCount++;
            OnInitialize?.Invoke();
        }

        protected override void Update()
        {
            UpdateCallCount++;
            OnUpdate?.Invoke();
        }

        protected override void Unload()
        {
            UnloadCallCount++;
            OnUnload?.Invoke();
        }

        protected override void OnQuit()
        {
            OnQuitCallCount++;
            OnOnQuit?.Invoke();
            base.OnQuit();
        }
    }
}