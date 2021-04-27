using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Dependencies;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.UnitTests.PJR
{
    /// <summary>
    /// Unit tests for the class DependencyProvider
    /// <see cref="DependencyUtils"/>
    /// </summary>
    [TestClass]
    public class DependencyUtilsTest
    {
        [TestMethod]
        public void ExtractDependenciesTest()
        {
            // Create a DummyGameService which is a candidate for being provided as dependency via the IDummyService interface
            DummyGameService dependencyProvider = new DummyGameService();
            RulesDictionary services = new RulesDictionary();
            services.AddRule(dependencyProvider);

            // Extract dependencies from a set of services including the DummyGameService -> the resulted DependencyProvider contains it
            DependencyProvider resultProvider = DependencyUtils.ExtractDependencies(services);
            Assert.IsTrue(resultProvider.TryGet(typeof(IDummyGameService), out object retrievedDependency));
            Assert.AreEqual(dependencyProvider, retrievedDependency);
        }

        [TestMethod]
        public void InjectDependenciesTest()
        {
            // Create a DummyGameRuleTer which is a candidate for dependency injection on the fields DummyServiceReference and DummyRuleBisReference
            DummyGameRuleTer dependencyConsumer = new DummyGameRuleTer();
            Assert.IsNull(dependencyConsumer.DummyServiceReference);
            Assert.IsNull(dependencyConsumer.DummyRuleBisReference);
            RulesDictionary rules = new RulesDictionary();
            rules.AddRule(dependencyConsumer);

            // Create the dependency providers from which to provide those dependencies
            DummyGameService dummyService = new DummyGameService();
            DummyGameRuleBis dummyRuleBis = new DummyGameRuleBis();
            ProcessService processService = new ProcessService(new GameProcess(new DummyGameProcessSetup(), new MockProcessTime())); // this one is mandatory
            DependencyProvider servicesProvider = new DependencyProvider();
            servicesProvider.Add(typeof(IProcessAccessor), processService);
            servicesProvider.Add(typeof(IDummyGameService), dummyService);
            DependencyProvider rulesProvider = new DependencyProvider();
            rulesProvider.Add(typeof(IDummyGameRuleBis), dummyRuleBis);

            // Inject dependencies to the set of rules containing the DummyGameRuleTer -> the dependent fields now reference the dependency providers
            DependencyUtils.InjectDependencies(rules, servicesProvider, rulesProvider);
            Assert.AreEqual(dummyService, dependencyConsumer.DummyServiceReference);
            Assert.AreEqual(dummyRuleBis, dependencyConsumer.DummyRuleBisReference);

            // Try to inject dependencies using only service provider -> no exception thrown, but DummyGameRuleBis stays null (optional dependency)
            DummyGameRuleTer dependencyConsumer2 = new DummyGameRuleTer();
            RulesDictionary rules2 = new RulesDictionary();
            rules2.AddRule(dependencyConsumer2);
            DependencyUtils.InjectDependencies(rules2, servicesProvider, null);
            Assert.AreEqual(dummyService, dependencyConsumer2.DummyServiceReference);
            Assert.IsNull(dependencyConsumer2.DummyRuleBisReference);

            // Try to inject dependencies using only rules provider -> throw DependencyException because the dependency on DummyGameService is required
            DummyGameRuleTer dependencyConsumer3 = new DummyGameRuleTer();
            RulesDictionary rules3 = new RulesDictionary();
            rules3.AddRule(dependencyConsumer3);
            Assert.ThrowsException<DependencyException>(() => DependencyUtils.InjectDependencies(rules3, null, rulesProvider));
        }
    }
}
