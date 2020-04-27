using GameEngine.PJR.Jobs;
using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Dependencies;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.PJR
{
    /// <summary>
    /// Component tests for the class GameJob
    /// <see cref="GameJob"/>
    /// </summary>
    [TestClass]
    public class GameJobTest
    {
        private MockProcessTime m_Time;
        private GameProcess m_Process;
        private Dictionary<string, object> m_Configuration;

        [TestInitialize]
        public void Initialize()
        {
            // Create the process and the configuration that will be used in every test
            m_Time = new MockProcessTime();
            m_Process = new GameProcess(new DummyGameProcessSetup(), m_Time);
            m_Process.ServiceProvider = new DependencyProvider();
            m_Process.ServiceProvider.Add(typeof(IProcessAccessor), new ProcessService(m_Process));
            m_Configuration = new Dictionary<string, object>();
        }

        [TestMethod]
        public void IsConfiguredUsingSetup()
        {
            // A GameMode job is created -> Name, Configuration and Parent can be retrieved immediately
            GameJob modeJob = new GameJob(new DummyGameModeSetup(), m_Configuration, m_Process);
            Assert.AreEqual("TestMode", modeJob.Name);
            Assert.IsFalse(modeJob.IsServiceJob);
            Assert.AreEqual(m_Configuration, modeJob.Configuration);
            Assert.AreEqual(m_Process, modeJob.ParentProcess);

            // A GameMode job is created -> informations related to the setup are configured during Setup state
            modeJob.Start();
            Assert.IsTrue(RunJobState(modeJob, GameJobState.Setup));
            Assert.AreNotEqual(0, modeJob.Rules.Count);
            Assert.IsNotNull(modeJob.InitUnloadOrder);
            Assert.IsNotNull(modeJob.UpdateScheduler);
            Assert.IsNotNull(modeJob.ErrorPolicy);
            Assert.IsNotNull(modeJob.PerformancePolicy);

            // A service job is created -> Name, Configuration and Parent can be retrieved immediately
            GameJob serviceJob = new GameJob(new DummyServiceSetup(), m_Configuration, m_Process);
            Assert.AreEqual("TestServices", serviceJob.Name);
            Assert.IsTrue(serviceJob.IsServiceJob);
            Assert.AreEqual(m_Configuration, serviceJob.Configuration);
            Assert.AreEqual(m_Process, serviceJob.ParentProcess);

            // A service job is created -> informations related to the setup are configured during Setup state
            serviceJob.Start();
            Assert.IsTrue(RunJobState(serviceJob, GameJobState.Setup));
            Assert.AreNotEqual(0, serviceJob.Rules.Count);
            Assert.IsNotNull(serviceJob.InitUnloadOrder);
            Assert.IsNotNull(serviceJob.UpdateScheduler);
            Assert.IsNotNull(serviceJob.ErrorPolicy);
            Assert.IsNotNull(serviceJob.PerformancePolicy);

            // InitUnload and UpdateScheduler contains rule types that are not part of the setup -> a simplification is made
            DummyGameModeSetup simpleSetup = new DummyGameModeSetup();
            simpleSetup.CustomRules = new List<GameRule>() { new DummyGameRuleBis() };
            GameJob simpleJob = new GameJob(simpleSetup, m_Configuration, m_Process);
            simpleJob.Start();
            Assert.IsTrue(RunJobState(simpleJob, GameJobState.Setup));
            Assert.AreEqual(1, simpleJob.InitUnloadOrder.Count);
            Assert.AreEqual(typeof(DummyGameRuleBis), simpleJob.InitUnloadOrder[0]);
            Assert.AreEqual(1, simpleJob.UpdateScheduler.Count);
            Assert.AreEqual(typeof(DummyGameRuleBis), simpleJob.UpdateScheduler[0].RuleType);

            // InitUnload and UpdateScheduler contains duplicates -> the setup state doesn't end up correctly due to a catched exception pausing the job
            DummyGameModeSetup duplicateSetup = new DummyGameModeSetup();
            duplicateSetup.CustomInitUnloadOrder = new List<Type>() { typeof(DummyGameRule), typeof(DummyGameRuleBis), typeof(DummyGameRule) };
            GameJob duplicateJob = new GameJob(duplicateSetup, m_Configuration, m_Process);
            duplicateJob.Start();
            Assert.IsFalse(RunJobState(simpleJob, GameJobState.Setup));
        }

        [TestMethod]
        public void ManageRuleDependencies()
        {
            // Create job with DummyGameRuleTer, which has required dependency on IDummyGameService and optional dependency on IDummyGameRuleBis
            DummyGameRuleTer rule = new DummyGameRuleTer();
            DummyGameModeSetup jobSetup = new DummyGameModeSetup();
            jobSetup.CustomRules = new List<GameRule>() { rule };
            GameJob job = new GameJob(jobSetup, m_Configuration, m_Process);

            // GameMode job contains DummyGameRuleTer and IDummyGameService is not in ServiceProvider -> DependencyInjection state does not end correctly
            Assert.IsFalse(m_Process.ServiceProvider.TryGet(typeof(IDummyGameService), out object _));
            job.Start();
            Assert.IsTrue(RunJobState(job, GameJobState.Setup));
            Assert.IsFalse(RunJobState(job, GameJobState.DependencyInjection));

            // Service job contains DummyGameService providing dependency for IDummyGameService -> interface appears in ServiceProvider after DependencyInjection state
            GameJob serviceJob = new GameJob(new DummyServiceSetup(), m_Configuration, m_Process);
            serviceJob.Start();
            Assert.IsTrue(RunJobState(serviceJob, GameJobState.Setup));
            Assert.IsTrue(RunJobState(serviceJob, GameJobState.DependencyInjection));
            Assert.IsTrue(m_Process.ServiceProvider.TryGet(typeof(IDummyGameService), out object _));

            // GameMode job contains only DummyGameRuleTer and IDummyGameService is in ServiceProvider -> DependencyInjection state succeeds
            // - required dependency on IDummyGameService is set correctly
            // - optional dependency on IDummyGameRuleBis is not set
            job.Restart();
            Assert.IsTrue(RunJobState(job, GameJobState.DependencyInjection));
            Assert.IsNotNull(rule.DummyServiceReference);
            Assert.IsNull(rule.DummyRuleBisReference);

            // GameMode job contains DummyGameRuleTer and DummyGameRuleBis, IDummyGameService is in ServiceProvider -> all dependencies are correctly injected
            DummyGameRuleBis ruleBis = new DummyGameRuleBis();
            DummyGameRuleTer ruleTer = new DummyGameRuleTer();
            DummyGameModeSetup correctJobSetup = new DummyGameModeSetup();
            correctJobSetup.CustomRules = new List<GameRule>() { ruleBis, ruleTer };
            GameJob correctJob = new GameJob(correctJobSetup, m_Configuration, m_Process);
            correctJob.Start();
            Assert.IsTrue(RunJobState(correctJob, GameJobState.Setup));
            Assert.IsTrue(RunJobState(correctJob, GameJobState.DependencyInjection));
            Assert.IsNotNull(ruleTer.DummyServiceReference);
            Assert.AreEqual(ruleBis, ruleTer.DummyRuleBisReference);
        }

        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            // Create and start a GameJob (use an empty GameJob to avoid errors due to interactions with rules)
            GameJob job = CreateEmptyGameJob();
            job.Start();

            // Perform Setup State
            Assert.AreEqual(GameJobState.Setup, job.State);
            Assert.IsTrue(job.IsLoading);
            Assert.AreEqual(0, job.LoadingProgress);
            Assert.IsTrue(RunJobState(job, GameJobState.Setup));

            // Perform DependencyInjection State
            Assert.AreEqual(GameJobState.DependencyInjection, job.State);
            Assert.IsTrue(job.IsLoading);
            Assert.IsTrue(RunJobState(job, GameJobState.DependencyInjection));

            // Perform InitializeRules State
            Assert.AreEqual(GameJobState.InitializeRules, job.State);
            Assert.IsTrue(job.IsLoading);
            Assert.IsTrue(RunJobState(job, GameJobState.InitializeRules));
            Assert.AreEqual(1, job.LoadingProgress);

            // Perform UpdateRules State
            Assert.AreEqual(GameJobState.UpdateRules, job.State);
            Assert.IsFalse(job.IsLoading);
            Assert.IsTrue(job.IsOperational);
            job.Update();
            m_Time.GoToNextFrame();

            // Perform UnloadRules State
            job.Unload();
            job.Update();
            Assert.AreEqual(GameJobState.UnloadRules, job.State);
            Assert.IsFalse(job.IsOperational);
            Assert.IsTrue(job.IsUnloading);
            Assert.IsTrue(RunJobState(job, GameJobState.UnloadRules));

            // End up in End state
            Assert.AreEqual(GameJobState.End, job.State);
            Assert.IsFalse(job.IsUnloading);
            Assert.IsTrue(job.IsFinished);
            job.Stop();
        }

        [TestMethod]
        public void CanBePausedAndRestarted()
        {
            // Create and start an empty GameJob
            GameJob job = CreateEmptyGameJob();
            job.Start();

            // The job is paused -> no progression is made and the RunState operation timeout
            job.Pause();
            Assert.IsFalse(RunJobState(job, GameJobState.Setup));

            // The job is restarted -> operation resume without error
            job.Restart();
            Assert.IsTrue(RunJobState(job, GameJobState.Setup));

            // Do it again in another state
            job.Pause();
            Assert.IsFalse(RunJobState(job, GameJobState.DependencyInjection));
            job.Restart();
            Assert.IsTrue(RunJobState(job, GameJobState.DependencyInjection));
        }

        [TestMethod]
        public void CanBeUnloadedAtAnyTime()
        {
            // Unload during Setup
            GameJob job = CreateEmptyGameJob();
            job.Start();
            Assert.AreEqual(GameJobState.Setup, job.State);
            job.Unload();
            job.Update();
            Assert.AreEqual(GameJobState.End, job.State);

            // Unload during DependencyInjection
            job = CreateEmptyGameJob();
            job.Start();
            RunJobState(job, GameJobState.Setup);
            Assert.AreEqual(GameJobState.DependencyInjection, job.State);
            job.Unload();
            job.Update();
            Assert.AreEqual(GameJobState.End, job.State);

            // Unload during InitializeRules
            job = CreateEmptyGameJob();
            job.Start();
            RunJobState(job, GameJobState.Setup);
            RunJobState(job, GameJobState.DependencyInjection);
            Assert.AreEqual(GameJobState.InitializeRules, job.State);
            job.Unload();
            job.Update();
            Assert.AreEqual(GameJobState.UnloadRules, job.State);
            Assert.IsTrue(RunJobState(job, GameJobState.UnloadRules));

            // Trying to unload during UnloadRules or End will do nothing
            job = CreateEmptyGameJob();
            job.Start();
            RunJobState(job, GameJobState.Setup);
            RunJobState(job, GameJobState.DependencyInjection);
            RunJobState(job, GameJobState.InitializeRules);
            job.Unload();
            job.Update();
            RunJobState(job, GameJobState.UnloadRules);
            Assert.AreEqual(GameJobState.End, job.State);
            job.Unload();
            job.Update();
            Assert.AreEqual(GameJobState.End, job.State);

            // Quit abruptly
            job = CreateEmptyGameJob();
            job.Start();
            job.OnQuit();
        }

        private GameJob CreateEmptyGameJob()
        {
            DummyGameModeSetup emptySetup = new DummyGameModeSetup();
            emptySetup.CustomRules = new List<GameRule>();
            return new GameJob(emptySetup, m_Configuration, m_Process);
        }

        // Simulate the execution of a given state by a job
        // Return false if the job is not currently in that state or if the execution takes more that maxFrames to execute (an error probably paused the job)
        private bool RunJobState(GameJob job, GameJobState state, int maxFrames = 10)
        {
            if (job.State != state)
                return false;

            for (int i = 0; i < maxFrames; i++)
            {
                job.Update();
                m_Time.GoToNextFrame();
                if (job.State != state)
                    return true;
            }
            return false;
        }
    }
}
