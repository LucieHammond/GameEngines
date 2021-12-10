using GameEngine.Core.Unity.System;
using GameEngine.PMR.Unity.Basics.Input;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Configuration
{
    public class InputSettings : ScriptableSettings<InputConfiguration>
    {
        public const string ASSET_NAME = "InputSettings";

        public const string CONFIG_ID = InputConfiguration.CONFIG_ID;

        public static InputSettings GetSettings()
        {
            InputSettings settings = Resources.Load<InputSettings>(ASSET_NAME);
            if (settings == null)
                settings = CreateInstance<InputSettings>();

            return settings;
        }

        public override bool Validate()
        {
            if (Configuration.ActionsAsset != null)
                return false;

            if (string.IsNullOrEmpty(Configuration.DefaultActionMap) || 
                Configuration.ActionsAsset.FindActionMap(Configuration.DefaultActionMap) == null)
                return false;

            return true;
        }
    }
}
