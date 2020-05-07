using GameEngine.Core.Model;
using GameEngine.PJR.Jobs;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules.Dependencies;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Mocks;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameEnginesTest.IntegrationTests
{
    /// <summary>
    /// Integration tests focusing on the interactions between GameJobs and GameRules
    /// </summary>
    [TestClass]
    public class JobsAndRulesTest
    {
        private MockProcessTime m_Time;
        private List<DummyGameRule> m_Rules;
        private GameJob m_Job;

        [TestInitialize]
        public void Initialize()
        {
            // Create the rules
            m_Rules = new List<DummyGameRule>()
            {
                new DummyGameRule(),
                new DummyGameRuleBis(),
                new DummyGameRuleTer()
            };

            // Create the job
            m_Time = new MockProcessTime();
            DummyGameModeSetup setup = new DummyGameModeSetup();
            setup.CustomRules = m_Rules;

            GameProcess process = new GameProcess(new DummyGameProcessSetup(), m_Time);
            process.ServiceProvider = new DependencyProvider();
            process.ServiceProvider.Add(typeof(IProcessAccessor), new ProcessService(process));
            process.ServiceProvider.Add(typeof(IDummyGameService), new DummyGameService());
            Configuration configuration = new Configuration();

            m_Job = new GameJob(setup, configuration, process);
            process.CurrentGameMode = m_Job;
        }

        [TestMethod]
        public void JobIsStarted_RulesAreInitialized()
        {
            m_Job.Start();

            // The rules's Initialize methods are called in the order defined by InitUnloadOrder
            List<int> rulesInitialized = new List<int>();
            m_Rules[0].OnInitialize += () => { rulesInitialized.Add(0); m_Rules[0].CallMarkInitialized(); };
            m_Rules[1].OnInitialize += () => { rulesInitialized.Add(1); m_Rules[1].CallMarkInitialized(); };
            m_Rules[2].OnInitialize += () => { rulesInitialized.Add(2); m_Rules[2].CallMarkInitialized(); };

            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => rulesInitialized.Count == 3));
            Assert.AreEqual(0, rulesInitialized[0]);
            Assert.AreEqual(1, rulesInitialized[1]);
            Assert.AreEqual(2, rulesInitialized[2]);
        }

        [TestMethod]
        public void RulesAreMarkedInitialized_JobIsOperational()
        {
            m_Job.Start();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.State == GameJobState.InitializeRules);
            
            // Rules will all declare themselves initialized when Initialize() is called
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();

            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational));
            Assert.AreEqual(1, m_Job.LoadingProgress);
        }

        [TestMethod]
        public void JobIsOperational_RulesAreUpdated()
        {
            m_Job.Start();
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational && m_Time.FrameCount % 2 == 0);

            // Rules are updated according to the UpdateSchedule
            List<int> rulesUpdated = new List<int>();
            m_Rules[0].OnUpdate += () => rulesUpdated.Add(0);
            m_Rules[1].OnUpdate += () => rulesUpdated.Add(1);
            m_Rules[2].OnUpdate += () => rulesUpdated.Add(2);

            // { 1, 0 } on even frames
            m_Job.Update();
            m_Time.GoToNextFrame();
            Assert.AreEqual(2, rulesUpdated.Count);
            Assert.AreEqual(1, rulesUpdated[0]);
            Assert.AreEqual(0, rulesUpdated[1]);
            rulesUpdated.Clear();

            // { 0, 2 } on odd frames
            m_Job.Update();
            m_Time.GoToNextFrame();
            Assert.AreEqual(2, rulesUpdated.Count);
            Assert.AreEqual(0, rulesUpdated[0]);
            Assert.AreEqual(2, rulesUpdated[1]);
        }

        [TestMethod]
        public void JobIsUnloaded_RulesAreUnloaded()
        {
            m_Job.Start();
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational);

            // The rules's Unload methods are called following an order contrary to the InitUnloadOrder
            List<int> rulesUnloaded = new List<int>();
            m_Rules[0].OnUnload += () => { rulesUnloaded.Add(0); m_Rules[0].CallMarkUnloaded(); };
            m_Rules[1].OnUnload += () => { rulesUnloaded.Add(1); m_Rules[1].CallMarkUnloaded(); };
            m_Rules[2].OnUnload += () => { rulesUnloaded.Add(2); m_Rules[2].CallMarkUnloaded(); };

            m_Job.Unload(); 
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => rulesUnloaded.Count == 3));
            Assert.AreEqual(2, rulesUnloaded[0]);
            Assert.AreEqual(1, rulesUnloaded[1]);
            Assert.AreEqual(0, rulesUnloaded[2]);
        }

        [TestMethod]
        public void RulesAreMarkedUnloaded_JobIsFinished()
        {
            m_Job.Start();
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational);

            // Rules will all declare themselves unloaded when Unload() is called
            m_Rules[0].OnUnload += () => m_Rules[0].CallMarkUnloaded();
            m_Rules[1].OnUnload += () => m_Rules[1].CallMarkUnloaded();
            m_Rules[2].OnUnload += () => m_Rules[2].CallMarkUnloaded();

            m_Job.Unload();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            m_Job.Stop();
        }

        [TestMethod]
        public void JobQuits_AllRulesQuit()
        {
            m_Job.Start();
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational);

            m_Job.OnQuit();
            Assert.AreEqual(1, m_Rules[0].OnQuitCallCount);
            Assert.AreEqual(1, m_Rules[1].OnQuitCallCount);
            Assert.AreEqual(1, m_Rules[2].OnQuitCallCount);
        }

        [TestMethod]
        public void RuleReportAnError_JobIsUnloaded()
        {
            // The error is reported before initialization
            m_Job.Start();
            m_Rules[0].CallMarkError();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => !m_Job.IsLoading));
            Assert.IsTrue(m_Job.IsUnloading);
            Assert.AreEqual(0, m_Rules[0].InitializeCallCount);
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            Assert.AreEqual(0, m_Rules[0].UnloadCallCount);

            // The error is reported during initialization
            Initialize();
            SetMarkCallbacks();
            m_Job.Start();
            m_Rules[1].OnInitialize = () => m_Rules[1].CallMarkError();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => !m_Job.IsLoading));
            Assert.IsTrue(m_Job.IsUnloading);
            Assert.AreEqual(1, m_Rules[0].InitializeCallCount);
            Assert.AreEqual(1, m_Rules[1].InitializeCallCount);
            Assert.AreEqual(0, m_Rules[2].InitializeCallCount);
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            Assert.AreEqual(1, m_Rules[0].UnloadCallCount);
            Assert.AreEqual(0, m_Rules[1].UnloadCallCount);
            Assert.AreEqual(0, m_Rules[2].UnloadCallCount);

            // The error is reported during update
            Initialize();
            SetMarkCallbacks();
            m_Job.Start();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational);
            m_Rules[0].OnUpdate += () => m_Rules[0].CallMarkError();
            m_Job.Update();
            m_Time.GoToNextFrame();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => !m_Job.IsOperational));
            Assert.IsTrue(m_Job.IsUnloading);
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            Assert.AreEqual(1, m_Rules[0].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[1].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[2].UnloadCallCount);

            // The error is reported during unload
            Initialize();
            SetMarkCallbacks();
            m_Job.Start();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational);
            m_Job.Unload();
            m_Rules[0].OnUnload = () => m_Rules[0].CallMarkError();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            Assert.AreEqual(1, m_Rules[0].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[1].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[2].UnloadCallCount);
        }

        [TestMethod]
        public void RuleThrowException_JobHandleException()
        {
            // During load, OnExceptionBehaviour = PauseJob
            m_Job.Start();
            SetMarkCallbacks();
            m_Rules[1].OnInitialize += () => throw new Exception();
            Assert.IsFalse(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational, 5));
            Assert.AreEqual(1, m_Rules[0].InitializeCallCount);
            Assert.AreEqual(1, m_Rules[1].InitializeCallCount);
            Assert.AreEqual(0, m_Rules[2].InitializeCallCount);

            m_Job.Restart();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational && m_Time.FrameCount % 2 == 0));

            // During update, OnExceptionBehaviour = SkipFrame
            m_Rules[1].OnUpdate += () => throw new Exception();
            m_Job.Update();
            m_Time.GoToNextFrame();
            Assert.AreEqual(0, m_Rules[0].UpdateCallCount); // skipped
            Assert.AreEqual(1, m_Rules[1].UpdateCallCount);

            // During unload, OnExceptionBehaviour = Continue and SkipUnloadIfError = true
            m_Job.Unload();
            m_Rules[1].OnUnload = () => throw new Exception();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsFinished));
            Assert.AreEqual(1, m_Rules[0].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[1].UnloadCallCount);
            Assert.AreEqual(1, m_Rules[2].UnloadCallCount);
        }

        [TestMethod]
        public void RulesAreStalling_JobHandleTimeout()
        {
            // Set custom performance policy
            m_Job.Start();
            SetMarkCallbacks();
            m_Job.SimulateExecutionUntil(m_Time, () => m_Job.State == GameJobState.DependencyInjection);
            int stallingTimeout = 1;
            m_Job.PerformancePolicy = new PerformancePolicy()
            {
                CheckStallingRules = true,
                InitStallingTimeout = stallingTimeout,
                UpdateStallingTimeout = stallingTimeout,
                UnloadStallingTimeout = stallingTimeout
            };

            // Timeout exception is called during load, and provokes pause (OnExceptionBehaviour)
            object objLock = new object();
            m_Rules[1].OnInitialize = () => Task.Run(() => 
            {
                Thread.Sleep(stallingTimeout);
                m_Rules[1].CallMarkInitialized();
                lock (objLock)
                    Monitor.Pulse(objLock);
            });
            Assert.IsFalse(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational, 5));
            Assert.AreEqual(1, m_Rules[0].InitializeCallCount);
            Assert.AreEqual(1, m_Rules[1].InitializeCallCount);
            Assert.AreEqual(0, m_Rules[2].InitializeCallCount);

            lock (objLock)
                Monitor.Wait(objLock);
            m_Job.Restart();
            Assert.IsTrue(m_Job.SimulateExecutionUntil(m_Time, () => m_Job.IsOperational && m_Time.FrameCount % 2 == 0));

            // Timeout exception is called during update, and provokes skipFrame (OnExceptionBehaviour)
            m_Rules[1].OnUpdate += () => Thread.Sleep(stallingTimeout);
            m_Job.Update();
            m_Time.GoToNextFrame();
            Assert.AreEqual(0, m_Rules[0].UpdateCallCount); // skipped
            Assert.AreEqual(1, m_Rules[1].UpdateCallCount);
        }

        private void SetMarkCallbacks()
        {
            m_Rules[0].OnInitialize += () => m_Rules[0].CallMarkInitialized();
            m_Rules[1].OnInitialize += () => m_Rules[1].CallMarkInitialized();
            m_Rules[2].OnInitialize += () => m_Rules[2].CallMarkInitialized();

            m_Rules[0].OnUnload += () => m_Rules[0].CallMarkUnloaded();
            m_Rules[1].OnUnload += () => m_Rules[1].CallMarkUnloaded();
            m_Rules[2].OnUnload += () => m_Rules[2].CallMarkUnloaded();
        }
    }
}
