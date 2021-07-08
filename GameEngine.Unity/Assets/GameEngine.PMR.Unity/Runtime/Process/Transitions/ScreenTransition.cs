using GameEngine.Core.Logger;
using GameEngine.Core.Rendering;
using GameEngine.PMR.Process.Transitions.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable screen canvas loaded from a scene or a prefab
    /// </summary>
    public class ScreenTransition : Transition
    {
        private const string TAG = "ScreenTransition";
        private const string TRANSITION_LAYER = "Transition";

        /// <summary>
        /// A loading screen that should be managed by the transition
        /// </summary>
        public LoadingScreen LoadingScreen { get; set; }

        /// <summary>
        /// A list of custom transition elements that should be managed by the transition
        /// </summary>
        public List<ITransitionElement> CustomElements { get; set; }

        private Action m_LoadScreen;
        private Action m_UnloadScreen;
        private Canvas m_ScreenCanvas;
        private float m_FadeDuration;
        private List<FadeRenderer> m_FadeRenderers;

        /// <summary>
        /// Create a new instance of ScreenTransition
        /// </summary>
        /// <param name="transitionScene">The name of the transition scene to load</param>
        /// <param name="canvasName">The name of the canvas gameobject defining the transition screen in the scene</param>
        /// <param name="fadeDuration">The time it should take to fade the screen (in seconds)</param>
        public ScreenTransition(string transitionScene, string canvasName, float fadeDuration)
        {
            m_LoadScreen = () =>
            {
                SceneManager.LoadScene(transitionScene, LoadSceneMode.Additive);
                GameObject canvasObject = GameObject.Find(canvasName);
                if (canvasObject == null)
                {
                    Log.Error(TAG, $"Failed to find the transition screen canvas named {canvasName} in the scene {transitionScene}");
                    return;
                }

                m_ScreenCanvas = SetupCanvas(canvasObject);
            };
            m_UnloadScreen = () =>
            {
                SceneManager.UnloadSceneAsync(transitionScene);
            };
            m_FadeDuration = fadeDuration;
            m_FadeRenderers = new List<FadeRenderer>();
        }

        /// <summary>
        /// Create a new instance of ScreenTransition
        /// </summary>
        /// <param name="transitionPrefab">The path of the prefab canvas gameobject defining the transition screen</param>
        /// <param name="fadeDuration">The time it should take to fade the screen (in seconds)</param>
        public ScreenTransition(string transitionPrefab, float fadeDuration)
        {
            m_LoadScreen = () =>
            {
                GameObject prefabObject = Resources.Load<GameObject>(transitionPrefab);
                if (prefabObject == null)
                {
                    Log.Error(TAG, $"Failed to find the transition screen prefab at resource path {transitionPrefab}");
                    return;
                }

                GameObject canvasObject = GameObject.Instantiate(prefabObject);
                m_ScreenCanvas = SetupCanvas(canvasObject);
            };
            m_UnloadScreen = () =>
            {
                GameObject.Destroy(m_ScreenCanvas);
            };
            m_FadeDuration = fadeDuration;
            m_FadeRenderers = new List<FadeRenderer>();
        }

        /// <summary>
        /// <see cref="Transition.Initialize()"/>
        /// </summary>
        protected override void Initialize()
        {
            m_LoadScreen();

            if (m_ScreenCanvas != null)
            {
                foreach (Image image in m_ScreenCanvas.GetComponentsInChildren<Image>(true))
                    m_FadeRenderers.Add(new FadeRenderer(image, m_FadeDuration, false));
                foreach (RawImage rawImage in m_ScreenCanvas.GetComponentsInChildren<RawImage>(true))
                    m_FadeRenderers.Add(new FadeRenderer(rawImage, m_FadeDuration, false));
                foreach (Text text in m_ScreenCanvas.GetComponentsInChildren<Text>(true))
                    m_FadeRenderers.Add(new FadeRenderer(text, m_FadeDuration, false));
            }

            LoadingScreen?.Setup();
        }

        /// <summary>
        /// <see cref="Transition.Enter()"/>
        /// </summary>
        protected override void Enter()
        {
            OnStartActivation();
            foreach (FadeRenderer fadeRenderer in m_FadeRenderers)
            {
                fadeRenderer.StartFadeIn();
            }
        }

        /// <summary>
        /// <see cref="Transition.Update()"/>
        /// </summary>
        protected override void Update()
        {
            if (State != TransitionState.Active)
            {
                foreach (FadeRenderer fadeRenderer in m_FadeRenderers)
                {
                    fadeRenderer.Update();
                }

                if (State == TransitionState.Activating && m_FadeRenderers.All((renderer) => renderer.IsFullyIn))
                {
                    OnFinishActivation();
                    MarkActivated();
                }

                if (State == TransitionState.Deactivating && m_FadeRenderers.All((renderer) => renderer.IsFullyOut))
                {
                    OnFinishDeactivation();
                    MarkDeactivated();
                }
            }
            else
            {
                LoadingScreen?.Update(m_LoadingProgress, m_LoadingAction);
                CustomElements?.ForEach((element) => element.UpdateTransition());
            }
        }

        /// <summary>
        /// <see cref="Transition.Exit()"/>
        /// </summary>
        protected override void Exit()
        {
            OnStartDeactivation();
            foreach (FadeRenderer fadeRenderer in m_FadeRenderers)
            {
                fadeRenderer.StartFadeOut();
            }
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {
            m_UnloadScreen();
        }

        private Canvas SetupCanvas(GameObject canvasObject)
        {
            if (!canvasObject.TryGetComponent(out Canvas canvas))
            {
                Log.Error(TAG, $"Failed to find a Canvas component on the {canvasObject.name} object loaded for the transition");
                return default;
            }

            canvasObject.layer = LayerMask.NameToLayer(TRANSITION_LAYER);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            return canvas;
        }

        private void OnStartActivation()
        {
            CustomElements?.ForEach((element) => element.OnStartTransitionActivation());
        }

        private void OnFinishActivation()
        {
            CustomElements?.ForEach((element) => element.OnFinishTransitionActivation());
        }

        private void OnStartDeactivation()
        {
            CustomElements?.ForEach((element) => element.OnStartTransitionDeactivation());
        }

        private void OnFinishDeactivation()
        {
            CustomElements?.ForEach((element) => element.OnFinishTransitionDeactivation());
        }
    }
}
