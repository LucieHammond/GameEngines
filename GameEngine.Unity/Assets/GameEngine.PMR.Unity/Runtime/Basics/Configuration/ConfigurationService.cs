using GameEngine.Core.Logger;
using GameEngine.Core.Unity.System;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Configuration
{
    /// <summary>
    /// A service that provides project configurations, which can be defined and modified as ScriptableSettings.
    /// </summary>
    [RuleAccess(typeof(IConfigurationService))]
    public class ConfigurationService : GameRule, IConfigurationService
    {
        private const string TAG = "ConfigurationService";

        /// <summary>
        /// A delegate representing a method for getting a ScriptableSettings object
        /// </summary>
        /// <typeparam name="T">The type of the configuration</typeparam>
        /// <returns>A ScriptableSettings object wrapping the configuration</returns>
        protected delegate ScriptableSettings<T> GetSettingsDelegate<T>();
        
        private Dictionary<string, object> m_RegisteredConfigurations;

        public ConfigurationService()
        {
            m_RegisteredConfigurations = new Dictionary<string, object>();
        }

        #region GameRule Cycle
        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
            RegisterSettings(ContentSettings.CONFIG_ID, () => ContentSettings.GetSettings());
            RegisterSettings(InputsSettings.CONFIG_ID, () => InputsSettings.GetSettings());

            MarkInitialized();
        }

        /// <summary>
        /// Unload
        /// </summary>
        protected override void Unload()
        {
            m_RegisteredConfigurations.Clear();

            MarkUnloaded();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() { }
        #endregion

        #region IConfigurationService API
        /// <summary>
        /// Retrieve a configuration by reference name
        /// </summary>
        /// <typeparam name="TConfig">The type of the configuration</typeparam>
        /// <param name="configId">The id of the configuration</param>
        /// <returns>A configuration object containing setup values</returns>
        public TConfig GetConfiguration<TConfig>(string configId) where TConfig : class
        {
            if (m_RegisteredConfigurations.TryGetValue(configId, out object settings))
            {
                if (settings is TConfig)
                {
                    return settings as TConfig;
                } 
                else
                {
                    Log.Error(TAG, $"Configuration {configId} does not match type {typeof(TConfig)}");
                }
            }
            else
            {
                Log.Error(TAG, $"Missing configuration {configId}");
            }

            return default;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Register a specific configuration wrapped in a ScriptableSettings so that it can be retrieved
        /// </summary>
        /// <typeparam name="T">The type of the configuration</typeparam>
        /// <param name="settingId">The id of the configuration</param>
        /// <param name="getSettings">A method that loads or create the ScriptableSettings wrapping the configuration</param>
        protected void RegisterSettings<T>(string settingId, GetSettingsDelegate<T> getSettings)
        {
            m_RegisteredConfigurations.Add(settingId, getSettings().Configuration);
        }
        #endregion
    }
}
