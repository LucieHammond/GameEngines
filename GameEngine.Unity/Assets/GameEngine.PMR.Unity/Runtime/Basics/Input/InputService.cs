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
        private bool m_CallSynchronously;

        private Dictionary<string, Dictionary<string, ContextCallback>> m_ActionStartCallbacks;
        private Dictionary<string, Dictionary<string, ContextCallback>> m_ActionPerformCallbacks;
        private Dictionary<string, Dictionary<string, ContextCallback>> m_ActionCancelCallbacks;
        private ConcurrentQueue<Action> m_CallbackQueue;

        #region GameRule cycle
        public InputService()
        {
            m_ActionStartCallbacks = new Dictionary<string, Dictionary<string, ContextCallback>>();
            m_ActionPerformCallbacks = new Dictionary<string, Dictionary<string, ContextCallback>>();
            m_ActionCancelCallbacks = new Dictionary<string, Dictionary<string, ContextCallback>>();
            m_CallbackQueue = new ConcurrentQueue<Action>();
        }

        protected override void Initialize()
        {
            InputConfiguration config = ConfigurationService.GetConfiguration<InputConfiguration>(InputConfiguration.CONFIG_ID);
            m_ActionsAsset = config.ActionsAsset;
            m_CallSynchronously = config.CallSynchronously;
            if (!string.IsNullOrEmpty(config.DefaultActionMap))
                SetCurrentActionMap(config.DefaultActionMap);

            foreach (InputActionMap actionMap in m_ActionsAsset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    string fullActionName = $"{actionMap}/{action}";
                    m_ActionStartCallbacks.Add(fullActionName, new Dictionary<string, ContextCallback>());
                    m_ActionPerformCallbacks.Add(fullActionName, new Dictionary<string, ContextCallback>());
                    m_ActionCancelCallbacks.Add(fullActionName, new Dictionary<string, ContextCallback>());
                    action.started += OnStarted;
                    action.performed += OnPerformed;
                    action.canceled += OnCanceled;
                }
            }

            MarkInitialized();
        }

        protected override void Unload()
        {
            m_ActionStartCallbacks.Clear();
            m_ActionPerformCallbacks.Clear();
            m_ActionCancelCallbacks.Clear();

            foreach (InputActionMap actionMap in m_ActionsAsset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    action.started -= OnStarted;
                    action.performed -= OnPerformed;
                    action.canceled -= OnCanceled;
                }
            }

            MarkUnloaded();
        }

        protected override void Update()
        {
            if (m_CallSynchronously)
            {
                while (m_CallbackQueue.TryDequeue(out Action callback))
                {
                    callback();
                }
            }
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


        public void RegisterButtonCallback(string actionMap, string actionName, ButtonCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) => callback.Invoke();
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterStatusCallback(string actionMap, string actionName, StatusCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) => callback(context.control.IsPressed());
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterNumericCallback(string actionMap, string actionName, NumericCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(int))
                    callback(context.ReadValue<int>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(float))
                    callback(context.ReadValue<float>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Vector2))
                    callback(context.ReadValue<Vector2>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Vector3))
                    callback(context.ReadValue<Vector3>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterRotationCallback(string actionMap, string actionName, RotationCallback callback, InputEvent phase = InputEvent.Perform)
        {
            ContextCallback contextCallback = (context) =>
            {
                if (context.valueType == typeof(Quaternion))
                    callback(context.ReadValue<Quaternion>());
            };
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), contextCallback, phase);
        }

        public void RegisterContextCallback(string actionMap, string actionName, ContextCallback callback, InputEvent phase = InputEvent.Perform)
        {
            RegisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), callback, phase);
        }


        public void UnregisterButtonCallback(string actionMap, string actionName, ButtonCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterStatusCallback(string actionMap, string actionName, StatusCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterNumericCallback(string actionMap, string actionName, NumericCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterRotationCallback(string actionMap, string actionName, RotationCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }

        public void UnregisterContextCallback(string actionMap, string actionName, ContextCallback callback, InputEvent phase = InputEvent.Perform)
        {
            UnregisterActionCallback($"{actionMap}/{actionName}", GetCallbackId(callback.Method), phase);
        }
        #endregion

        #region private
        private void OnStarted(InputAction.CallbackContext context)
        {
            string fullActionName = $"{context.action.actionMap.name}/{context.action.name}";
            if (m_ActionStartCallbacks.ContainsKey(fullActionName))
            {
                foreach (ContextCallback callback in m_ActionStartCallbacks[fullActionName].Values)
                {
                    if (!m_CallSynchronously)
                        callback(context);
                    else
                        m_CallbackQueue.Enqueue(() => callback(context));
                }
            }
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            string fullActionName = $"{context.action.actionMap.name}/{context.action.name}";
            if (m_ActionPerformCallbacks.ContainsKey(fullActionName))
            {
                foreach (ContextCallback callback in m_ActionPerformCallbacks[fullActionName].Values)
                {
                    if (!m_CallSynchronously)
                        callback(context);
                    else
                        m_CallbackQueue.Enqueue(() => callback(context));
                }
            }
        }

        private void OnCanceled(InputAction.CallbackContext context)
        {
            string fullActionName = $"{context.action.actionMap.name}/{context.action.name}";
            if (m_ActionCancelCallbacks.ContainsKey(fullActionName))
            {
                foreach (ContextCallback callback in m_ActionCancelCallbacks[fullActionName].Values)
                {
                    if (!m_CallSynchronously)
                        callback(context);
                    else
                        m_CallbackQueue.Enqueue(() => callback(context));
                }
            }
        }

        private void RegisterActionCallback(string fullActionName, string callbackId, ContextCallback actionCallback, InputEvent phase)
        {
            Dictionary<string, Dictionary<string, ContextCallback>> actionCallbacks =
                phase == InputEvent.Start ? m_ActionStartCallbacks :
                phase == InputEvent.Perform ? m_ActionPerformCallbacks :
                phase == InputEvent.Cancel ? m_ActionCancelCallbacks : null;

            if (actionCallbacks.ContainsKey(fullActionName))
            {
                if (!actionCallbacks[fullActionName].ContainsKey(callbackId))
                {
                    actionCallbacks[fullActionName].Add(callbackId, actionCallback);
                }
                else
                {
                    Log.Error(TAG, $"Duplicated registry: Callback {callbackId} is already registered for action {fullActionName} at event {phase}");
                }
            }
            else
            {
                Log.Error(TAG, $"Action doesn't exist: Cannot find the {fullActionName} action in the action maps definitions");
            }
        }

        private void UnregisterActionCallback(string fullActionName, string callbackId, InputEvent phase)
        {
            Dictionary<string, Dictionary<string, ContextCallback>> actionCallbacks =
                phase == InputEvent.Start ? m_ActionStartCallbacks :
                phase == InputEvent.Perform ? m_ActionPerformCallbacks :
                phase == InputEvent.Cancel ? m_ActionCancelCallbacks : null;

            if (actionCallbacks.ContainsKey(fullActionName))
            {
                if (actionCallbacks[fullActionName].ContainsKey(callbackId))
                {
                    actionCallbacks[fullActionName].Remove(callbackId);
                }
                else
                {
                    Log.Error(TAG, $"Unknown registry: Callback {callbackId} is not registered for action {fullActionName} at event {phase}");
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
