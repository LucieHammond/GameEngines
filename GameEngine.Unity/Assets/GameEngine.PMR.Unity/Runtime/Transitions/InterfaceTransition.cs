using GameEngine.Core.Logger;
using GameEngine.PMR.Process.Transitions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable interface loaded from a scene or a prefab
    /// </summary>
    public class InterfaceTransition : StandardTransition
    {
        private const string TAG = "InterfaceTransition";

        private delegate void LoadDelegate(Action onLoad);
        private delegate void UnloadDelegate();

        private GameObject m_InterfaceCanvas;
        
        private LoadDelegate m_LoadAction;
        private UnloadDelegate m_UnloadAction;
        private Queue<Func<ITransitionElement>> m_ElementsToInclude;

        /// <summary>
        /// Create a new instance of InterfaceTransition
        /// </summary>
        /// <param name="transitionScene">The name of the transition scene to load</param>
        /// <param name="canvasName">The name of the canvas gameobject containing the transition interface in the scene</param>
        public InterfaceTransition(string transitionScene, string canvasName)
        {
            m_ElementsToInclude = new Queue<Func<ITransitionElement>>();
            
            m_LoadAction = (onLoad) =>
            {
                SceneManager.LoadSceneAsync(transitionScene, LoadSceneMode.Additive).completed += (_) =>
                {
                    m_InterfaceCanvas = GameObject.Find(canvasName);
                    if (m_InterfaceCanvas == null)
                    {
                        Log.Error(TAG, $"Failed to find the transition interface named {canvasName} in the scene {transitionScene}");
                        return;
                    }

                    onLoad?.Invoke();
                };
            };

            m_UnloadAction = () =>
            {
                SceneManager.UnloadSceneAsync(transitionScene);
            };
        }

        /// <summary>
        /// Create a new instance of InterfaceTransition
        /// </summary>
        /// <param name="transitionPrefab">The id of the prefab resource defining the transition interface</param>
        public InterfaceTransition(string transitionPrefab)
        {
            m_ElementsToInclude = new Queue<Func<ITransitionElement>>();
            
            m_LoadAction = (onLoad) =>
            {
                GameObject prefabObject = Resources.Load<GameObject>(transitionPrefab);
                if (prefabObject == null)
                {
                    Log.Error(TAG, $"Failed to find the prefab for transition interface {transitionPrefab} in resources");
                    return;
                }

                m_InterfaceCanvas = GameObject.Instantiate(prefabObject);
                onLoad?.Invoke();
            };
            
            m_UnloadAction = () =>
            {
                GameObject.Destroy(m_InterfaceCanvas);
            };
        }

        /// <summary>
        /// Add a custom transition object to be managed by the transition
        /// </summary>
        /// <typeparam name="T">The type of the Unity component on which the transition takes place</typeparam>
        /// <param name="objectName">The name of the concerned gameobject inside the interface</param>
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
        /// <see cref="Transition.Prepare()"/>
        /// </summary>
        protected override void Prepare()
        {
            m_LoadAction(() =>
            {
                SetupCanvas(m_InterfaceCanvas);
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
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {
            m_UnloadAction();
            m_CustomElements.Clear();
        }

        private void SetupCanvas(GameObject canvasObject)
        {
            if (!canvasObject.TryGetComponent(out Canvas canvas))
            {
                Log.Error(TAG, $"Failed to find a Canvas component on the {canvasObject.name} object loaded for the transition");
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }

        private T FindComponent<T>(string objectName) where T : Component
        {
            GameObject gameObject = m_InterfaceCanvas.transform.Find(objectName)?.gameObject;
            if (gameObject == null)
            {
                Log.Error(TAG, $"Failed to find a gameobject named {objectName} inside the interface {m_InterfaceCanvas}");
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
