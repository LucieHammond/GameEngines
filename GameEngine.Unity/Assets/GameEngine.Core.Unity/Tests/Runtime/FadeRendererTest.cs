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
    /// <see cref="FadeRendererTest"/>
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
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, duration, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-in phase
            fadeRenderer.StartFadeIn(() => callbackCalled = true);
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
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, duration, true);
            Assert.IsTrue(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-out phase
            fadeRenderer.StartFadeOut(() => callbackCalled = true);
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
            bool callbackCalled = false;

            // Create a fade renderer associated with the image, which makes it initially inactive
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, duration, false);
            Assert.IsFalse(m_TestImage.gameObject.activeInHierarchy);

            // Launch the fade-between phase
            fadeRenderer.StartFadeBetween(() => callbackCalled = true);
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

            // The callback method has been called and the renderer return the correct state
            Assert.IsTrue(callbackCalled);
            Assert.IsTrue(fadeRenderer.IsFullyOut);
        }

        [UnityTest]
        public IEnumerator ControlFadeParameters()
        {
            // Create a fade renderer associated with the image (initially inactive)
            FadeRenderer fadeRenderer = new FadeRenderer(m_TestImage, false);
            m_Simulation.RegisterBehaviour(MonoBehaviourEvent.Update, fadeRenderer.Update);

            // With a zero fade duration, the image appears in its final fade state in the next frame
            fadeRenderer.FadeDuration = 0;
            Assert.IsTrue(fadeRenderer.IsFullyOut);
            fadeRenderer.StartFadeIn();
            yield return null;
            Assert.IsTrue(fadeRenderer.IsFullyIn);

            // With a black transparent fade color, the image darkens when it fade out
            fadeRenderer.FadeDuration = 0.05f;
            fadeRenderer.FadeColor = new Color(0, 0, 0, 0);
            Assert.AreEqual(Color.white, m_TestImage.color);
            fadeRenderer.StartFadeOut();
            yield return new WaitForSeconds(fadeRenderer.FadeDuration / 2);
            Assert.IsTrue(m_TestImage.color.r < 1 && m_TestImage.color.g < 1 && m_TestImage.color.b < 1);
            yield return new WaitForSeconds(fadeRenderer.FadeDuration / 2);
            Assert.AreEqual(new Color(0, 0, 0, 0), m_TestImage.color);

            // With a black opaque fade color, the image blend to black instead of fading
            fadeRenderer.FadeDuration = 0;
            fadeRenderer.FadeColor = Color.black;
            fadeRenderer.StartFadeBetween();
            yield return null;
            Assert.AreEqual(Color.white, m_TestImage.color);
            yield return null;
            Assert.AreEqual(Color.black, m_TestImage.color);
        }
    }
}
