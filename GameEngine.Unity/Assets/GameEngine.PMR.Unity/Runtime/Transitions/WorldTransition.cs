using GameEngine.Core.Logger;
using GameEngine.Core.Unity.Rendering;
using GameEngine.Core.Unity.Utilities;
using GameEngine.PMR.Process.Transitions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable world scene with a specialized camera
    /// </summary>
    public class WorldTransition : StandardTransition
    {
        private const string TAG = "WorldTransition";

        private delegate void LoadDelegate(Action onLoad);
        private delegate void UnloadDelegate();

        private bool m_SeparationActive;
        private float m_SeparationTime;
        private Color m_SeparationColor;
        private FadeRenderer m_SeparationRenderer;

        private LoadDelegate m_LoadAction;
        private UnloadDelegate m_UnloadAction;
        private Action m_SceneActivation;
        private Action m_SceneDeactivation;
        private Queue<Func<ITransitionElement>> m_ElementsToInclude;

        /// <summary>
        /// Create a new instance of WorldTransition
        /// </summary>
        /// <param name="transitionScene">The name of the transition scene to load</param>
        /// <param name="activation">A method to call during the transition entry for activating the scene</param>
        /// <param name="deactivation">A method to call during the transition exit for deactivating the scene</param>
        public WorldTransition(string transitionScene, Action activation = null, Action deactivation = null)
        {
            m_ElementsToInclude = new Queue<Func<ITransitionElement>>();
            m_SceneActivation = activation;
            m_SceneDeactivation = deactivation;

            m_LoadAction = (onLoad) =>
            {
                SceneManager.LoadSceneAsync(transitionScene, LoadSceneMode.Additive).completed += (_) => onLoad?.Invoke();
            };

            m_UnloadAction = () =>
            {
                SceneManager.UnloadSceneAsync(transitionScene);
            };
        }

        /// <summary>
        /// Add a custom transition object to be managed by the transition
        /// </summary>
        /// <typeparam name="T">The type of the Unity component on which the transition takes place</typeparam>
        /// <param name="objectName">The name of the concerned gameobject inside the world scene</param>
        /// <param name="createElement">A method handling the creation of an ITransitionElement from the retrieved component</param>
        public void AddTransitionObject<T>(string objectName, Func<T, ITransitionElement> createElement)
            where T : Component
        {
            m_ElementsToInclude.Enqueue(() =>
            {
                T component = FindComponent<T>(objectName);
                if (component == null)
                    return null;
                return createElement(component);
            });
        }

        /// <summary>
        /// Activate the display of a fading solid color separation screen to introduce the transition scene
        /// </summary>
        /// <param name="separationColor">The color of the speration screen</param>
        /// <param name="fadeDuration">The duration of the fade between the two scenes</param>
        public void ActivateSeparationScreen(Color separationColor, float fadeDuration)
        {
            m_SeparationActive = true;
            m_SeparationTime = fadeDuration;
            m_SeparationColor = separationColor;
        }

        /// <summary>
        /// <see cref="Transition.Prepare()"/>
        /// </summary>
        protected override void Prepare()
        {
            m_LoadAction(() =>
            {
                if (m_SeparationActive)
                {
                    Image separationPanel = CreateSeparationPanel();
                    separationPanel.color = m_SeparationColor;
                    m_SeparationRenderer = new FadeRenderer(separationPanel, false);
                }

                while (m_ElementsToInclude.Count > 0)
                {
                    ITransitionElement element = m_ElementsToInclude.Dequeue().Invoke();
                    if (element != null)
                        m_CustomElements.Add(element);
                }

                MarkReady();
            });
        }

        /// <summary>
        /// <see cref="Transition.Enter()"/>
        /// </summary>
        protected override void Enter()
        {
            if (m_SeparationActive)
                m_SeparationRenderer.StartFadeBetween(m_SeparationTime / 2, () => m_SceneActivation?.Invoke());
            else
                m_SceneActivation?.Invoke();

            base.Enter();
        }

        /// <summary>
        /// <see cref="Transition.Update()"/>
        /// </summary>
        protected override void Update()
        {
            if (m_SeparationActive && (State == TransitionState.Entering || State == TransitionState.Exiting))
                m_SeparationRenderer.Update();

            base.Update();
        }

        /// <summary>
        /// <see cref="Transition.Exit()"/>
        /// </summary>
        protected override void Exit()
        {
            if (m_SeparationActive)
                m_SeparationRenderer.StartFadeBetween(m_SeparationTime / 2, () => m_SceneDeactivation?.Invoke());
            else
                m_SceneDeactivation?.Invoke();

            base.Exit();
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {
            m_UnloadAction();
            m_CustomElements.Clear();
        }

        private Image CreateSeparationPanel()
        {
            string canvasName = "Separator";
            string panelName = "Panel";

            GameObject panelObject = GameObject.Find($"{canvasName}/{panelName}");
            if (panelObject == null)
            {
                GameObject canvasObject = new GameObject(canvasName);
                GameObject.DontDestroyOnLoad(canvasObject);

                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 101;

                panelObject = new GameObject(panelName);
                panelObject.transform.parent = canvasObject.transform;

                panelObject.CreateDefaultRectTransform();
                panelObject.AddComponent<Image>();
            }

            return panelObject.GetComponent<Image>();
        }

        private T FindComponent<T>(string objectName) where T : Component
        {
            GameObject gameObject = GameObject.Find(objectName);
            if (gameObject == null)
            {
                Log.Error(TAG, $"Failed to find a gameobject named {objectName} in the scene hierarchy");
                return default;
            }

            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                Log.Error(TAG, $"Failed to find a component of type {typeof(T)} on the gameobject {objectName}");
                return default;
            }

            return component;
        }
    }
}
