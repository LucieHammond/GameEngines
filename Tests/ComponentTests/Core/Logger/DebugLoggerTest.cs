using GameEngine.Core.Logger.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class DebugLogger
    /// <see cref="DebugLogger"/>
    /// </summary>
    [TestClass]
    public class DebugLoggerTest : BaseLoggerTest
    {
        private StringWriter m_DebugOutput;

        public DebugLoggerTest()
        {
            m_Logger = new DebugLogger();
            m_DebugOutput = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(m_DebugOutput));
        }

        protected override string GetLogsAsString()
        {
            return m_DebugOutput.ToString();
        }

        protected override void ResetLogs()
        {
            m_DebugOutput.Flush();
        }
    }
}
