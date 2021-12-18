using GameEngine.Core.Logger;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameEngine.PMR.Unity.Basics.Input
{
    [RuleAccess(typeof(IInputService))]
    public class InputService : GameRule, IInputService
    {
        private const string TAG = "InputService";

        [RuleDependency(RuleDependencySource.Service, Required = true)]
        public IConfigurationService ConfigurationService;

        private InputActionAsset m_ActionsAsset;

        private Dictionary<string, Dictionary<string, ContextCallback>> m_ActionCallbacks;

        #region GameRule cycle
        public InputService()
        {
            m_ActionCallbacks = new Dictionary<string, Dictionary<string, ContextCallback>>();
        }

        protected override void Initialize()
        {
            InputConfiguration config = ConfigurationService.GetConfiguration<InputConfiguration>(InputConfiguration.CONFIG_ID);
            m_ActionsAsset = config.ActionsAsset;
            if (!string.IsNullOrEmpty(config.DefaultActionMap))
                SetCurrentActionMap(config.DefaultActionMap);

            foreach (InputActionMap actionMap in m_ActionsAsset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    string fullActionName = $"{actionMap.name}/{action.name}";
                    m_ActionCallbacks.Add(fullActionName, new Dictionary<string, ContextCallback>());
                    action.performed += OnAction;
                    action.canceled += OnAction;
                }
            }

            MarkInitialized();
        }

        protected override void Unload()
        {
            m_ActionCallbacks.Clear();

            foreach (InputActionMap actionMap in m_ActionsAsset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    action.performed -= OnAction;
                    action.canceled -= OnAction;
                }
            }

            MarkUnloaded();
        }

        protected override void Update()
        {
            InputSystem.Update();
        }
        #endregion

        #region IInputService API
        public void SetCurrentActionMap(string actionMapName)
        {
            foreach (InputActionMap actionMap in m_ActionsAsset.actionMaps)
            {
                if (actionMap.name == actionMapName)
                    actionMap.Enable();
                else
                    actionMap.Disable();
            }
        }

        public void EnableActionMap(string actionMapName)
        {
            m_ActionsAsset.FindActionMap(actionMapName, true).Enable();
        }

        public void DisableActionMap(string actionMapName)
        {
            m_ActionsAsset.FindActionMap(actionMapName, true).Disable();
        }

        public string GetBindingDisplay(string actionMap, string actionName, string controlSchemes)
        {
            return m_ActionsAsset.FindActionMap(actionMap, true).FindAction(actionName, true).GetBindingDisplayString(0, controlSchemes);
        }

        public void RebindAction(string actionMap, string actionName, string controlSchemes, string bindingPath)
        {
            m_ActionsAsset.FindActionMap(actionMap, true).FindAction(actionName, true).ApplyBindingOverride(bindingPath, controlSchemes);
        }


        public void RegisterButtonCallback(string actionMap, string actionName, ButtonCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.performed)
                    callback.Invoke();
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterStatusCallback(string actionMap, string actionName, StatusCallback callback)
        {
            ContextCallback contextCallback = (context) => callback(context.control.IsPressed());
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterNumericCallback(string actionMap, string actionName, NumericCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(int))
                    callback(context.ReadValue<int>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(float))
                    callback(context.ReadValue<float>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Vector2))
                    callback(context.ReadValue<Vector2>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Vector3))
                    callback(context.ReadValue<Vector3>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterRotationCallback(string actionMap, string actionName, RotationCallback callback)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Quaternion))
                    callback(context.ReadValue<Quaternion>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback);
        }

        public void RegisterContextCallback(string actionMap, string actionName, ContextCallback callback)
        {
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), callback);
        }


        public void UnregisterButtonCallback(string actionMap, string actionName, ButtonCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterStatusCallback(string actionMap, string actionName, StatusCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterNumericCallback(string actionMap, string actionName, NumericCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterRotationCallback(string actionMap, string actionName, RotationCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }

        public void UnregisterContextCallback(string actionMap, string actionName, ContextCallback callback)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method));
        }
        #endregion

        #region private
        private void OnAction(InputAction.CallbackContext context)
        {
            string fullActionName = $"{context.action.actionMap.name}/{context.action.name}";
            if (m_ActionCallbacks.ContainsKey(fullActionName))
            {
                foreach (ContextCallback callback in m_ActionCallbacks[fullActionName].Values)
                {
                    callback(context);
                }
            }
        }

        private void RegisterActionCallback(string fullActionName, string callbackId, ContextCallback actionCallback)
        {
            if (m_ActionCallbacks.ContainsKey(fullActionName))
            {
                if (!m_ActionCallbacks[fullActionName].ContainsKey(callbackId))
                {
                    m_ActionCallbacks[fullActionName].Add(callbackId, actionCallback);
                }
                else
                {
                    Log.Error(TAG, $"Duplicated registry: Callback {callbackId} is already registered for action {fullActionName}");
                }
            }
            else
            {
                Log.Error(TAG, $"Action doesn't exist: Cannot find the {fullActionName} action in the action maps definitions");
            }
        }

        private void UnregisterActionCallback(string fullActionName, string callbackId)
        {
            if (m_ActionCallbacks.ContainsKey(fullActionName))
            {
                if (m_ActionCallbacks[fullActionName].ContainsKey(callbackId))
                {
                    m_ActionCallbacks[fullActionName].Remove(callbackId);
                }
                else
                {
                    Log.Error(TAG, $"Unknown registry: Callback {callbackId} is not registered for action {fullActionName}");
                }
            }
            else
            {
                Log.Error(TAG, $"Action doesn't exist: Cannot find the {fullActionName} action in the action maps definitions");
            }
        }

        private string GetCallbackId(MethodInfo callbackInfo)
        {
            return $"{callbackInfo.DeclaringType.FullName}.{callbackInfo.Name}";
        }
        #endregion
    }
}
