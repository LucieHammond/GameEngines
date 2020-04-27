﻿using GameEngine.PJR.Process;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Dependencies;
using GameEngine.PJR.Rules.Dependencies.Attributes;
using System;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameRule : GameRule
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

        public DummyGameRule() : base()
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
            OnInitialize?.Invoke();
            InitializeCallCount++;
        }

        protected override void Update()
        {
            OnUpdate?.Invoke();
            UpdateCallCount++;
        }

        protected override void Unload()
        {
            OnUnload?.Invoke();
            UnloadCallCount++;
        }

        protected override void OnQuit()
        {
            OnOnQuit?.Invoke();
            OnQuitCallCount++;
            base.OnQuit();
        }
    }

    public interface IDummyGameRuleBis
    {

    }

    [DependencyProvider(typeof(IDummyGameRuleBis))]
    public class DummyGameRuleBis : DummyGameRule, IDummyGameRuleBis
    {
    }

    public class DummyGameRuleTer : DummyGameRule
    {
        [DependencyConsumer(DependencyType.Service, true)]
        public IDummyGameService DummyServiceReference;

        [DependencyConsumer(DependencyType.Rule, false)]
        public IDummyGameRuleBis DummyRuleBisReference;
    }
}