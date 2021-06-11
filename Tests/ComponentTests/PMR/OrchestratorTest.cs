using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class Orchestrator
    /// <see cref="Orchestrator"/>
    /// </summary>
    [TestClass]
    public class OrchestratorTest
    {
        private FakeTime m_Time;

        public OrchestratorTest()
        {
            m_Time = new FakeTime();
        }

        [TestMethod]
        public void CanBeCreatedAndDestroyed()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string category, out GameProcess process);

            // Orchestrator parameters are correctly set
            Assert.AreEqual(category, orchestrator.Category);
            Assert.AreEqual(process, orchestrator.MainProcess);
            Assert.IsNull(orchestrator.Parent);
            Assert.AreEqual(0, orchestrator.Children.Count);

            // At creation, orchestrator is in Wait state with null CurrentModule and null CurrentTransition
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.IsFalse(orchestrator.IsOperational);
            Assert.IsNull(orchestrator.CurrentModule);
            Assert.IsNull(orchestrator.CurrentTransition);

            // A new orchestrator can be created as a child of an existing orchestrator
            Orchestrator childOrchestrator = new Orchestrator($"Sub-{category}", process, orchestrator);
            Assert.AreEqual(orchestrator, childOrchestrator.Parent);

            // An orchestrator can be stopped abruptly at any time
            childOrchestrator.OnQuit();
            orchestrator.OnQuit();
        }

        [TestMethod]
        public void ExecuteWithoutTransition()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNullTransition();
            GameModule createdModule;

            // Perform the LoadModule operation: Wait -> RunTransition -> Operational
            bool loadCompleted = false;
            orchestrator.OnOperational += () => loadCompleted = true;
            orchestrator.LoadModule(moduleSetup, null);
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.IsTrue(loadCompleted);

            // After the LoadModule sequence, CurrentModule has finished loading
            Assert.IsNotNull(orchestrator.CurrentModule);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);
            createdModule = orchestrator.CurrentModule;

            // Perform the ReloadModule operation: Operational -> RunTransition -> Operational
            bool reloadCompleted = false;
            orchestrator.OnOperational += () => reloadCompleted = true;
            orchestrator.ReloadModule();
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.IsTrue(reloadCompleted);

            // After the ReloadModule sequence, CurrentModule is fully reloaded
            Assert.IsNotNull(orchestrator.CurrentModule);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);
            Assert.AreEqual(createdModule, orchestrator.CurrentModule);

            // Perform the UnloadModule operation: Operational -> RunTransition -> Wait
            bool unloadCompleted = false;
            orchestrator.OnTerminated += () => unloadCompleted = true;
            orchestrator.UnloadModule();
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.IsTrue(unloadCompleted);

            // After the UnloadModule sequence, CurrentModule has finished unloading and is null again
            Assert.IsNull(orchestrator.CurrentModule);
            Assert.AreEqual(GameModuleState.End, createdModule.State);
        }

        [TestMethod]
        public void ExecuteWithTransition()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition);

            // Perform the LoadModule operation: Wait -> EnterTransition -> RunTransition -> ExitTransition -> Operational
            orchestrator.LoadModule(moduleSetup, null);
            Assert.AreEqual(transition, orchestrator.CurrentTransition);
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition.LoadingProgress);

            transition.CallMarkStartCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.AreEqual(1, transition.LoadingProgress);

            // Perform the ReloadModule operation: Operational -> EnterTransition -> RunTransition -> ExitTransition -> Operational
            orchestrator.ReloadModule();
            Assert.AreEqual(transition, orchestrator.CurrentTransition);
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition.LoadingProgress);

            transition.CallMarkStartCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.AreEqual(1, transition.LoadingProgress);

            // Perform the UnloadModule operation: Operational -> EnterTransition -> RunTransition -> ExitTransition -> Wait
            orchestrator.UnloadModule();
            Assert.AreEqual(transition, orchestrator.CurrentTransition);
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition.LoadingProgress);

            transition.CallMarkStartCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.AreEqual(1, transition.LoadingProgress);
        }

        [TestMethod]
        public void ChangeAssociatedModule()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup1 = GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition1);
            IGameModuleSetup moduleSetup2 = GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition2);
            GameModule module1;

            // Load module 1 with transition 1
            orchestrator.LoadModule(moduleSetup1, null);
            module1 = orchestrator.CurrentModule;
            Assert.AreEqual(transition1, orchestrator.CurrentTransition);
            transition1.OnStart += () => transition1.CallMarkStartCompleted();
            transition1.OnStop += () => transition1.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.IsOperational));

            // After the LoadModule sequence, the module 1 is fully loaded
            Assert.AreEqual(module1, orchestrator.CurrentModule);
            Assert.AreEqual(transition1, orchestrator.CurrentTransition);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);

            // Switch to module 2 with transition 2 : Operational -> EnterTransition -> RunTransition -> ExitTransition -> Operational
            orchestrator.SwitchToModule(moduleSetup2, null);
            Assert.AreEqual(module1, orchestrator.CurrentModule);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);

            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition2.LoadingProgress);

            transition2.CallMarkStartCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition2.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.AreEqual(1, transition2.LoadingProgress);

            // After the SwitchToModule sequence, the module 2 is fully loaded
            Assert.AreNotEqual(module1, orchestrator.CurrentModule);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);
        }

        [TestMethod]
        public void InterruptRunningOperation()
        {
            // 1) Example with operations requiring the same transition: interrupt a loading operation with an unloading operation
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition);
            transition.OnStart += () => transition.CallMarkStartCompleted();
            transition.OnStop += () => transition.CallMarkStopCompleted();

            // A) If orchestrator is in RunTransition state -> transition keeps running with a different module operation
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.RunTransition);
            Assert.AreEqual(GameModuleState.Setup, orchestrator.CurrentModule.State);

            orchestrator.UnloadModule();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.IsNull(orchestrator.CurrentModule);

            // B) If orchestrator is in ExitTransition state -> transition return to Start immediatly with a different module operation
            transition.OnStop = null;
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.ExitTransition);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);

            orchestrator.UnloadModule();
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            transition.OnStop += () => transition.CallMarkStopCompleted();
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.Wait));
            Assert.IsNull(orchestrator.CurrentModule);

            // 2) Example with operations requiring different transitions: interrupt a loading operation with an switchToModule operation
            IGameModuleSetup moduleSetup2 = GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition2);
            transition2.OnStart += () => transition2.CallMarkStartCompleted();
            transition2.OnStop += () => transition2.CallMarkStopCompleted();

            // C) If orchestrator is in EnterTransition state -> transition goes to Exit immediatly and then the other transition starts
            transition.OnStart = null;
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.EnterTransition);
            Assert.AreEqual(GameModuleState.Start, orchestrator.CurrentModule.State);

            orchestrator.SwitchToModule(moduleSetup2, null);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.Operational));
        }

        [TestMethod]
        public void ManageSubmodules()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNullTransition();
            IGameModuleSetup submoduleSetup = GetModuleSetupWithNullTransition();
            string submoduleCategory = "TestSubmodule";
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.IsOperational);

            // Add a submodule -> submodule is correctly loaded
            orchestrator.AddSubmodule(submoduleCategory, submoduleSetup, null);
            Assert.AreEqual(1, orchestrator.Children.Count);
            Orchestrator childOrchestrator = orchestrator.Children[0];
            Assert.IsNotNull(childOrchestrator.Parent);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => childOrchestrator.IsOperational));
            Assert.IsNotNull(childOrchestrator.CurrentModule);

            // Retrieve the submodule -> return the created submodule (if category is correct)
            Assert.ThrowsException<InvalidOperationException>(() => orchestrator.GetSubmodule("InvalidCategory"));
            Assert.AreEqual(childOrchestrator.CurrentModule, orchestrator.GetSubmodule(submoduleCategory));

            // Remove the submodule -> submodule is correctly unloaded (if category is correct)
            Assert.ThrowsException<InvalidOperationException>(() => orchestrator.RemoveSubmodule("InvalidCategory"));
            orchestrator.RemoveSubmodule(submoduleCategory);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => childOrchestrator.State == OrchestratorState.Wait));
            Assert.IsNull(childOrchestrator.CurrentModule);
            Assert.AreEqual(0, orchestrator.Children.Count);

            // Perform the UnloadModule operation when orchestrator has children: Operational -> Reset -> RunTransition -> Wait
            orchestrator.AddSubmodule(submoduleCategory, submoduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => childOrchestrator.IsOperational);

            orchestrator.UnloadModule();
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.Reset, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time, 20));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
        }

        private Orchestrator CreateValidOrchestrator(out string category, out GameProcess process)
        {
            category = "Test";
            process = new GameProcess(new StubGameProcessSetup(), m_Time);
            return new Orchestrator(category, process, null);
        }

        private IGameModuleSetup GetModuleSetupWithNullTransition()
        {
            StubGameServiceSetup setup = new StubGameServiceSetup();
            setup.CustomRules = new List<GameRule>();
            setup.CustomInitUnloadOrder = new List<Type>();
            setup.CustomUpdateScheduler = new List<RuleScheduling>();
            return setup;
        }

        private IGameModuleSetup GetModuleSetupWithNonNullTransition(out SpyTransitionActivity transition)
        {
            transition = new SpyTransitionActivity();

            StubGameServiceSetup setup = new StubGameServiceSetup();
            setup.CustomRules = new List<GameRule>();
            setup.CustomInitUnloadOrder = new List<Type>();
            setup.CustomUpdateScheduler = new List<RuleScheduling>();
            setup.CustomTransitionActivity = transition;
            return setup;
        }
    }
}
