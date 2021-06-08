using GameEngine.Core.Logger;
using GameEnginesTest.Tools.Mocks.Spies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.Tools.Utils
{
    public static class AssertUtils
    {
        public static void ThrowsDerivativeException<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Threw exception {e.GetType().Name} which does not derive from expected {typeof(T).Name}.");
            }

            Assert.Fail($"No exception thrown. {typeof(T).Name} or derivatives were expected.");
        }

        public static void ThrowsNoException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.Fail($"Threw exception {e.GetType().Name} when none was excepted.");
            }
        }

        public static void LogInfo(Action action, string tag = null, string message = null)
        {
            SpyLogger logger = SpyLogger.GetDebugLogger();
            Log.AddLogger(logger);
            if (tag != null)
                logger.OnLogInfo += (logTag, logMessage) => Assert.AreEqual(tag, logTag);
            if (message != null)
                logger.OnLogInfo += (logTag, logMessage) => Assert.AreEqual(message, logMessage);

            action();

            Assert.IsTrue(logger.LogInfoCalls > 0);
            logger.Clear();
            Log.Targets.Clear();
        }

        public static void LogWarning(Action action, string tag = null, string message = null)
        {
            SpyLogger logger = SpyLogger.GetDebugLogger();
            Log.AddLogger(logger);
            if (tag != null)
                logger.OnLogWarning += (logTag, logMessage) => Assert.AreEqual(tag, logTag);
            if (message != null)
                logger.OnLogWarning += (logTag, logMessage) => Assert.AreEqual(message, logMessage);

            action();

            Assert.IsTrue(logger.LogWarningCalls > 0);
            logger.Clear();
            Log.Targets.Clear();
        }

        public static void LogError(Action action, string tag = null, string message = null)
        {
            SpyLogger logger = SpyLogger.GetDebugLogger();
            Log.AddLogger(logger);
            if (tag != null)
                logger.OnLogError += (logTag, logMessage) => Assert.AreEqual(tag, logTag);
            if (message != null)
                logger.OnLogError += (logTag, logMessage) => Assert.AreEqual(message, logMessage);

            action();

            Assert.IsTrue(logger.LogErrorCalls > 0);
            logger.Clear();
            Log.Targets.Clear();
        }

        public static void LogException<T>(Action action, string tag = null) where T : Exception
        {
            SpyLogger logger = SpyLogger.GetDebugLogger();
            Log.AddLogger(logger);
            if (tag != null)
                logger.OnLogException += (logTag, logMessage) => Assert.AreEqual(tag, logTag);
            logger.OnLogException += (logTag, logException) => Assert.IsInstanceOfType(logException, typeof(T));

            action();

            Assert.IsTrue(logger.LogExceptionCalls > 0);
            logger.Clear();
            Log.Targets.Clear();
        }
    }
}