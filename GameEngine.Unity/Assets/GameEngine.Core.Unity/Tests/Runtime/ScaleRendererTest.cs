using GameEngine.Core.Unity.Rendering;
using GameEngine.Core.UnityTests.Tools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameEngine.Core.UnityTests
{
    /// <summary>
    /// Component tests for the ScaleRenderer class
    /// <see cref="ScaleRenderer"/>
    /// </summary>
    public class ScaleRendererTest
    {
        SimulationBehaviour m_Simulation;
        GameObject m_ImagePrefab;
        GameObject m_TestObject;
        Image m_TestImage;

        public ScaleRendererTest()
        {
            m_Simulation = new GameObject("Test Simulation").AddComponent<SimulationBehaviour>();
            m_ImagePrefab = (GameObject)Resources.Load("test_image");
        }

        [SetUp]
        public void Initialize()
        {
            m_TestObject = GameObject.Instantiate(m_ImagePrefab);
            m_TestImage = m_TestObject.GetComponentInChildren<Image>();
        }

        [TearDown]
        public void CleanUp()
        {
            m_Simulation.ResetBehaviours();
            GameObject.Destroy(m_TestObject);
        }

        [UnityTest]
        public IEnumerator ExecuteScaleIn()
        {
            float duration = 0.05f;
            bool callbackCalled = false;

            // Create a scale renderer associated with the image, which makes it initially inactive
            ScaleRenderer scaleRenderer = new ScaleRenderer(m_TestImage, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the scale-up phase
            scaleRenderer.StartScaleUp(duration, () => callbackCalled = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, scaleRenderer.Update);

            // At the start, the image is fullt invisible
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);
            Assert.AreEqual(Vector3.zero, m_TestImage.rectTransform.localScale);

            // During the scale up, the image size is between 0 and 1
            yield return new WaitForSeconds(duration / 2);
            Assert.Greater(m_TestImage.rectTransform.localScale.x, 0);
            Assert.Greater(m_TestImage.rectTransform.localScale.y, 0);
            Assert.Greater(m_TestImage.rectTransform.localScale.z, 0);
            Assert.Less(m_TestImage.rectTransform.localScale.x, 1);
            Assert.Less(m_TestImage.rectTransform.localScale.y, 1);
            Assert.Less(m_TestImage.rectTransform.localScale.z, 1);

            // At the end, the image is displayed at full size
            yield return new WaitForSeconds(duration / 2);
            Assert.AreEqual(Vector3.one, m_TestImage.rectTransform.localScale);

            // The callback method has been called and the renderer return the correct state
            Assert.IsTrue(callbackCalled);
            Assert.IsTrue(scaleRenderer.IsFullyUp);
        }

        [UnityTest]
        public IEnumerator ExecuteScaleOut()
        {
            float duration = 0.05f;
            bool callbackCalled = false;

            // Create a scale renderer associated with the image, which keeps it initially active
            ScaleRenderer scaleRenderer = new ScaleRenderer(m_TestImage, true);
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);

            // Launch the scale-down phase
            scaleRenderer.StartScaleDown(duration, () => callbackCalled = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, scaleRenderer.Update);

            // At the start, the image is displayed at full size
            Assert.AreEqual(Vector3.one, m_TestImage.rectTransform.localScale);

            // During the scale down, the image size is between 0 and 1
            yield return new WaitForSeconds(duration / 2);
            Assert.Greater(m_TestImage.rectTransform.localScale.x, 0);
            Assert.Greater(m_TestImage.rectTransform.localScale.y, 0);
            Assert.Greater(m_TestImage.rectTransform.localScale.z, 0);
            Assert.Less(m_TestImage.rectTransform.localScale.x, 1);
            Assert.Less(m_TestImage.rectTransform.localScale.y, 1);
            Assert.Less(m_TestImage.rectTransform.localScale.z, 1);

            // At the end, the image is fully invisible
            yield return new WaitForSeconds(duration / 2);
            Assert.AreEqual(Vector3.zero, m_TestImage.rectTransform.localScale);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // The callback method has been called and the renderer return the correct state
            Assert.IsTrue(callbackCalled);
            Assert.IsTrue(scaleRenderer.IsFullyDown);
        }

        [UnityTest]
        public IEnumerator ExecuteScaleInAndOut()
        {
            float duration = 0.05f;
            bool callback1Called = false;
            bool callback2Called = false;

            // Create a scale renderer associated with the image, which makes it initially inactive
            ScaleRenderer scaleRenderer = new ScaleRenderer(m_TestImage, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the scale-between phase
            scaleRenderer.StartScaleBetween(duration, () => callback1Called = true, () => callback2Called = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, scaleRenderer.Update);

            // At the start, the image is fully invisible
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);
            Assert.AreEqual(Vector3.zero, m_TestImage.rectTransform.localScale);

            // After the scale duration, the image is fully displayed
            yield return new WaitForSeconds(duration);
            Assert.AreEqual(Vector3.one, m_TestImage.rectTransform.localScale);

            // After another scale duration, the image is fully invisible again
            yield return new WaitForSeconds(duration);
            Assert.AreEqual(Vector3.zero, m_TestImage.rectTransform.localScale);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // The callback methods have been called and the renderer return the correct state
            Assert.IsTrue(callback1Called);
            Assert.IsTrue(callback2Called);
            Assert.IsTrue(scaleRenderer.IsFullyDown);
        }

        [UnityTest]
        public IEnumerator ControlScaleParameters()
        {
            // With a zero scale duration, the image appears in its final scale state in the next frame
            ScaleRenderer scaleRenderer = new ScaleRenderer(m_TestImage, false);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, scaleRenderer.Update);
            Assert.IsTrue(scaleRenderer.IsFullyDown);
            scaleRenderer.StartScaleUp(0);
            yield return null;
            Assert.IsTrue(scaleRenderer.IsFullyUp);
            m_Simulation.ResetBehaviours();

            // With a custom reduced scale, the image scaling can be non uniformed accross dimensions
            ScaleRenderer scaleBlack = new ScaleRenderer(m_TestImage, new Vector3(1, 0, 0), false);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, scaleBlack.Update);
            Assert.AreEqual(new Vector3(1, 0, 0), m_TestImage.rectTransform.localScale);
            scaleBlack.SetFullActivation(true);
            Assert.AreEqual(1, m_TestImage.rectTransform.localScale.x);
            Assert.AreEqual(1, m_TestImage.rectTransform.localScale.y);
            scaleBlack.SetFullActivation(false);
            Assert.AreEqual(1, m_TestImage.rectTransform.localScale.x);
            Assert.AreEqual(0, m_TestImage.rectTransform.localScale.y);
        }
    }
}
