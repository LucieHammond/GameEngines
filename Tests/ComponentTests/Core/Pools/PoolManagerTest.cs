using GameEngine.Core.Pools;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class PoolManager
    /// <see cref="PoolManager{T, TPooler, TDescriptor}"/>
    /// </summary>
    [TestClass]
    public class PoolManagerTest
    {
        private readonly PoolDescriptor<string> m_PoolDescriptorA;
        private readonly PoolDescriptor<string> m_PoolDescriptorB;
        private readonly PoolDescriptor<string> m_PoolDescriptorC;

        public PoolManagerTest()
        {
            m_PoolDescriptorA = new PoolDescriptor<string>()
            {
                PoolId = "test_pool_A",
                InitialSize = 5,
                IsExtensible = true,
                ObjectDescriptor = "A"
            };

            m_PoolDescriptorB = new PoolDescriptor<string>()
            {
                PoolId = "test_pool_B",
                InitialSize = 8,
                IsExtensible = false,
                ObjectDescriptor = "B"
            };

            m_PoolDescriptorC = new PoolDescriptor<string>()
            {
                PoolId = "test_pool_C",
                InitialSize = 3,
                IsExtensible = true,
                ObjectDescriptor = "C"
            };
        }

        [TestMethod]
        public void CreateAndDestroyPools()
        {
            // Create a default pool manager -> it doesn't contain any pool yet
            TestPoolManager poolManager = new TestPoolManager();
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorA.PoolId));
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorB.PoolId));
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorC.PoolId));

            // Initialize the pool manager with a list of pools -> the requested pools are created
            poolManager.Initialize(new List<PoolDescriptor<string>>() { m_PoolDescriptorA, m_PoolDescriptorB });
            Assert.IsTrue(poolManager.ContainPool(m_PoolDescriptorA.PoolId));
            Assert.IsTrue(poolManager.ContainPool(m_PoolDescriptorB.PoolId));
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorC.PoolId));

            // Ask to create a new pool -> this pool is added
            poolManager.CreatePool(m_PoolDescriptorC);
            Assert.IsTrue(poolManager.ContainPool(m_PoolDescriptorC.PoolId));

            // Ask to destroy a pool -> this pool is removed
            poolManager.DestroyPool(m_PoolDescriptorB.PoolId);
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorB.PoolId));

            // Clear the manager -> all pools are destroyed
            poolManager.Clear();
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorA.PoolId));
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorB.PoolId));
            Assert.IsFalse(poolManager.ContainPool(m_PoolDescriptorC.PoolId));
        }

        [TestMethod]
        public void GetAndReleaseObjects()
        {
            TestPoolManager poolManager = new TestPoolManager();
            poolManager.Initialize(new List<PoolDescriptor<string>>() { m_PoolDescriptorA, m_PoolDescriptorB });

            // Get object from pool A -> object is configured with type A
            TestObject objectA = poolManager.GetObjectFromPool(m_PoolDescriptorA.PoolId);
            Assert.AreEqual(m_PoolDescriptorA.ObjectDescriptor, objectA.Type);

            // Get object from pool B -> object is configured with type B
            TestObject objectB = poolManager.GetObjectFromPool(m_PoolDescriptorB.PoolId);
            Assert.AreEqual(m_PoolDescriptorB.ObjectDescriptor, objectB.Type);

            // Get another object from pool A -> object is different from the first A object
            TestObject objectAbis = poolManager.GetObjectFromPool(m_PoolDescriptorA.PoolId);
            Assert.AreEqual(m_PoolDescriptorA.ObjectDescriptor, objectAbis.Type);
            Assert.AreNotEqual(objectA, objectAbis);

            // Release an object from pool A into pool A -> object can be taken again
            poolManager.ReleaseObjectToPool(m_PoolDescriptorA.PoolId, objectA);
            bool found = false;
            for (int i = 0; i < m_PoolDescriptorA.InitialSize; i++)
            {
                if (objectA == poolManager.GetObjectFromPool(m_PoolDescriptorA.PoolId))
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found);

            // Release an object from pool A into pool B -> log Pool error
            AssertUtils.LogError(() => poolManager.ReleaseObjectToPool(m_PoolDescriptorB.PoolId, objectAbis), "Pool");

            // Release an object from pool B into pool B after reset -> log Pool error
            poolManager.ResetPools();
            AssertUtils.LogError(() => poolManager.ReleaseObjectToPool(m_PoolDescriptorB.PoolId, objectB), "Pool");
        }

        [TestMethod]
        public void ManageInvalidPoolIds()
        {
            TestPoolManager poolManager = new TestPoolManager();
            poolManager.Initialize(new List<PoolDescriptor<string>>() { m_PoolDescriptorA, m_PoolDescriptorB });

            // Try to create a pool that already exist -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => poolManager.CreatePool(m_PoolDescriptorA));

            // Try to get an object from a pool that doesn't exist -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => poolManager.GetObjectFromPool(m_PoolDescriptorC.PoolId));

            // Try to release an object to a pool that doesn't exist -> throw ArgumentException
            TestObject pooledObject = new TestObject(m_PoolDescriptorC.ObjectDescriptor);
            Assert.ThrowsException<ArgumentException>(() => poolManager.ReleaseObjectToPool(m_PoolDescriptorC.PoolId, pooledObject));

            // Try to destroy a pool that doesn't exist -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => poolManager.DestroyPool(m_PoolDescriptorC.PoolId));
        }

        private class TestPoolManager : PoolManager<TestObject, DummyPooler, string>
        {
            protected override DummyPooler CreateObjectPooler(string objectType)
            {
                return new DummyPooler(objectType);
            }
        }
    }
}
