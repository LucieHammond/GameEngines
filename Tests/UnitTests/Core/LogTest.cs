#define ENABLE_LOGS
using GameEngine.Core.Logger;
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
        DummyLogger m_Logger;
        private object m_LogLock = new object();

        public LogTest()
        {
            m_Logger = new DummyLogger();
            Log.Logger = m_Logger;
        }

        [TestMethod]
        public void DebugTest()
        {
            string tag = "Test";
            string message = "debug message";
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogDebug += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With MinLevel > Debug -> LogDebug is not called
                Log.MinLevel = LogLevel.Info;
                Log.Debug(tag, message);
                Log.MinLevel = LogLevel.Warning;
                Log.Debug(tag, message);
                Log.MinLevel = LogLevel.Error;
                Log.Debug(tag, message);
                Log.MinLevel = LogLevel.Fatal;
                Log.Debug(tag, message);
                Assert.AreEqual(0, totalCalls);

                // With MinLevel = Debug -> LogDebug is called
                Log.MinLevel = LogLevel.Debug;
                Log.Debug(tag, message);
                Assert.AreEqual(1, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogDebug is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Debug(tag, message);
                Assert.AreEqual(1, totalCalls);

                // TagsFilter is set and contains tag -> LogDebug is called
                Log.TagsFilter.Add(tag);
                Log.Debug(tag, message);
                Assert.AreEqual(2, totalCalls);

                // Works with a formatted message
                Log.Debug(tag, "{0} message", "debug");
                Assert.AreEqual(3, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Debug(null, message));
            }
        }

        [TestMethod]
        public void InfoTest()
        {
            string tag = "Test";
            string message = "info message";
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogInfo += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With MinLevel > Info -> LogInfo is not called
                Log.MinLevel = LogLevel.Warning;
                Log.Info(tag, message);
                Log.MinLevel = LogLevel.Error;
                Log.Info(tag, message);
                Log.MinLevel = LogLevel.Fatal;
                Log.Info(tag, message);
                Assert.AreEqual(0, totalCalls);

                // With MinLevel <= Info -> LogInfo is called
                Log.MinLevel = LogLevel.Debug;
                Log.Info(tag, message);
                Log.MinLevel = LogLevel.Info;
                Log.Info(tag, message);
                Assert.AreEqual(2, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogInfo is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Info(tag, message);
                Assert.AreEqual(2, totalCalls);

                // TagsFilter is set and contains tag -> LogInfo is called
                Log.TagsFilter.Add(tag);
                Log.Info(tag, message);
                Assert.AreEqual(3, totalCalls);

                // Works with a formatted message
                Log.Info(tag, "{0} message", "info");
                Assert.AreEqual(4, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Info(null, message));
            }
        }

        [TestMethod]
        public void WarningTest()
        {
            string tag = "Test";
            string message = "warning message";
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogWarning += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With MinLevel > Warning -> LogWarning is not called
                Log.MinLevel = LogLevel.Error;
                Log.Warning(tag, message);
                Log.MinLevel = LogLevel.Fatal;
                Log.Warning(tag, message);
                Assert.AreEqual(0, totalCalls);

                // With MinLevel <= Warning -> LogWarning is called
                Log.MinLevel = LogLevel.Debug;
                Log.Warning(tag, message);
                Log.MinLevel = LogLevel.Info;
                Log.Warning(tag, message);
                Log.MinLevel = LogLevel.Warning;
                Log.Warning(tag, message);
                Assert.AreEqual(3, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogWarning is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Warning(tag, message);
                Assert.AreEqual(3, totalCalls);

                // TagsFilter is set and contains tag -> LogWarning is called
                Log.TagsFilter.Add(tag);
                Log.Warning(tag, message);
                Assert.AreEqual(4, totalCalls);

                // Works with a formatted message
                Log.Warning(tag, "{0} message", "warning");
                Assert.AreEqual(5, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Warning(null, message));
            }
        }

        [TestMethod]
        public void ErrorTest()
        {
            string tag = "Test";
            string message = "error message";
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogError += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With MinLevel > Error -> LogError is not called
                Log.MinLevel = LogLevel.Fatal;
                Log.Error(tag, message);
                Assert.AreEqual(0, totalCalls);

                // With MinLevel <= Error -> LogError is called
                Log.MinLevel = LogLevel.Debug;
                Log.Error(tag, message);
                Log.MinLevel = LogLevel.Info;
                Log.Error(tag, message);
                Log.MinLevel = LogLevel.Warning;
                Log.Error(tag, message);
                Log.MinLevel = LogLevel.Error;
                Log.Error(tag, message);
                Assert.AreEqual(4, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogError is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Error(tag, message);
                Assert.AreEqual(4, totalCalls);

                // TagsFilter is set and contains tag -> LogError is called
                Log.TagsFilter.Add(tag);
                Log.Error(tag, message);
                Assert.AreEqual(5, totalCalls);

                // Works with a formatted message
                Log.Error(tag, "{0} message", "error");
                Assert.AreEqual(6, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Error(null, message));
            }
        }

        [TestMethod]
        public void ExceptionTest()
        {
            string tag = "Test";
            Exception exception = new Exception("exception message");
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogException += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With MinLevel = Fatal -> LogException is not called
                Log.MinLevel = LogLevel.Fatal;
                Log.Exception(tag, exception);
                Assert.AreEqual(0, totalCalls);

                // With any level -> LogException is called
                Log.MinLevel = LogLevel.Debug;
                Log.Exception(tag, exception);
                Log.MinLevel = LogLevel.Info;
                Log.Exception(tag, exception);
                Log.MinLevel = LogLevel.Warning;
                Log.Exception(tag, exception);
                Log.MinLevel = LogLevel.Error;
                Log.Exception(tag, exception);
                Assert.AreEqual(4, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogException is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Exception(tag, exception);
                Assert.AreEqual(4, totalCalls);

                // TagsFilter is set and contains tag -> LogException is called
                Log.TagsFilter.Add(tag);
                Log.Exception(tag, exception);
                Assert.AreEqual(5, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Exception(null, exception));
            }
        }

        [TestMethod]
        public void FatalTest()
        {
            string tag = "Test";
            string message = "fatal message";
            int totalCalls = 0;

            lock (m_LogLock)
            {
                m_Logger.OnLogError += (t, m) => totalCalls++;
                Log.TagsFilter = null;

                // With any level -> LogError is called
                Log.MinLevel = LogLevel.Debug;
                Log.Fatal(tag, message);
                Log.MinLevel = LogLevel.Info;
                Log.Fatal(tag, message);
                Log.MinLevel = LogLevel.Warning;
                Log.Fatal(tag, message);
                Log.MinLevel = LogLevel.Error;
                Log.Fatal(tag, message);
                Log.MinLevel = LogLevel.Fatal;
                Log.Fatal(tag, message);
                Assert.AreEqual(5, totalCalls);

                // TagsFilter is set but doesn't contain tag -> LogError is not called
                Log.TagsFilter = new HashSet<string>() { "OtherTag" };
                Log.Fatal(tag, message);
                Assert.AreEqual(5, totalCalls);

                // TagsFilter is set and contains tag -> LogError is called
                Log.TagsFilter.Add(tag);
                Log.Fatal(tag, message);
                Assert.AreEqual(6, totalCalls);

                // Works with a formatted message
                Log.Fatal(tag, "{0} message", "fatal");
                Assert.AreEqual(7, totalCalls);

                // Tag is null -> throw ArgumentException
                Assert.ThrowsException<ArgumentException>(() => Log.Fatal(null, message));
            }
        }

        private class DummyLogger : ILogger
        {
            public Action<string, string> OnLogDebug;
            public Action<string, string> OnLogInfo;
            public Action<string, string> OnLogWarning;
            public Action<string, string> OnLogError;
            public Action<string, Exception> OnLogException;

            public void LogDebug(string tag, string message)
            {
                OnLogDebug?.Invoke(tag, message);
            }

            public void LogInfo(string tag, string message)
            {
                OnLogInfo?.Invoke(tag, message);
            }

            public void LogWarning(string tag, string message)
            {
                OnLogWarning?.Invoke(tag, message);
            }

            public void LogError(string tag, string message)
            {
                OnLogError?.Invoke(tag, message);
            }

            public void LogException(string tag, Exception e)
            {
                OnLogException?.Invoke(tag, e);
            }
        }
    }
}
