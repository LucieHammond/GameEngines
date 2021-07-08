using GameEngine.Core.Descriptors;
using GameEngine.Core.Pools;
using GameEngine.Core.Pools.Managers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.Core.Test
{
    /// <summary>
    /// Component tests for the GameObjectPoolManager class
    /// <see cref="GameObjectPoolManager"/>
    /// </summary>
    public class GameObjectPoolTest
    {
        private readonly GameObjectPoolManager m_PoolManager;
        private PoolDescriptor<GameObjectDescriptor> m_PoolDescriptor;

        private GameObject m_TestPrefab;
        private Transform m_PoolRoot;

        public GameObjectPoolTest()
        {
            m_PoolManager = new GameObjectPoolManager();
        }

        [SetUp]
        public void Initialize()
        {
            m_TestPrefab = (GameObject)Resources.Load("test_cube");
            m_PoolRoot = new GameObject("Test Pool").transform;

            m_PoolDescriptor = new PoolDescriptor<GameObjectDescriptor>()
            {
                PoolId = "go_test_pool",
                InitialSize = 10,
                IsExtensible = true,
                ObjectDescriptor = new GameObjectDescriptor()
                {
                    ReferencePrefab = m_TestPrefab,
                    Parent = m_PoolRoot
                }
            };
        }

        [TearDown]
        public void CleanUp()
        {
            m_PoolManager.Clear();
            Object.Destroy(m_PoolRoot.gameObject);
        }

        [UnityTest]
        public IEnumerator InstantiateObjectsWhenCreated()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);
            yield return null;

            // The pooled gameobjects are instantiated as children of the pool root
            Assert.AreEqual(m_PoolDescriptor.InitialSize, m_PoolRoot.childCount);

            for (int i = 0; i < m_PoolRoot.childCount; i++)
            {
                GameObject gameObject = m_PoolRoot.GetChild(i).gameObject;

                // All pooled objects are named according to the reference prefab with a rank suffix
                Assert.AreEqual($"{m_TestPrefab.name}_{i + 1}", gameObject.name);

                // All pooled objects are similar to the reference prefab (same components, parameters...)
                Assert.AreSame(m_TestPrefab.GetComponent<MeshFilter>().sharedMesh, gameObject.GetComponent<MeshFilter>().sharedMesh);

                // All pooled objects are deactivated
                Assert.IsFalse(gameObject.activeInHierarchy);
            }
        }

        [UnityTest]
        public IEnumerator ActivateObjectWhenRequested()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);
            GameObject gameObject = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The returned game object is one of the children of the pool root
            Assert.IsTrue(gameObject.transform.IsChildOf(m_PoolRoot));

            // The game object have been activated
            Assert.IsTrue(gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator DeactivateObjectWhenReleased()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);
            GameObject gameObject = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            m_PoolManager.ReleaseObjectToPool(m_PoolDescriptor.PoolId, gameObject);
            yield return null;

            // The released game object is still a children of the pool root
            Assert.IsTrue(gameObject.transform.IsChildOf(m_PoolRoot));

            // The game object have been deactivated
            Assert.IsFalse(gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator DestroyObjectsWhenCleared()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);
            GameObject gameObject = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            m_PoolManager.DestroyPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The pool root has no more children game objects since they are all destroyed
            Assert.AreEqual(0, m_PoolRoot.childCount);

            // The remaining reference to one of them is null in Unity
            Assert.IsTrue(gameObject == null);
        }

        [Test]
        public void ManipulatePooledComponents()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);

            // Get a MeshRenderer component from the pool -> component is not null and belongs to an activated pooled object
            MeshRenderer objectRenderer = m_PoolManager.GetComponentFromPool<MeshRenderer>(m_PoolDescriptor.PoolId);
            Assert.IsTrue(objectRenderer != null);
            Assert.IsTrue(objectRenderer.gameObject.activeInHierarchy);
            Assert.IsTrue(objectRenderer.transform.IsChildOf(m_PoolRoot));

            // If the component doesn't exist on the pooled object -> throw MissingComponentException
            Assert.Throws<MissingComponentException>(() => m_PoolManager.GetComponentFromPool<BoxCollider>(m_PoolDescriptor.PoolId));

            // Release the MeshRenderer component to the pool -> the object it belongs to is deactivated
            m_PoolManager.ReleaseComponentToPool(m_PoolDescriptor.PoolId, objectRenderer);
            Assert.IsFalse(objectRenderer.gameObject.activeInHierarchy);
        }
    }
}
