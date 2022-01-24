using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Process.Transitions;
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

            // At creation, orchestrator is in Wait state with null module, null transition and empty operations
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.IsFalse(orchestrator.IsOperational);
            Assert.IsNull(orchestrator.CurrentModule);
            Assert.IsNull(orchestrator.CurrentTransition);
            Assert.AreEqual(0, orchestrator.NextOperations.Count);
            Assert.AreEqual(0, orchestrator.PastTransitions.Count);

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
            IGameModuleSetup moduleSetup = GetModuleSetupWithNullTransition("Module");

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
            GameModule createdModule = orchestrator.CurrentModule;

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
            IGameModuleSetup moduleSetup = GetModuleSetupWithNonNullTransition("Module", out SpyTransition transition);

            // Perform the LoadModule operation: Wait -> EnterTransition -> RunTransition -> ExitTransition -> Operational
            orchestrator.LoadModule(moduleSetup, null);
            transition.CallMarkReady();
            Assert.AreEqual(transition, orchestrator.CurrentTransition);
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);

            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition.LoadingProgress);

            transition.CallMarkEntered();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);

            transition.CallMarkCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkExited();
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

            transition.CallMarkEntered();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);

            transition.CallMarkCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkExited();
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

            transition.CallMarkEntered();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);

            transition.CallMarkCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition.CallMarkExited();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Wait, orchestrator.State);
            Assert.AreEqual(1, transition.LoadingProgress);
        }

        [TestMethod]
        public void ChangeAssociatedModule()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup1 = GetModuleSetupWithNonNullTransition("Module1", out SpyTransition transition1);
            IGameModuleSetup moduleSetup2 = GetModuleSetupWithNonNullTransition("Module2", out SpyTransition transition2);

            // Load module 1 with transition 1
            transition1.SetAutomaticCompletion();
            orchestrator.LoadModule(moduleSetup1, null);
            Assert.IsNull(orchestrator.CurrentModule);
            Assert.AreEqual(transition1, orchestrator.CurrentTransition);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.IsOperational));

            // After the LoadModule sequence, the module 1 is fully loaded
            Assert.AreEqual(moduleSetup1.Name, orchestrator.CurrentModule.Name);
            Assert.AreEqual(transition1, orchestrator.CurrentTransition);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);

            // Switch to module 2 with transition 2 : Operational -> EnterTransition -> RunTransition -> ExitTransition -> Operational
            orchestrator.SwitchToModule(moduleSetup2, null);
            Assert.AreEqual(moduleSetup1.Name, orchestrator.CurrentModule.Name);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);

            transition2.CallMarkReady();
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(0, transition2.LoadingProgress);

            transition2.CallMarkEntered();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);

            transition2.CallMarkCompleted();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.ExitTransition, orchestrator.State);

            transition2.CallMarkExited();
            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.Operational, orchestrator.State);
            Assert.AreEqual(1, transition2.LoadingProgress);

            // After the SwitchToModule sequence, the module 2 is fully loaded
            Assert.AreEqual(moduleSetup2.Name, orchestrator.CurrentModule.Name);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);
            Assert.AreEqual(GameModuleState.UpdateRules, orchestrator.CurrentModule.State);
        }

        [TestMethod]
        public void InterruptRunningOperation()
        {
            // - Example with operations requiring the same transition (load and unload)
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNonNullTransition("Module1", out SpyTransition transition);
            transition.SetAutomaticCompletion();

            // A) If orchestrator is in EnterTransition state -> continue in EnterTransition with same transition
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.EnterTransition);

            orchestrator.UnloadModule();
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(transition, orchestrator.CurrentTransition);

            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.Wait));
            Assert.IsNull(orchestrator.CurrentModule);

            // B) If orchestrator is in ExitTransition state -> return to EnterTransition with same transition
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.ExitTransition);

            orchestrator.UnloadModule();
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.EnterTransition, orchestrator.State);
            Assert.AreEqual(transition, orchestrator.CurrentTransition);

            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.Wait));
            Assert.IsNull(orchestrator.CurrentModule);

            // - Example with operations requiring different transitions (switch transition)
            IGameModuleSetup moduleSetup2 = GetModuleSetupWithNonNullTransition("Module2", out SpyTransition transition2);
            transition2.SetAutomaticCompletion();

            // C) If orchestrator is in RunTransition state -> go to ChangeTransition state with new transition
            orchestrator.LoadModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.RunTransition);

            orchestrator.SwitchToModule(moduleSetup2, null);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.ChangeTransition, orchestrator.State);
            Assert.AreEqual(transition2, orchestrator.CurrentTransition);
            Assert.AreEqual(TransitionState.Running, transition.State);

            Assert.IsTrue(orchestrator.SimulateUpToNextState(m_Time));
            Assert.AreEqual(OrchestratorState.RunTransition, orchestrator.State);
            Assert.AreEqual(TransitionState.Inactive, transition.State);

            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.Operational));
            Assert.AreEqual(moduleSetup2.Name, orchestrator.CurrentModule.Name);

            // D) If orchestrator is in SwitchTransition state -> continue in SwitchTransition state with new transition
            orchestrator.SwitchToModule(moduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.RunTransition);
            orchestrator.SwitchToModule(moduleSetup2, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.State == OrchestratorState.ChangeTransition);

            orchestrator.SwitchToModule(moduleSetup, null);
            orchestrator.SimulateOneFrame(m_Time);
            Assert.AreEqual(OrchestratorState.ChangeTransition, orchestrator.State);
            Assert.AreEqual(transition, orchestrator.CurrentTransition);
        }

        [TestMethod]
        public void ManageSubmodules()
        {
            Orchestrator orchestrator = CreateValidOrchestrator(out string _, out GameProcess _);
            IGameModuleSetup moduleSetup = GetModuleSetupWithNullTransition("Module");
            IGameModuleSetup submoduleSetup = GetModuleSetupWithNullTransition("Submodule");
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

            // Perform the UnloadModule operation when orchestrator has children: children are unloaded first
            orchestrator.AddSubmodule(submoduleCategory, submoduleSetup, null);
            orchestrator.SimulateExecutionUntil(m_Time, () => childOrchestrator.IsOperational);

            orchestrator.UnloadModule();
            Assert.IsTrue(orchestrator.Children.Count > 0);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.Children.Count == 0));
            Assert.IsNotNull(orchestrator.CurrentModule);
            Assert.IsTrue(orchestrator.SimulateExecutionUntil(m_Time, () => orchestrator.CurrentModule == null));
        }

        private Orchestrator CreateValidOrchestrator(out string category, out GameProcess process)
        {
            category = "Test";
            process = new GameProcess(new StubGameProcessSetup(), m_Time);
            return new Orchestrator(category, process, null);
        }

        private IGameModuleSetup GetModuleSetupWithNullTransition(string name)
        {
            StubGameServiceSetup setup = new StubGameServiceSetup();
            setup.CustomName = name;
            setup.CustomRules = new List<GameRule>();
            setup.CustomInitUnloadOrder = new List<Type>();
            setup.CustomUpdateScheduler = new List<RuleScheduling>();
            return setup;
        }

        private IGameModuleSetup GetModuleSetupWithNonNullTransition(string name, out SpyTransition transition)
        {
            transition = new SpyTransition();

            StubGameServiceSetup setup = new StubGameServiceSetup();
            setup.CustomName = name;
            setup.CustomRules = new List<GameRule>();
            setup.CustomInitUnloadOrder = new List<Type>();
            setup.CustomUpdateScheduler = new List<RuleScheduling>();
            setup.CustomTransition = transition;
            return setup;
        }
    }
}
