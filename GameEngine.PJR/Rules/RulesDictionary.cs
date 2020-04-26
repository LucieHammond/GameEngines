using GameEngine.PJR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEngine.PJR.Rules
{
    /// <summary>
    /// A dictionary structure for storing GameRules. 
    /// The dictionary can contain only one rule instance per type of rule and this instance can be retrieved given its type
    /// </summary>
    public class RulesDictionary : Dictionary<Type, GameRule>
    {
        /// <summary>
        /// Add a game rule of type T to the dictionary
        /// </summary>
        /// <typeparam name="T">The type of the rule</typeparam>
        /// <param name="rule">The rule object</param>
        /// <exception cref="ArgumentException">Thrown when the dictionary already contains a rule of type T</exception>
        public void AddRule<T>(T rule) where T : GameRule
        {
            Type ruleType = rule.GetType();
            if (this.ContainsKey(ruleType))
                throw new ArgumentException($"Rules dictionary already contains a rule of type {ruleType.Name}");

            this.Add(ruleType, rule);
        }

        /// <summary>
        /// Add all the rules defined in a rule pack to the dictionary
        /// </summary>
        /// <param name="pack">The rule pack</param>
        /// <exception cref="ArgumentException">Thrown is case one of the rules provokes a type collision with some previously added rules</exception>
        public void AddRulePack(IGameRulePack pack)
        {
            foreach (GameRule rule in pack.GetRules())
            {
                AddRule(rule);
            }
        }

        internal IEnumerable<GameRule> GetRulesInOrder(List<Type> order)
        {
            for (int i = 0; i < order.Count; i++)
            {
                yield return this[order[i]];
            }
        }

        internal IEnumerable<GameRule> GetRulesInReverseOrder(List<Type> order)
        {
            for (int i = order.Count - 1; i >= 0; i--)
            {
                yield return this[order[i]];
            }
        }

        internal IEnumerable<GameRule> GetRulesInOrderForFrame(List<RuleScheduling> scheduling, int frameCount)
        {
            foreach (RuleScheduling rule in scheduling)
            {
                if (rule.IsExpectedAtFrame(frameCount))
                {
                    yield return this[rule.RuleType];
                }
            }
        }
    }
}
