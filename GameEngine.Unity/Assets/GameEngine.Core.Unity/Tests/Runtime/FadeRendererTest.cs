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
    /// Component tests for the FadeRenderer class
    /// <see cref="FadeRenderer"/>
    /// </summary>
    public class FadeRendererTest
    {
        SimulationBehaviour m_Simulation;
        GameObject m_ImagePrefab;
        GameObject m_TestObject;
        Image m_TestImage;

        public FadeRendererTest()
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
        public IEnumerator ExecuteFadeIn()
        {
            float duration = 0.05f;
            bool callbackCalled = false;

            // Create a fade renderer associated with the image, which makes it initially inactive
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-in phase
            fadeRenderer.StartFadeIn(duration, () => callbackCalled = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeRenderer.Update);

            // At the start, the image is fully invisible
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);
            Assert.AreEqual(0, m_TestImage.color.a);

            // During the fade in, the image opacity is between 0 and 1
            yield return new WaitForSeconds(duration / 2);
            Assert.Greater(m_TestImage.color.a, 0);
            Assert.Less(m_TestImage.color.a, 1);

            // At the end, the image is fully displayed
            yield return new WaitForSeconds(duration / 2);
            Assert.AreEqual(1, m_TestImage.color.a);

            // The callback method has been called and the renderer return the correct state
            Assert.IsTrue(callbackCalled);
            Assert.IsTrue(fadeRenderer.IsFullyIn);
        }

        [UnityTest]
        public IEnumerator ExecuteFadeOut()
        {
            float duration = 0.05f;
            bool callbackCalled = false;

            // Create a fade renderer associated with the image, which keeps it initially active
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, true);
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-out phase
            fadeRenderer.StartFadeOut(duration, () => callbackCalled = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeRenderer.Update);

            // At the start, the image is fully displayed
            Assert.AreEqual(1, m_TestImage.color.a);

            // During the fade in, the image opacity is between 0 and 1
            yield return new WaitForSeconds(duration / 2);
            Assert.Greater(m_TestImage.color.a, 0);
            Assert.Less(m_TestImage.color.a, 1);

            // At the end, the image is fully invisible
            yield return new WaitForSeconds(duration / 2);
            Assert.AreEqual(0, m_TestImage.color.a);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // The callback method has been called and the renderer return the correct state
            Assert.IsTrue(callbackCalled);
            Assert.IsTrue(fadeRenderer.IsFullyOut);
        }

        [UnityTest]
        public IEnumerator ExecuteFadeInAndOut()
        {
            float duration = 0.05f;
            bool callback1Called = false;
            bool callback2Called = false;

            // Create a fade renderer associated with the image, which makes it initially inactive
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-between phase
            fadeRenderer.StartFadeBetween(duration, () => callback1Called = true, () => callback2Called = true);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeRenderer.Update);

            // At the start, the image is fully invisible
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);
            Assert.AreEqual(0, m_TestImage.color.a);

            // After the fade duration, the image is fully displayed
            yield return new WaitForSeconds(duration);
            Assert.AreEqual(1, m_TestImage.color.a);

            // After another fade duration, the image is fully invisible again
            yield return new WaitForSeconds(duration);
            Assert.AreEqual(0, m_TestImage.color.a);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // The callback methods have been called and the renderer return the correct state
            Assert.IsTrue(callback1Called);
            Assert.IsTrue(callback2Called);
            Assert.IsTrue(fadeRenderer.IsFullyOut);
        }

        [UnityTest]
        public IEnumerator ControlFadeParameters()
        {
            // With a zero fade duration, the image appears in its final fade state in the next frame
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, false);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeRenderer.Update);
            Assert.IsTrue(fadeRenderer.IsFullyOut);
            fadeRenderer.StartFadeIn(0);
            yield return null;
            Assert.IsTrue(fadeRenderer.IsFullyIn);
            m_Simulation.ResetBehaviours();

            // With a black opaque fade color, the image blend to black instead of fading
            FadeRenderer fadeBlack = new FadeRenderer(m_TestImage, Color.black, false);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeBlack.Update);
            Assert.AreEqual(Color.black, m_TestImage.color);
            fadeBlack.SetFullActivation(true);
            Assert.AreEqual(Color.white, m_TestImage.color);
            fadeBlack.SetFullActivation(false);
            Assert.AreEqual(Color.black, m_TestImage.color);
        }
    }
}
