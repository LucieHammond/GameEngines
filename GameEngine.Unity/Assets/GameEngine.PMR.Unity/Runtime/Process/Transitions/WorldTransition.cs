using GameEngine.Core.Logger;
using GameEngine.Core.Rendering;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Process.Transitions.Elements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable world scene with a specialized camera
    /// </summary>
    public class WorldTransition : Transition
    {
        private const string TAG = "WorldTransition";

        /// <summary>
        /// The solid color of the panel that separates the module scene from the transition scene (default is black)
        /// </summary>
        public Color SeparationColor { get; set; }

        /// <summary>
        /// A loading screen that should be managed by the transition
        /// </summary>
        public LoadingScreen LoadingScreen { get; set; }

        /// <summary>
        /// A list of custom transition elements that should be managed by the transition
        /// </summary>
        public List<ITransitionElement> CustomElements { get; set; }

        private Action m_LoadWorld;
        private Action m_UnloadWorld;
        private Camera m_Camera;
        private float m_FadeDuration;
        private FadeRenderer m_FadeRenderer;

        /// <summary>
        /// Create a new instance of WorldTransition
        /// </summary>
        /// <param name="transitionScenes">The list of transition scenes to load (by name)</param>
        /// <param name="cameraName">The name of the camera gameobject that should render the transition</param>
        /// <param name="fadeDuration">The time it should takes to fade from the module scene to the transition scene</param>
        public WorldTransition(List<string> transitionScenes, string cameraName, float fadeDuration)
        {
            m_LoadWorld = () =>
            {
                foreach (string scene in transitionScenes)
                    SceneManager.LoadScene(scene, LoadSceneMode.Additive);

                m_Camera = SetupCamera(cameraName);
            };
            m_UnloadWorld = () =>
            {
                foreach (string scene in transitionScenes)
                    SceneManager.UnloadSceneAsync(scene);

                if (m_Camera != null)
                    m_Camera.gameObject.SetActive(false);
            };

            m_FadeDuration = fadeDuration;
            SeparationColor = Color.black;
        }

        /// <summary>
        /// <see cref="Transition.Initialize()"/>
        /// </summary>
        protected override void Initialize()
        {
            Image separationPanel = GetSeparationPanel();
            separationPanel.color = SeparationColor;
            m_FadeRenderer = new FadeRenderer(separationPanel, m_FadeDuration / 2, false);
        }

        /// <summary>
        /// <see cref="Transition.Enter()"/>
        /// </summary>
        protected override void Enter()
        {
            m_FadeRenderer.StartFadeIn(onFinish: () =>
            {
                m_LoadWorld();
                LoadingScreen?.Setup();
                OnStartActivation();

                m_FadeRenderer.StartFadeOut(onFinish: () =>
                {
                    OnFinishActivation();
                    MarkActivated();
                });
            });
        }

        /// <summary>
        /// <see cref="Transition.Update()"/>
        /// </summary>
        protected override void Update()
        {
            if (State != TransitionState.Active)
            {
                m_FadeRenderer.Update();
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
            m_FadeRenderer.StartFadeIn(onFinish: () =>
            {
                m_UnloadWorld();
                OnStartDeactivation();

                m_FadeRenderer.StartFadeOut(onFinish: () =>
                {
                    OnFinishDeactivation();
                    MarkDeactivated();
                });
            });
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {

        }

        private Image GetSeparationPanel()
        {
            string canvasName = "Separator";
            string panelName = "Solid Color Panel";

            GameObject panelObject = GameObject.Find($"{canvasName}/{panelName}");
            if (panelObject == null)
            {
                GameObject canvasObject = new GameObject(canvasName);
                GameObject.DontDestroyOnLoad(canvasObject);
                canvasObject.layer = LayerMask.NameToLayer("Transition");

                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 2000;

                panelObject = new GameObject(panelName);
                panelObject.transform.parent = canvasObject.transform;

                panelObject.CreateDefaultRectTransform();
                panelObject.AddComponent<Image>();
            }

            return panelObject.GetComponent<Image>();
        }

        private Camera SetupCamera(string cameraName)
        {
            GameObject cameraObject = GameObject.Find(cameraName);

            if (cameraObject == null)
            {
                Log.Error(TAG, $"Failed to find the camera gameobject named {cameraName} for the transition");
                return null;
            }
            if (!cameraObject.TryGetComponent(out Camera camera))
            {
                Log.Error(TAG, $"Failed to find a Camera component on the {cameraObject.name} object loaded for the transition");
                return null;
            }

            camera.depth = 1;
            return camera;
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
