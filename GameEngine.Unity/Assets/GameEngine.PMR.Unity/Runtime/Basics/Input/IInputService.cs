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

        void RegisterButtonCallback(string actionMap, string actionName, ButtonCallback callback);
        void RegisterStatusCallback(string actionMap, string actionName, StatusCallback callback);
        void RegisterNumericCallback(string actionMap, string actionName, NumericCallback callback);
        void RegisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback);
        void RegisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback);
        void RegisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback);
        void RegisterRotationCallback(string actionMap, string actionName, RotationCallback callback);
        void RegisterContextCallback(string actionMap, string actionName, ContextCallback callback);

        void UnregisterButtonCallback(string actionMap, string actionName, ButtonCallback callback);
        void UnregisterStatusCallback(string actionMap, string actionName, StatusCallback callback);
        void UnregisterNumericCallback(string actionMap, string actionName, NumericCallback callback);
        void UnregisterAxis1DCallback(string actionMap, string actionName, Axis1DCallback callback);
        void UnregisterAxis2DCallback(string actionMap, string actionName, Axis2DCallback callback);
        void UnregisterAxis3DCallback(string actionMap, string actionName, Axis3DCallback callback);
        void UnregisterRotationCallback(string actionMap, string actionName, RotationCallback callback);
        void UnregisterContextCallback(string actionMap, string actionName, ContextCallback callback);
    }
}
