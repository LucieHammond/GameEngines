using GameEngine.Core.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.Core.Tests
{
    /// <summary>
    /// Component tests for the UnityLogger class implementing ILogger interface
    /// <see cref="UnityLogger"/>
    /// </summary>
    public class UnityLoggerTest
    {
        private static UnityLogger m_Logger;

        private List<string> m_LogMessageList;
        private List<string> m_LogStackTraceList;
        private List<LogType> m_LogTypeList;
        private readonly Application.LogCallback m_LogCallback;

        public UnityLoggerTest()
        {
            m_Logger = new UnityLogger();
            m_LogMessageList = new List<string>();
            m_LogStackTraceList = new List<string>();
            m_LogTypeList = new List<LogType>();

            m_LogCallback = (logString, stackTrace, type) =>
            {
                m_LogMessageList.Add(logString);
                m_LogStackTraceList.Add(stackTrace);
                m_LogTypeList.Add(type);
            };
        }

        [SetUp]
        public void Initialize()
        {
            Application.logMessageReceived += m_LogCallback;
        }

        [TearDown]
        public void CleanUp()
        {
            Application.logMessageReceived -= m_LogCallback;
            m_LogMessageList.Clear();
            m_LogStackTraceList.Clear();
            m_LogTypeList.Clear();
        }

        [Test]
        public void ContainRelevantInformation()
        {
            string tag = "TestTag";

            // For a simple message -> log should be of the right type and contain the right message preceeded by the right tag
            string message = "A message to log";
            Regex expectedFormat = new Regex($@"\[.*{tag}.*\]\s+.*{message}.*");

            m_Logger.LogDebug(tag, message);
            LogAssert.Expect(LogType.Log, expectedFormat);

            m_Logger.LogInfo(tag, message);
            LogAssert.Expect(LogType.Log, expectedFormat);

            m_Logger.LogWarning(tag, message);
            LogAssert.Expect(LogType.Warning, expectedFormat);

            m_Logger.LogError(tag, message);
            LogAssert.Expect(LogType.Error, expectedFormat);

            // For an exception -> log should be of type Exception and contain all the information about the exception
            Exception exception;
            try { throw new InvalidOperationException("message of the exception", new ArgumentException("message of the inner exception")); }
            catch (Exception e) { exception = e; }
            expectedFormat = new Regex($@"{exception.InnerException.GetType().Name}:\s+\[.*{tag}.*\]\s+{exception.InnerException.Message}");

            m_Logger.LogException(tag, exception);
            LogAssert.Expect(LogType.Exception, expectedFormat);

            Assert.IsTrue(m_LogMessageList.Last().Contains(tag));
            Assert.IsTrue(m_LogMessageList.Last().Contains(exception.InnerException.GetType().Name));
            Assert.IsTrue(m_LogMessageList.Last().Contains(exception.InnerException.Message));
            Assert.IsTrue(m_LogStackTraceList.Last().Contains(exception.Message));
            Assert.IsTrue(m_LogStackTraceList.Last().Contains($"{exception.TargetSite.DeclaringType.FullName}.{exception.TargetSite.Name}"));
            Assert.AreEqual(LogType.Exception, m_LogTypeList.Last());

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void RespectOrderInMainThread()
        {
            string[] tags = new string[] { "Tag1", "Tag2", "Tag3", "Tag4" };
            string[] messages = new string[] { "first message", "second message", "third message", "fourth message" };
            LogType[] types = new LogType[] { LogType.Log, LogType.Log, LogType.Warning, LogType.Error };

            m_Logger.LogDebug(tags[0], messages[0]);
            m_Logger.LogInfo(tags[1], messages[1]);
            m_Logger.LogWarning(tags[2], messages[2]);
            m_Logger.LogError(tags[3], messages[3]);

            // Logs from main thread appear in order inside the console
            for (int i = 0; i < tags.Length; i++)
            {
                LogAssert.Expect(types[i], new Regex($@"\[.*{tags[i]}.*\]\s+.*{messages[i]}.*"));
            }
            LogAssert.NoUnexpectedReceived();

            // Displayed logs cause ordered calls of the callback method
            Assert.AreEqual(4, m_LogMessageList.Count);
            for (int i = 0; i < tags.Length; i++)
            {
                Assert.AreEqual(types[i], m_LogTypeList[i]);
                Assert.IsTrue(m_LogMessageList[i].Contains(tags[i]));
                Assert.IsTrue(m_LogMessageList[i].Contains(messages[i]));
            }
        }

        [Test]
        public void SupportMultithreadingCalls()
        {
            string tag = "Tag";
            string[] messages = new string[] { "message nb 1", "message nb 2", "message nb 3" };

            Task[] tasks = new Task[]
            {
                new Task(() => m_Logger.LogInfo(tag, messages[0])),
                new Task(() => m_Logger.LogInfo(tag, messages[1])),
                new Task(() => m_Logger.LogInfo(tag, messages[2])),
            };

            foreach (Task task in tasks)
                task.Start();
            Task.WaitAll(tasks);

            Regex commonFormat = new Regex(@"\[.*Tag.*\]\s+.*message nb {1,4}.*");
            for (int i = 0; i < messages.Length; i++)
            {
                LogAssert.Expect(LogType.Log, commonFormat);
            }
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ManageNullMessages()
        {
            string tag = "TestTag";

            // Messages can be null
            m_Logger.LogDebug(tag, null);
            m_Logger.LogInfo(tag, null);
            m_Logger.LogWarning(tag, null);
            m_Logger.LogError(tag, null);

            Assert.AreEqual(4, m_LogMessageList.Count);
            Regex emptyMessageFormat = new Regex($@"\[.*{tag}.*\]\s+");
            LogAssert.Expect(LogType.Log, emptyMessageFormat);
            LogAssert.Expect(LogType.Log, emptyMessageFormat);
            LogAssert.Expect(LogType.Warning, emptyMessageFormat);
            LogAssert.Expect(LogType.Error, emptyMessageFormat);
            LogAssert.NoUnexpectedReceived();

            // Exceptions can't be null -> thow NullReferenceException
            Assert.Throws<NullReferenceException>(() => m_Logger.LogException(tag, null));
        }
    }
}
