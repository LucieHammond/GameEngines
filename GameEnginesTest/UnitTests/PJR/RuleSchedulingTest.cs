using GameEngine.PJR.Rules.Scheduling;
using GameEnginesTest.Tools.Dummy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.UnitTests.PJR
{
    /// <summary>
    /// Unit tests for the class RuleScheduling
    /// <see cref="RuleScheduling"/>
    /// </summary>
    [TestClass]
    public class RuleSchedulingTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // Create RuleScheduling with valid rule type and predefined SchedulePattern -> given parameters are correctly retrievable
            SchedulePattern evenPattern = new SchedulePattern(2, 0);
            RuleScheduling ruleScheduling1 = new RuleScheduling(typeof(DummyGameRule), evenPattern);
            Assert.AreEqual(typeof(DummyGameRule), ruleScheduling1.RuleType);
            Assert.AreEqual(evenPattern, ruleScheduling1.Pattern);

            // Create RuleScheduling with valid rule type, Frequency and Offset -> given parameters are correctly retrievable
            RuleScheduling ruleScheduling2 = new RuleScheduling(typeof(DummyGameRuleBis), 4, 4);
            Assert.AreEqual(typeof(DummyGameRuleBis), ruleScheduling2.RuleType);
            Assert.AreEqual(4, ruleScheduling2.Pattern.Frequency);
            Assert.AreEqual(0, ruleScheduling2.Pattern.Offset); // Offset has been simplified to 4 % 4 = 0

            // Try to create RuleScheduling with incorrect rule type -> throws ArgumentException
            Assert.ThrowsException<ArgumentException>(() => new RuleScheduling(typeof(RuleScheduling), 3, 0));
        }

        [TestMethod]
        public void IsExpectedAtFrameTest()
        {
            // Create 2 scheduling rules
            RuleScheduling everyFrame = new RuleScheduling(typeof(DummyGameRule), 1, 0);
            RuleScheduling multipleOfThree = new RuleScheduling(typeof(DummyGameRuleTer), 3, 0);

            // When frame = 0 -> true for everyFrame, true for multipleOfThree
            Assert.IsTrue(everyFrame.IsExpectedAtFrame(0));
            Assert.IsTrue(multipleOfThree.IsExpectedAtFrame(0));

            // When frame = 10 -> true for everyFrame, false for multipleOfThree
            Assert.IsTrue(everyFrame.IsExpectedAtFrame(10));
            Assert.IsFalse(multipleOfThree.IsExpectedAtFrame(10));

            // When frame = 9999999 -> true for everyFrame, true for multipleOfThree
            Assert.IsTrue(everyFrame.IsExpectedAtFrame(9999999));
            Assert.IsTrue(multipleOfThree.IsExpectedAtFrame(9999999));

            // When frame is negative -> false for all
            Assert.IsFalse(everyFrame.IsExpectedAtFrame(-1));
            Assert.IsFalse(multipleOfThree.IsExpectedAtFrame(-1));
        }
    }
}
