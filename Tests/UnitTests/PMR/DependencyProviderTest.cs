using GameEngine.PMR.Rules.Dependencies;
using GameEnginesTest.Tools.Mocks.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.UnitTests.PMR
{
    /// <summary>
    /// Unit tests for the class DependencyProvider
    /// <see cref="DependencyProvider"/>
    /// </summary>
    [TestClass]
    public class DependencyProviderTest
    {
        [TestMethod]
        public void AddTest()
        {
            DependencyProvider provider = new DependencyProvider();

            // Add an interface dependency with valid implementation of it -> interface can be retrieved from DependencyProvider
            IStubGameService dependency = new StubGameService();
            provider.Add(typeof(IStubGameService), dependency);
            provider.TryGet(typeof(IStubGameService), out object retrievedDependency);
            Assert.AreEqual(dependency, retrievedDependency);

            // Try to add a dependency that is not an interface -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.Add(typeof(StubGameService), dependency));

            // Try to add an interface dependency but passing an object that doesn't implement it -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.Add(typeof(IStubGameService), new StubGameRule()));

            // Try to add an interface dependency that is already in DependencyProvider -> throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => provider.Add(typeof(IStubGameService), dependency));
        }

        [TestMethod]
        public void TryGetTest()
        {
            // Create DependencyProvider with one dependency on IStubGameService
            DependencyProvider provider = new DependencyProvider();
            IStubGameService dependency = new StubGameService();
            provider.Add(typeof(IStubGameService), dependency);

            // Try get this correct dependency -> return true and retrieve the dependency
            Assert.IsTrue(provider.TryGet(typeof(IStubGameService), out object retrievedDependency));
            Assert.IsInstanceOfType(retrievedDependency, typeof(IStubGameService));
            Assert.AreEqual(dependency, retrievedDependency);

            // Try get a dependency that is not in provider -> return false
            Assert.IsFalse(provider.TryGet(typeof(IStubGameRuleBis), out object _));

            // Try get a dependency that is not an interface -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.TryGet(typeof(StubGameService), out object _));
        }

        [TestMethod]
        public void LinkToParentProviderTest()
        {
            // Link a dependency provider to another parent dependency provider
            DependencyProvider provider = new DependencyProvider();
            DependencyProvider parentProvider = new DependencyProvider();
            provider.LinkToParentProvider(parentProvider);

            // The linked parent provider contains dependencies that the child doesn't have -> dependency can be retrieved
            IStubGameRuleBis parentDependency = new StubGameRuleBis();
            parentProvider.Add(typeof(IStubGameRuleBis), parentDependency);
            Assert.IsTrue(provider.TryGet(typeof(IStubGameRuleBis), out object retrievedDependency));
            Assert.AreEqual(parentDependency, retrievedDependency);

            // The linked parent provider contains a kind of dependency that the child already have -> the child dependency is retrieved
            IStubGameRuleBis childDependency = new StubGameRuleBis();
            provider.Add(typeof(IStubGameRuleBis), childDependency);
            Assert.IsTrue(provider.TryGet(typeof(IStubGameRuleBis), out retrievedDependency));
            Assert.AreEqual(childDependency, retrievedDependency);

            // Link multiple parent providers recursively -> can retrieve dependency from the most distant of them
            DependencyProvider parentProvider2 = new DependencyProvider();
            parentProvider.LinkToParentProvider(parentProvider2);
            DependencyProvider parentProvider3 = new DependencyProvider();
            parentProvider2.LinkToParentProvider(parentProvider3);
            parentProvider3.Add(typeof(IStubGameService), new StubGameService());
            Assert.IsTrue(provider.TryGet(typeof(IStubGameService), out _));
        }
    }
}
