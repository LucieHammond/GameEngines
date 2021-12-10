using UnityEngine;
using UnityEngine.InputSystem;

namespace GameEngine.PMR.Unity.Basics.Input
{
    public delegate void ButtonCallback();
    public delegate void StatusCallback(bool value);
    public delegate void NumericCallback(int value);
    public delegate void Axis1DCallback(float value);
    public delegate void Axis2DCallback(Vector2 value);
    public delegate void Axis3DCallback(Vector3 value);
    public delegate void RotationCallback(Quaternion value);
    public delegate void ContextCallback(InputAction.CallbackContext context);

    public enum InputEvent { Start, Perform, Cancel }

    public interface IInputService
    {
        void SetCurrentActionMap(string actionMapName);
        void EnableActionMap(string actionMapName);
        void DisableActionMap(string actionMapName);

        string GetBindingDisplay(string actionMap, string actionName, string controlSchemes);
        void RebindAction(string actionMap, string actionName, string controlSchemes, string bindingPath);

        void RegisterButtonCallback(string actionMap, string actionName, ButtonCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterStatusCallback(string actionMap, string actionName, StatusCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterNumericCallback(string actionMap, string actionName, NumericCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterRotationCallback(string actionMap, string actionName, RotationCallback callback, InputEvent phase = InputEvent.Perform);
        void RegisterContextCallback(string actionMap, string actionName, ContextCallback callback, InputEvent phase = InputEvent.Perform);

        void UnregisterButtonCallback(string actionMap, string actionName, ButtonCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterStatusCallback(string actionMap, string actionName, StatusCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterNumericCallback(string actionMap, string actionName, NumericCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterRotationCallback(string actionMap, string actionName, RotationCallback callback, InputEvent phase = InputEvent.Perform);
        void UnregisterContextCallback(string actionMap, string actionName, ContextCallback callback, InputEvent phase = InputEvent.Perform);
    }
}
