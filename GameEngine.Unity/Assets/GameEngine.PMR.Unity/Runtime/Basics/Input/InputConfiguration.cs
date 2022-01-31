using System;
using UnityEngine.InputSystem;

namespace GameEngine.PMR.Unity.Basics.Input
{
    [Serializable]
    public class InputConfiguration
    {
        public const string CONFIG_ID = "Input";

        public InputActionAsset ActionsAsset;

        public string DefaultActionMap;
    }
}
