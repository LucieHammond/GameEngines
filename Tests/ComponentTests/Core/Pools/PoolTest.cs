using GameEngine.Core.Pools;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class Pool
    /// <see cref="Pool{T}"/>
    /// </summary>
    [TestClass]
    public class PoolTest
    {
        private readonly SpyPooler m_Pooler;
        private readonly string m_PoolId;

        public PoolTest()
        {
            m_Pooler = new SpyPooler("Test");
            m_PoolId = "test_pool";
        }

        [TestMethod]
        public void InitializeAndClearPool()
        {
            int initialSize = 20;

            // Create pool -> pool has correct id and correct size 
            Pool<TestObject> pool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize);
            Assert.AreEqual(m_PoolId, pool.PoolId);
            Assert.AreEqual(initialSize, pool.PoolSize);

            // Clear pool -> pool is empty
            pool.ClearPool();
            Assert.AreEqual(0, pool.PoolSize);

            // Reset pool -> pool is filled to the initial size again
            pool.ResetPool();
            Assert.AreEqual(initialSize, pool.PoolSize);

            // Reset pool without previous clear -> pool is emptied and refilled to the initial size
            pool.ResetPool();
            Assert.AreEqual(initialSize, pool.PoolSize);
        }

        [TestMethod]
        public void TakeAndReleaseObjects()
        {
            int initialSize = 5;
            Pool<TestObject> pool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize);

            // Get an object from pool -> object is not null and pool size haven't changed
            TestObject pooledObject = pool.GetFreeObject();
            Assert.IsNotNull(pooledObject);
            Assert.AreEqual(initialSize, pool.PoolSize);

            // Get other objects from pool -> all objects are different
            Assert.AreNotEqual(pooledObject, pool.GetFreeObject());
            Assert.AreNotEqual(pooledObject, pool.GetFreeObject());

            // Release first object to pool -> object can be taken again
            pool.ReleaseUsedObject(pooledObject);
            bool found = false;
            for (int i = 0; i < initialSize; i++)
            {
                if (pooledObject == pool.GetFreeObject())
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void ManageObjectStatesWithPooler()
        {
            int initialSize = 3;

            // Initialize pool -> all objects are created and initialized by pooler
            Pool<TestObject> pool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize);
            Assert.AreEqual(initialSize, m_Pooler.CreatedObjects.Count);
            for (int i = 0; i < initialSize; i++)
            {
                Assert.IsTrue(m_Pooler.CreatedObjects[i].Initialized);
                Assert.IsFalse(m_Pooler.CreatedObjects[i].Activated);
                Assert.IsFalse(m_Pooler.CreatedObjects[i].Cleared);
            }

            // Get object from pool -> object is prepared by pooler
            TestObject pooledObject = pool.GetFreeObject();
            Assert.IsTrue(pooledObject.Activated);

            // Release object to pool -> object is restored by pooler
            pool.ReleaseUsedObject(pooledObject);
            Assert.IsFalse(pooledObject.Activated);

            // Clear pool -> all pooled objects are destroyed by pooler
            pool.ClearPool();
            Assert.AreEqual(initialSize, m_Pooler.CreatedObjects.Count);
            for (int i = 0; i < initialSize; i++)
            {
                Assert.IsTrue(m_Pooler.CreatedObjects[i].Cleared);
            }
        }

        [TestMethod]
        public void ExtendPoolOnShortage()
        {
            int initialSize = 5;
            Pool<TestObject> inextensiblePool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize, false);
            Pool<TestObject> extensiblePool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize, true);

            // Reserve all objects from both pools -> pool size don't change
            for (int i = 0; i < initialSize; i++)
                inextensiblePool.GetFreeObject();
            Assert.AreEqual(initialSize, inextensiblePool.PoolSize);

            for (int i = 0; i < initialSize; i++)
                extensiblePool.GetFreeObject();
            Assert.AreEqual(initialSize, extensiblePool.PoolSize);

            // Try to take a new object from inextensible pool -> log error and return null object
            AssertUtils.LogError(() =>
            {
                TestObject pooledObject = inextensiblePool.GetFreeObject();
                Assert.IsNull(pooledObject);
            });
            Assert.AreEqual(initialSize, inextensiblePool.PoolSize);

            // Try to take a new object from extensible pool -> log warning and return a newly pooled object
            AssertUtils.LogWarning(() =>
            {
                TestObject pooledObject = extensiblePool.GetFreeObject();
                Assert.IsNotNull(pooledObject);
            });
            Assert.AreEqual(initialSize + 1, extensiblePool.PoolSize);
        }

        [TestMethod]
        public void RespondToInvalidSituations()
        {
            int initialSize = 5;
            Pool<TestObject> pool = new Pool<TestObject>(m_Pooler, m_PoolId, initialSize);
            TestObject pooledObject = pool.GetFreeObject();

            // Release an invalid object -> log error
            TestObject invalidObject = new TestObject("Test");
            AssertUtils.LogError(() => pool.ReleaseUsedObject(invalidObject));

            // Try to release an object after clear -> log error
            pool.ClearPool();
            AssertUtils.LogError(() => pool.ReleaseUsedObject(pooledObject));

            // Try to release an object after reset -> log error
            pool.ResetPool();
            AssertUtils.LogError(() => pool.ReleaseUsedObject(pooledObject));
        }
    }
}
