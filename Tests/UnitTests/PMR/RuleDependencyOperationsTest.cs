using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using GameEngine.PMR.Rules.Dependencies.Model;
using GameEnginesTest.Tools.Mocks.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.UnitTests.PMR
{
    /// <summary>
    /// Unit tests for the class RuleDependencyOperations
    /// <see cref="RuleDependencyOperations"/>
    /// </summary>
    [TestClass]
    public class RuleDependencyOperationsTest
    {
        [TestMethod]
        public void ExtractDependenciesTest()
        {
            // Create a StubGameService which is a candidate for being provided as dependency via the IStubGameService interface
            StubGameService dependencyProvider = new StubGameService();
            RulesDictionary services = new RulesDictionary();
            services.AddRule(dependencyProvider);

            // Extract dependencies from a set of services including the StubGameService -> the resulted DependencyProvider contains it
            DependencyProvider resultProvider = RuleDependencyOperations.ExtractDependencies(services);
            Assert.IsTrue(resultProvider.TryGet(typeof(IStubGameService), out object retrievedDependency));
            Assert.AreEqual(dependencyProvider, retrievedDependency);
        }

        [TestMethod]
        public void InjectDependenciesTest()
        {
            // Create a StubGameRuleTer which is a candidate for dependency injection on the fields StubServiceReference and StubRuleBisReference
            StubGameRuleTer dependencyConsumer = new StubGameRuleTer();
            Assert.IsNull(dependencyConsumer.StubServiceReference);
            Assert.IsNull(dependencyConsumer.StubRuleBisReference);

            // Create the dependency providers from which to provide those dependencies
            StubGameService dummyService = new StubGameService();
            StubGameRuleBis dummyRuleBis = new StubGameRuleBis();
            DependencyProvider servicesProvider = new DependencyProvider();
            servicesProvider.Add(typeof(IStubGameService), dummyService);
            DependencyProvider rulesProvider = new DependencyProvider();
            rulesProvider.Add(typeof(IStubGameRuleBis), dummyRuleBis);

            // Inject dependencies to the StubGameRuleTer rule -> the dependent fields now reference the dependency providers
            RuleDependencyOperations.InjectRuleDependencies(dependencyConsumer, servicesProvider, rulesProvider);
            Assert.AreEqual(dummyService, dependencyConsumer.StubServiceReference);
            Assert.AreEqual(dummyRuleBis, dependencyConsumer.StubRuleBisReference);

            // Try to inject dependencies using only rules provider -> no exception thrown, but StubServiceReference stays null (optional dependency)
            StubGameRuleTer dependencyConsumer2 = new StubGameRuleTer();
            RuleDependencyOperations.InjectRuleDependencies(dependencyConsumer2, null, rulesProvider);
            Assert.IsNull(dependencyConsumer2.StubServiceReference);
            Assert.AreEqual(dummyRuleBis, dependencyConsumer2.StubRuleBisReference);

            // Try to inject dependencies using only services provider -> throw DependencyException because the dependency on StubGameRuleTer is required
            StubGameRuleTer dependencyConsumer3 = new StubGameRuleTer();
            Assert.ThrowsException<DependencyException>(() => RuleDependencyOperations.InjectRuleDependencies(dependencyConsumer3, servicesProvider, null));
        }
    }
}
