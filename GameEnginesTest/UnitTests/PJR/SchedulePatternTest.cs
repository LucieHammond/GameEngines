using GameEngine.PJR.Rules.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.UnitTests.PJR
{
    /// <summary>
    /// Unit tests for the class SchedulePattern
    /// <see cref="SchedulePattern"/>
    /// </summary>
    [TestClass]
    public class SchedulePatternTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // Create schedule pattern with coherent parameters (Offset < Frequency) -> schedule is defined with those parameters
            SchedulePattern pattern1 = new SchedulePattern(75, 36);
            Assert.AreEqual(75, pattern1.Frequency);
            Assert.AreEqual(36, pattern1.Offset);

            // Create schedule pattern with Offset >= Frequency -> Offset is simplified to Offset modulo Frequency
            SchedulePattern pattern2 = new SchedulePattern(3, 13);
            Assert.AreEqual(3, pattern2.Frequency);
            Assert.AreEqual(1, pattern2.Offset); // 3 % 13 = 1

            // Create default schedule pattern -> Frequency = 1 and Offset = 0
            SchedulePattern pattern3 = new SchedulePattern();
            Assert.AreEqual(1, pattern3.Frequency);
            Assert.AreEqual(0, pattern3.Offset);
        }

        [TestMethod]
        public void IsFrameIncludedTest()
        {
            // Create 3 schedule patterns
            SchedulePattern defaultPattern = SchedulePattern.Default;
            SchedulePattern oddPattern = new SchedulePattern(2, 1);
            SchedulePattern neverPattern = new SchedulePattern(0, 0);

            // When frame = 0 -> true for default, false for odd, false for never
            Assert.IsTrue(defaultPattern.IsFrameIncluded(0));
            Assert.IsFalse(oddPattern.IsFrameIncluded(0));
            Assert.IsFalse(neverPattern.IsFrameIncluded(0));

            // When frame = 1 -> true for default, true for odd, false for never
            Assert.IsTrue(defaultPattern.IsFrameIncluded(1));
            Assert.IsTrue(oddPattern.IsFrameIncluded(1));
            Assert.IsFalse(neverPattern.IsFrameIncluded(1));

            // When frame = 125664785 -> true for default, true for odd, false for never
            Assert.IsTrue(defaultPattern.IsFrameIncluded(125664785));
            Assert.IsTrue(oddPattern.IsFrameIncluded(125664785));
            Assert.IsFalse(neverPattern.IsFrameIncluded(125664785));

            // When frame is negative -> false for all
            Assert.IsFalse(defaultPattern.IsFrameIncluded(-1));
            Assert.IsFalse(oddPattern.IsFrameIncluded(-1));
            Assert.IsFalse(neverPattern.IsFrameIncluded(-1));
        }
    }
}
