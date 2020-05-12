using GameEngine.Core.Logger.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class FileLogger
    /// <see cref="FileLogger"/>
    /// </summary>
    [TestClass]
    public class FileLoggerTest : BaseLoggerTest
    {
        private string m_FilePath;

        public FileLoggerTest()
        {
            m_FilePath = Path.Combine(Environment.CurrentDirectory, "TestLogFile.txt");
            m_Logger = new FileLogger(m_FilePath);
        }

        protected override string GetLogsAsString()
        {
            return File.ReadAllText(m_FilePath);
        }

        protected override void ResetLogs()
        {
            if (File.Exists(m_FilePath))
            {
                File.Delete(m_FilePath);
            }
        }
    }
}
