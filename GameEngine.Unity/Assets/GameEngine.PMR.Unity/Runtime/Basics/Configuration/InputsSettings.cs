using GameEngine.Core.Unity.System;
using GameEngine.PMR.Unity.Basics.Input;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Configuration
{
    public class InputsSettings : ScriptableSettings<InputConfiguration>
    {
        public const string ASSET_NAME = "InputsSettings";

        public const string CONFIG_ID = InputConfiguration.CONFIG_ID;

        public static InputsSettings GetSettings()
        {
            InputsSettings settings = Resources.Load<InputsSettings>(ASSET_NAME);
            if (settings == null)
                settings = CreateInstance<InputsSettings>();

            return settings;
        }

        public override bool Validate()
        {
            if (Configuration.ActionsAsset == null)
                return false;

            if (string.IsNullOrEmpty(Configuration.DefaultActionMap) || 
                Configuration.ActionsAsset.FindActionMap(Configuration.DefaultActionMap) == null)
                return false;

            return true;
        }
    }
}
