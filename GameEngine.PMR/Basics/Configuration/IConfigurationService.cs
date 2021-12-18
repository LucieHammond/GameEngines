namespace GameEngine.PMR.Basics.Configuration
{
    /// <summary>
    /// Interface that exposes the operations provided by the ConfigurationService
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Retrieve a configuration by reference name
        /// </summary>
        /// <typeparam name="TConfig">The type of the configuration</typeparam>
        /// <param name="configId">The id of the configuration</param>
        /// <returns>A configuration object containing setup values</returns>
        TConfig GetConfiguration<TConfig>(string configId) where TConfig : class;
    }
}
