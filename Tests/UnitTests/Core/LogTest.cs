#define ENABLE_LOGS
using GameEngine.Core.Logger;
using GameEnginesTest.Tools.Mocks.Spies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the static class Log
    /// <see cref="Log"/>
    /// </summary>
    [TestClass]
    public class LogTest
    {
        private readonly SpyLogger m_DebugLogger;
        private readonly SpyLogger m_InfoLogger;
        private readonly SpyLogger m_WarningLogger;
        private readonly SpyLogger m_ErrorLogger;
        private readonly SpyLogger m_FatalLogger;
        private readonly SpyLogger m_TestTagLogger;
        private readonly SpyLogger m_OtherTagsLogger;

        private readonly string m_TestTag = "Test";
        private readonly object m_LogLock = new object();

        public LogTest()
        {
            m_DebugLogger = new SpyLogger();
            m_InfoLogger = new SpyLogger();
            m_WarningLogger = new SpyLogger();
            m_ErrorLogger = new SpyLogger();
            m_FatalLogger = new SpyLogger();
            m_TestTagLogger = new SpyLogger();
            m_OtherTagsLogger = new SpyLogger();
        }

        [TestInitialize]
        public void Initialize()
        {
            Log.AddLogger(m_DebugLogger, LogLevel.Debug);
            Log.AddLogger(m_InfoLogger, LogLevel.Info);
            Log.AddLogger(m_WarningLogger, LogLevel.Warning);
            Log.AddLogger(m_ErrorLogger, LogLevel.Error);
            Log.AddLogger(m_FatalLogger, LogLevel.Fatal);
            Log.AddLogger(m_TestTagLogger, LogLevel.Debug, new HashSet<string>() { m_TestTag });
            Log.AddLogger(m_OtherTagsLogger, LogLevel.Debug, new HashSet<string>() { "OtherTag1", "OtherTag2", "OtherTag3" });
        }

        [TestMethod]
        public void DebugTest()
        {
            string message = "debug message";

            lock (m_LogLock)
            {
                Log.Debug(m_TestTag, message);

                // For loggers with MinLevel = Debug -> LogDebug is called
                Assert.AreEqual(1, m_DebugLogger.LogDebugCalls);

                // For loggers with MinLevel > Debug -> LogDebug is not called
                Assert.AreEqual(0, m_InfoLogger.LogDebugCalls);
                Assert.AreEqual(0, m_WarningLogger.LogDebugCalls);
                Assert.AreEqual(0, m_ErrorLogger.LogDebugCalls);
                Assert.AreEqual(0, m_FatalLogger.LogDebugCalls);

                // For loggers filtering on test tag -> LogDebug is called
                Assert.AreEqual(1, m_TestTagLogger.LogDebugCalls);

                // For loggers filtering on other tags but not test tag -> LogDebug is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogDebugCalls);

                // The same Works with a formatted message
                Log.Debug(m_TestTag, "{0} message", "debug");
                Assert.AreEqual(2, m_DebugLogger.LogDebugCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Debug(null, message));
            }
        }

        [TestMethod]
        public void InfoTest()
        {
            string message = "info message";

            lock (m_LogLock)
            {
                Log.Info(m_TestTag, message);

                // For loggers with MinLevel <= Info -> LogInfo is called
                Assert.AreEqual(1, m_DebugLogger.LogInfoCalls);
                Assert.AreEqual(1, m_InfoLogger.LogInfoCalls);

                // For loggers with MinLevel > Info -> LogInfo is not called
                Assert.AreEqual(0, m_WarningLogger.LogInfoCalls);
                Assert.AreEqual(0, m_ErrorLogger.LogInfoCalls);
                Assert.AreEqual(0, m_FatalLogger.LogInfoCalls);

                // For loggers filtering on test tag -> LogInfo is called
                Assert.AreEqual(1, m_TestTagLogger.LogInfoCalls);

                // For loggers filtering on other tags but not test tag -> LogInfo is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogInfoCalls);

                // The same Works with a formatted message
                Log.Info(m_TestTag, "{0} message", "info");
                Assert.AreEqual(2, m_InfoLogger.LogInfoCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Info(null, message));
            }
        }

        [TestMethod]
        public void WarningTest()
        {
            string message = "warning message";

            lock (m_LogLock)
            {
                Log.Warning(m_TestTag, message);

                // For loggers with MinLevel <= Warning -> LogWarning is called
                Assert.AreEqual(1, m_DebugLogger.LogWarningCalls);
                Assert.AreEqual(1, m_InfoLogger.LogWarningCalls);
                Assert.AreEqual(1, m_WarningLogger.LogWarningCalls);

                // For loggers with MinLevel > Warning -> LogWarning is not called
                Assert.AreEqual(0, m_ErrorLogger.LogWarningCalls);
                Assert.AreEqual(0, m_FatalLogger.LogWarningCalls);

                // For loggers filtering on test tag -> LogWarning is called
                Assert.AreEqual(1, m_TestTagLogger.LogWarningCalls);

                // For loggers filtering on other tags but not test tag -> LogWarning is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogWarningCalls);

                // The same Works with a formatted message
                Log.Warning(m_TestTag, "{0} message", "warning");
                Assert.AreEqual(2, m_WarningLogger.LogWarningCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Warning(null, message));
            }
        }

        [TestMethod]
        public void ErrorTest()
        {
            string message = "error message";

            lock (m_LogLock)
            {
                Log.Error(m_TestTag, message);

                // For loggers with MinLevel <= Error -> LogError is called
                Assert.AreEqual(1, m_DebugLogger.LogErrorCalls);
                Assert.AreEqual(1, m_InfoLogger.LogErrorCalls);
                Assert.AreEqual(1, m_WarningLogger.LogErrorCalls);
                Assert.AreEqual(1, m_ErrorLogger.LogErrorCalls);

                // For loggers with MinLevel > Error -> LogError is not called
                Assert.AreEqual(0, m_FatalLogger.LogErrorCalls);

                // For loggers filtering on test tag -> LogError is called
                Assert.AreEqual(1, m_TestTagLogger.LogErrorCalls);

                // For loggers filtering on other tags but not test tag -> LogError is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogErrorCalls);

                // The same Works with a formatted message
                Log.Error(m_TestTag, "{0} message", "error");
                Assert.AreEqual(2, m_ErrorLogger.LogErrorCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Error(null, message));
            }
        }

        [TestMethod]
        public void ExceptionTest()
        {
            Exception exception = new Exception("exception message");

            lock (m_LogLock)
            {
                Log.Exception(m_TestTag, exception);

                // For loggers with MinLevel <= Error -> LogException is called
                Assert.AreEqual(1, m_DebugLogger.LogExceptionCalls);
                Assert.AreEqual(1, m_InfoLogger.LogExceptionCalls);
                Assert.AreEqual(1, m_WarningLogger.LogExceptionCalls);
                Assert.AreEqual(1, m_ErrorLogger.LogExceptionCalls);

                // For loggers with MinLevel > Error -> LogException is not called
                Assert.AreEqual(0, m_FatalLogger.LogExceptionCalls);

                // For loggers filtering on test tag -> LogException is called
                Assert.AreEqual(1, m_TestTagLogger.LogExceptionCalls);

                // For loggers filtering on other tags but not test tag -> LogException is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogExceptionCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Exception(null, exception));
            }
        }

        [TestMethod]
        public void FatalTest()
        {
            string message = "fatal message";

            lock (m_LogLock)
            {
                Log.Fatal(m_TestTag, message);

                // For loggers with any MinLevel -> LogError is called
                Assert.AreEqual(1, m_DebugLogger.LogErrorCalls);
                Assert.AreEqual(1, m_InfoLogger.LogErrorCalls);
                Assert.AreEqual(1, m_WarningLogger.LogErrorCalls);
                Assert.AreEqual(1, m_ErrorLogger.LogErrorCalls);
                Assert.AreEqual(1, m_FatalLogger.LogErrorCalls);

                // For loggers filtering on test tag -> LogError is called
                Assert.AreEqual(1, m_TestTagLogger.LogErrorCalls);

                // For loggers filtering on other tags but not test tag -> LogError is not called
                Assert.AreEqual(0, m_OtherTagsLogger.LogErrorCalls);

                // The same Works with a formatted message
                Log.Fatal(m_TestTag, "{0} message", "fatal");
                Assert.AreEqual(2, m_FatalLogger.LogErrorCalls);

                // If tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Fatal(null, message));
            }
        }
    }
}
