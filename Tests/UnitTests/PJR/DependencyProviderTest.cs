using GameEngine.PJR.Rules.Dependencies;
using GameEnginesTest.Tools.Dummy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.UnitTests.PJR
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
            IDummyGameService dependency = new DummyGameService();
            provider.Add(typeof(IDummyGameService), dependency);
            provider.TryGet(typeof(IDummyGameService), out object retrievedDependency);
            Assert.AreEqual(dependency, retrievedDependency);

            // Try to add a dependency that is not an interface -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.Add(typeof(DummyGameService), dependency));

            // Try to add an interface dependency but passing an object that doesn't implement it -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.Add(typeof(IDummyGameService), new DummyGameRule()));

            // Try to add an interface dependency that is already in DependencyProvider -> throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => provider.Add(typeof(IDummyGameService), dependency));
        }

        [TestMethod]
        public void TryGetTest()
        {
            // Create DependencyProvider with one dependency on IDummyGameService
            DependencyProvider provider = new DependencyProvider();
            IDummyGameService dependency = new DummyGameService();
            provider.Add(typeof(IDummyGameService), dependency);

            // Try get this correct dependency -> return true and retrieve the dependency
            Assert.IsTrue(provider.TryGet(typeof(IDummyGameService), out object retrievedDependency));
            Assert.IsInstanceOfType(retrievedDependency, typeof(IDummyGameService));
            Assert.AreEqual(dependency, retrievedDependency);

            // Try get a dependency that is not in provider -> return false
            Assert.IsFalse(provider.TryGet(typeof(IDummyGameRuleBis), out object _));

            // Try get a dependency that is not an interface -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => provider.TryGet(typeof(DummyGameService), out object _));
        }
    }
}
