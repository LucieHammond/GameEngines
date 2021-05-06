using GameEngine.Core.Logger;
using GameEngine.Core.Logger.Base;
using System;
using System.Collections.Generic;

/// <summary>
/// A static class providing methods to easily configure the log service provided by the Log class
/// </summary>
public static class LogSetup
{
    /// <summary>
    /// Initialize the log service based on the given log settings
    /// </summary>
    /// <param name="settings">The log settings to use for configuration</param>
    public static void InitializeLogs(LogSettings settings)
    {
        UnityLogger unityLogger = new UnityLogger(settings.TagsColors);
        Log.AddLogger(unityLogger, settings.MinLogLevel, settings.ActivateFiltering ? settings.TagsFilter : null);

        foreach (KeyValuePair<BaseLoggerType, object[]> additionalLogger in settings.AdditionalLoggers)
        {
            ILogger logger = null;
            object[] parameters = additionalLogger.Value;
            int index = 0;
            switch (additionalLogger.Key)
            {
                case BaseLoggerType.ConsoleLogger:
                    logger = new ConsoleLogger();
                    break;
                case BaseLoggerType.DebugLogger:
                    logger = new DebugLogger();
                    break;
                case BaseLoggerType.FileLogger:
                    logger = new FileLogger((string)parameters[index++]);
                    break;
                case BaseLoggerType.NetLogger:
                    logger = new NetLogger(new Uri((string)parameters[index++]), (string)parameters[index++], (string)parameters[index++]);
                    break;
            }

            if (logger != null)
            {
                Log.AddLogger(logger, (LogLevel)parameters[index], null);
            }
        }
    }
}