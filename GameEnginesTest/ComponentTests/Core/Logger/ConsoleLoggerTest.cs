using GameEngine.Core.Logger.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class ConsoleLogger
    /// <see cref="ConsoleLogger"/>
    /// </summary>
    [TestClass]
    public class ConsoleLoggerTest : BaseLoggerTest
    {
        private StringWriter m_ConsoleOutput;

        public ConsoleLoggerTest()
        {
            m_Logger = new ConsoleLogger();
            m_ConsoleOutput = new StringWriter();
            Console.SetOut(m_ConsoleOutput);
        }

        protected override string GetLogsAsString()
        {
            return m_ConsoleOutput.ToString();
        }

        protected override void ResetLogs()
        {
            m_ConsoleOutput.Flush();
        }
    }
}
