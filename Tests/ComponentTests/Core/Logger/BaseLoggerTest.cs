using GameEngine.Core.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the base loggers implementing ILogger interface
    /// <see cref="ILogger"/>
    /// </summary>
    [TestClass]
    public abstract class BaseLoggerTest
    {
        protected ILogger m_Logger;

        protected abstract string GetLogsAsString();

        protected abstract void ResetLogs();

        [TestCleanup]
        public void Cleanup()
        {
            ResetLogs();
        }

        [TestMethod]
        public void LogsContainRelevantInformation()
        {
            string tag = "TestTag";

            // For a warning message
            string message = "test warning to log";
            m_Logger.LogWarning(tag, message);
            string outputLogs = GetLogsAsString();
            string[] outputLines = outputLogs.Split("\n").Where((line) => line != "").ToArray();

            Assert.AreEqual(1, outputLines.Length);
            Assert.IsTrue(outputLogs.Contains(tag));
            Assert.IsTrue(outputLogs.Contains(message));
            Assert.IsTrue(outputLogs.ToLower().Contains("warning"));

            // For an exception
            try { throw new InvalidOperationException("exception message", new ArgumentException("inner exception message")); }
            catch (Exception exception)
            {
                ResetLogs();
                m_Logger.LogException(tag, exception);
                string exceptionLogs = GetLogsAsString();

                Assert.IsTrue(exceptionLogs.Contains(exception.GetType().Name));
                Assert.IsTrue(exceptionLogs.Contains(exception.Message));
                Assert.IsTrue(exceptionLogs.Contains(exception.StackTrace));
                Assert.IsTrue(exceptionLogs.Contains(exception.InnerException.GetType().Name));
                Assert.IsTrue(exceptionLogs.Contains(exception.InnerException.Message));
                Assert.IsTrue(exceptionLogs.ToLower().Contains("error"));
            }
        }

        [TestMethod]
        public void CanHandleSequentialCalls()
        {
            string[] tags = new string[] { "Tag1", "Tag2", "Tag3", "Tag4" };
            string[] messages = new string[] { "first message", "second message", "third message", "fourth message" };

            m_Logger.LogDebug(tags[0], messages[0]);
            m_Logger.LogInfo(tags[1], messages[1]);
            m_Logger.LogWarning(tags[2], messages[2]);
            m_Logger.LogError(tags[3], messages[3]);
            string outputLogs = GetLogsAsString();
            string[] outputLines = outputLogs.Split("\n").Where((line) => line != "").ToArray();

            Assert.AreEqual(4, outputLines.Length);
            for (int i = 0; i < tags.Length; i++)
            {
                Assert.IsTrue(outputLines[i].Contains(tags[i]));
                Assert.IsTrue(outputLines[i].Contains(messages[i]));
            }
        }

        [TestMethod]
        public void CanHandleConcurrentCalls()
        {
            string[] tags = new string[] { "Tag1", "Tag2", "Tag3", "Tag4" };
            string[] messages = new string[] { "first message", "second message", "third message", "fourth message" };

            Task[] tasks = new Task[]
            {
                new Task(() => m_Logger.LogDebug(tags[0], messages[0])),
                new Task(() => m_Logger.LogInfo(tags[1], messages[1])),
                new Task(() => m_Logger.LogWarning(tags[2], messages[2])),
                new Task(() => m_Logger.LogError(tags[3], messages[3]))
            };
            foreach (Task task in tasks)
                task.Start();
            Task.WaitAll(tasks);

            string outputLogs = GetLogsAsString();
            string[] outputLines = outputLogs.Split("\n").Where((line) => line != "").ToArray();

            Assert.AreEqual(4, outputLines.Length);
            foreach (string line in outputLines)
            {
                int i;
                if (line.ToLower().Contains("debug")) i = 0;
                else if (line.ToLower().Contains("info")) i = 1;
                else if (line.ToLower().Contains("warning")) i = 2;
                else if (line.ToLower().Contains("error")) i = 3;
                else
                {
                    i = -1;
                    Assert.Fail();
                }

                Assert.IsTrue(line.Contains(tags[i]));
                Assert.IsTrue(line.Contains(messages[i]));
            }
        }
    }
}
