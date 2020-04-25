using GameEngine.PSMR.Rules;
using GameEngine.PSMR.Rules.Scheduling;
using GameEnginesTest.Tools.Dummy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.UnitTests.PSMR
{
    /// <summary>
    /// Unit tests for the class RulesDictionary
    /// <see cref="RulesDictionary"/>
    /// </summary>
    [TestClass]
    public class RulesDictionaryTest
    {
        [TestMethod]
        public void AddRuleTest()
        {
            // Create empty dictionary
            RulesDictionary dictionary = new RulesDictionary();
            Assert.AreEqual(0, dictionary.Count);

            // Add a rule to the dictionary -> the rule is visible and retrievable
            DummyGameRule rule1 = new DummyGameRule();
            dictionary.AddRule(rule1);
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(rule1, dictionary[rule1.GetType()]);

            // Try to add another rule of the same type -> throw ArgumentException
            DummyGameRule rule2 = new DummyGameRule();
            Assert.ThrowsException<ArgumentException>(() => dictionary.AddRule(rule2));

            // Try to add another rule of different type -> both rules are visible
            DummyGameRuleBis rule3 = new DummyGameRuleBis();
            dictionary.AddRule(rule3);
            Assert.AreEqual(2, dictionary.Count);
        }

        [TestMethod]
        public void AddRulePackTest()
        {
            // Create empty dictionary
            RulesDictionary dictionary = new RulesDictionary();
            Assert.AreEqual(0, dictionary.Count);

            // Add a rule pack to the dictionary -> all the rules created by the pack in GetRules() can be found in the dictionary
            DummyGameRulesPack rulePack = new DummyGameRulesPack();
            Assert.IsNull(rulePack.CreatedRules);
            dictionary.AddRulePack(rulePack);
            Assert.IsNotNull(rulePack.CreatedRules);
            Assert.AreEqual(rulePack.CreatedRules.Count, dictionary.Count);
            foreach (GameRule rule in rulePack.CreatedRules)
                Assert.AreEqual(rule, dictionary[rule.GetType()]);

            // Try to add the same pack again -> throw ArgumentException
            Assert.ThrowsException<ArgumentException>(() => dictionary.AddRulePack(rulePack));
        }

        [TestMethod]
        public void GetRulesInOrderTest()
        {
            // Create a dictionary with 2 rules
            RulesDictionary dictionary = new RulesDictionary();
            DummyGameRule rule1 = new DummyGameRule();
            DummyGameRuleBis rule2 = new DummyGameRuleBis();
            dictionary.AddRule(rule1);
            dictionary.AddRule(rule2);

            // Call GetRulesInOrder with a correct ordered list of GameRule types -> return an IEnumerable visiting the corresponding rules in that same order
            List<Type> correctOrder = new List<Type> { typeof(DummyGameRule), typeof(DummyGameRuleBis), typeof(DummyGameRule) };
            IEnumerator<GameRule> resultEnumerator = dictionary.GetRulesInOrder(correctOrder).GetEnumerator();
            resultEnumerator.MoveNext();
            Assert.AreEqual(rule1, resultEnumerator.Current);
            resultEnumerator.MoveNext();
            Assert.AreEqual(rule2, resultEnumerator.Current);
            resultEnumerator.MoveNext();
            Assert.AreEqual(rule1, resultEnumerator.Current);
            Assert.IsFalse(resultEnumerator.MoveNext());

            // The order parameter contains a GameRule type that is not present in the dictionary -> throw KeyNotFoundException when reaching the missing rule
            List<Type> missingRuleOrder = new List<Type> { typeof(DummyGameRuleTer) };
            IEnumerable<GameRule> result2 = dictionary.GetRulesInOrder(missingRuleOrder);
            Assert.ThrowsException<KeyNotFoundException>(() => result2.GetEnumerator().MoveNext());

            // The order parameter contains non GameRules types -> throw KeyNotFoundException when reaching the incorrect type
            List<Type> nonRuleOrder = new List<Type> { typeof(string) };
            IEnumerable<GameRule> result3 = dictionary.GetRulesInOrder(nonRuleOrder);
            Assert.ThrowsException<KeyNotFoundException>(() => result3.GetEnumerator().MoveNext());
        }

        [TestMethod]
        public void GetRulesInReverseOrderTest()
        {
            // Create a dictionary with 2 rules
            RulesDictionary dictionary = new RulesDictionary();
            DummyGameRule rule1 = new DummyGameRule();
            DummyGameRuleBis rule2 = new DummyGameRuleBis();
            dictionary.AddRule(rule1);
            dictionary.AddRule(rule2);

            // Call GetRulesInOrder with a correct ordered list of GameRule types -> return an IEnumerable visiting the corresponding rules in the reverse order
            List<Type> correctOrder = new List<Type> { typeof(DummyGameRule), typeof(DummyGameRuleBis) };
            IEnumerator<GameRule> resultEnumerator = dictionary.GetRulesInReverseOrder(correctOrder).GetEnumerator();
            resultEnumerator.MoveNext();
            Assert.AreEqual(rule2, resultEnumerator.Current);
            resultEnumerator.MoveNext();
            Assert.AreEqual(rule1, resultEnumerator.Current);
            Assert.IsFalse(resultEnumerator.MoveNext());

            // The order parameter contains a GameRule type that is not present in the dictionary -> throw KeyNotFoundException when reaching the missing rule
            List<Type> missingRuleOrder = new List<Type> { typeof(DummyGameRuleTer) };
            IEnumerable<GameRule> result2 = dictionary.GetRulesInReverseOrder(missingRuleOrder);
            Assert.ThrowsException<KeyNotFoundException>(() => result2.GetEnumerator().MoveNext());

            // The order parameter contains a null type -> throw ArgumentNullException when reaching the incorrect type
            List<Type> orderWithNull = new List<Type> { null };
            IEnumerable<GameRule> result3 = dictionary.GetRulesInReverseOrder(orderWithNull);
            Assert.ThrowsException<ArgumentNullException>(() => result3.GetEnumerator().MoveNext());
        }

        [TestMethod]
        public void GetRulesInOrderForFrameTest()
        {
            // Create dictionary with 2 rules
            RulesDictionary dictionary = new RulesDictionary();
            DummyGameRule rule1 = new DummyGameRule();
            DummyGameRuleBis rule2 = new DummyGameRuleBis();
            dictionary.AddRule(rule1);
            dictionary.AddRule(rule2);

            // Create the following test schedule :
            List<RuleScheduling> scheduling = new List<RuleScheduling>()
            {
                new RuleScheduling(rule2.GetType(), 2, 1),                      // rule2 on second frame every 2 frames
                new RuleScheduling(rule1.GetType(), SchedulePattern.Default),   // rule1 every frame
            };

            // At frame 0 for this schedule -> returns : rule1
            IEnumerator<GameRule> frame0 = dictionary.GetRulesInOrderForFrame(scheduling, 0).GetEnumerator();
            frame0.MoveNext();
            Assert.AreEqual(rule1, frame0.Current);
            Assert.IsFalse(frame0.MoveNext());

            // At frame 1 for this schedule -> returns : rule2, rule1
            IEnumerator<GameRule> frame1 = dictionary.GetRulesInOrderForFrame(scheduling, 1).GetEnumerator();
            frame1.MoveNext();
            Assert.AreEqual(rule2, frame1.Current);
            frame1.MoveNext();
            Assert.AreEqual(rule1, frame1.Current);
            Assert.IsFalse(frame1.MoveNext());

            // With negative frame -> returns an enumerable with no rules
            IEnumerator<GameRule> negativeFrame = dictionary.GetRulesInOrderForFrame(scheduling, -1).GetEnumerator();
            Assert.IsFalse(negativeFrame.MoveNext());

            // With a scheduling containing a rule that is not present in dictionary -> throw KeyNotFoundException when reaching the missing rule
            List<RuleScheduling> invalidScheduling = new List<RuleScheduling>()
            {
                new RuleScheduling(rule1.GetType(), SchedulePattern.Default),
                new RuleScheduling(typeof(DummyGameRuleTer), 2, 0) // invalid rule
            };
            IEnumerator<GameRule> invalidFrame = dictionary.GetRulesInOrderForFrame(invalidScheduling, 10).GetEnumerator();
            Assert.IsTrue(invalidFrame.MoveNext());
            Assert.ThrowsException<KeyNotFoundException>(() => invalidFrame.MoveNext());
        }
    }
}
