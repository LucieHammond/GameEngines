using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Core.UnityTests.Tools
{
    public class SimulationBehaviour : MonoBehaviour
    {
        Dictionary<MonoBehaviourEvent, Action> m_SimulationBehaviours;

        public SimulationBehaviour()
        {
            m_SimulationBehaviours = new Dictionary<MonoBehaviourEvent, Action>();
            foreach (MonoBehaviourEvent behaviourEvent in Enum.GetValues(typeof(MonoBehaviourEvent)))
            {
                m_SimulationBehaviours.Add(behaviourEvent, null);
            }
        }

        public void RegisterBehaviour(MonoBehaviourEvent behaviourEvent, Action behaviourAction)
        {
            m_SimulationBehaviours[behaviourEvent] += behaviourAction;
        }

        public void UnregisterBehaviour(MonoBehaviourEvent behaviourEvent, Action behaviourAction)
        {
            m_SimulationBehaviours[behaviourEvent] -= behaviourAction;
        }

        public void ResetBehaviours()
        {
            foreach (MonoBehaviourEvent behaviourEvent in Enum.GetValues(typeof(MonoBehaviourEvent)))
            {
                m_SimulationBehaviours[behaviourEvent] = null;
            }
        }

        #region Unity Events
        public void Awake()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.Awake]?.Invoke();
        }

        public void OnEnable()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnEnable]?.Invoke();
        }

        public void Start()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.Start]?.Invoke();
        }

        public void FixedUpdate()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.FixedUpdate]?.Invoke();
        }

        public void Update()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.Update]?.Invoke();
        }

        public void LateUpdate()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.LateUpdate]?.Invoke();
        }

        public void OnGUI()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnGUI]?.Invoke();
        }

        public void OnApplicationPause()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnApplicationPause]?.Invoke();
        }

        public void OnApplicationQuit()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnApplicationQuit]?.Invoke();
        }

        public void OnDisable()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnDisable]?.Invoke();
        }

        public void OnDestroy()
        {
            m_SimulationBehaviours[MonoBehaviourEvent.OnDestroy]?.Invoke();
        }
        #endregion
    }
}
