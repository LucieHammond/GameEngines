using GameEngine.Core.Utilities;
using GameEnginesTest.Tools.Dummy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEnginesTest.UnitTests.Core
{
    [TestClass]
    public class ReflectionUtilsTest
    {
        [TestMethod]
        public void GetTypeFromNameTest()
        {
            // For a type that isn't defined -> return null in all cases
            string undefinedType = "Unknown.Namespace.UndefinedType";
            Assert.IsNull(ReflectionUtils.GetTypeFromName(undefinedType, false));
            Assert.IsNull(ReflectionUtils.GetTypeFromName(undefinedType, true));

            // For a type defined in mscorlib -> type is found with limited and full search
            string coreType = "System.Int32";
            Type limitedResult = ReflectionUtils.GetTypeFromName(coreType, false);
            Type fullResult = ReflectionUtils.GetTypeFromName(coreType, true);
            Assert.AreEqual(typeof(System.Int32), limitedResult);
            Assert.AreEqual(typeof(System.Int32), fullResult);

            // For a type defined in ReflectionUtils's assembly -> type is found with limited and full search
            string executingType = typeof(ReflectionUtils).FullName; 
            limitedResult = ReflectionUtils.GetTypeFromName(executingType, false);
            fullResult = ReflectionUtils.GetTypeFromName(executingType, true);
            Assert.AreEqual(typeof(ReflectionUtils), limitedResult);
            Assert.AreEqual(typeof(ReflectionUtils), fullResult);

            // For a type defined in another assembly -> type is found with full search but not with limited search
            string otherType = typeof(ReflectionUtilsTest).FullName;
            limitedResult = ReflectionUtils.GetTypeFromName(otherType, false);
            fullResult = ReflectionUtils.GetTypeFromName(otherType, true);
            Assert.IsNull(limitedResult);
            Assert.AreEqual(typeof(ReflectionUtilsTest), fullResult);
        }

        [TestMethod]
        public void GetDerivedTypesTest()
        {
            Assembly testAssembly = Assembly.GetExecutingAssembly();
            Assembly otherAssembly = Assembly.GetAssembly(typeof(ReflectionUtils));
            Assembly systemAssembly = Assembly.GetAssembly(typeof(System.String));

            // Search types derived from DummyTypeA in test assembly -> find defined types B and C
            Type[] result1 = ReflectionUtils.GetDerivedTypes(typeof(DummyTypeA), testAssembly);
            Assert.AreEqual(2, result1.Length);
            Assert.IsTrue(result1.Contains(typeof(DummyTypeB)) && result1.Contains(typeof(DummyTypeC)));

            // Search types implementing IDummyInterface in test assembly -> find defined types C and D
            Type[] result2 = ReflectionUtils.GetDerivedTypes(typeof(IDummyInterface), testAssembly);
            Assert.AreEqual(2, result2.Length);
            Assert.IsTrue(result2.Contains(typeof(DummyTypeC)) && result2.Contains(typeof(DummyTypeD)));

            // Make the same research without specifying the assembly (test assembly is the calling assembly) -> get the same results
            Type[] result1bis = ReflectionUtils.GetDerivedTypes(typeof(DummyTypeA));
            Type[] result2bis = ReflectionUtils.GetDerivedTypes(typeof(IDummyInterface));
            Assert.IsTrue(result1bis.SequenceEqual(result1));
            Assert.IsTrue(result2bis.SequenceEqual(result2));

            // Make the same research in a list of assemblies including test assembly -> get the same results
            List<Assembly> assemblies = new List<Assembly>() { testAssembly, otherAssembly, systemAssembly };
            Type[] result1ter = ReflectionUtils.GetDerivedTypes(typeof(DummyTypeA), assemblies);
            Type[] result2ter = ReflectionUtils.GetDerivedTypes(typeof(IDummyInterface), assemblies);
            Assert.IsTrue(result1ter.SequenceEqual(result1));
            Assert.IsTrue(result2ter.SequenceEqual(result2));

            // Make the same research in assemblies other than test assembly -> get empty results
            assemblies.Remove(testAssembly);
            Assert.AreEqual(0, ReflectionUtils.GetDerivedTypes(typeof(DummyTypeA), assemblies).Length);
            Assert.AreEqual(0, ReflectionUtils.GetDerivedTypes(typeof(IDummyInterface), assemblies).Length);
        }

        [TestMethod]
        public void GetTypesWithAttributeTest()
        {
            Assembly testAssembly = Assembly.GetExecutingAssembly();
            Assembly otherAssembly = Assembly.GetAssembly(typeof(ReflectionUtils));
            Assembly systemAssembly = Assembly.GetAssembly(typeof(System.String));

            // Search types with DummyAttribute in test assembly -> find defined type A
            Type[] result1 = ReflectionUtils.GetTypesWithAttribute(typeof(DummyAttribute), testAssembly);
            Assert.AreEqual(1, result1.Length);
            Assert.IsTrue(result1.Contains(typeof(DummyTypeA)));

            // Search types with DummyInheritedAttribute in test assembly -> find defined types A, B and C
            Type[] result2 = ReflectionUtils.GetTypesWithAttribute(typeof(DummyInheritedAttribute), testAssembly);
            Assert.AreEqual(3, result2.Length);
            Assert.IsTrue(result2.Contains(typeof(DummyTypeA)) && result2.Contains(typeof(DummyTypeB)) && result2.Contains(typeof(DummyTypeC)));

            // Make the same research without specifying the assembly (test assembly is the calling assembly) -> get the same results
            Type[] result1bis = ReflectionUtils.GetTypesWithAttribute(typeof(DummyAttribute));
            Type[] result2bis = ReflectionUtils.GetTypesWithAttribute(typeof(DummyInheritedAttribute));
            Assert.IsTrue(result1bis.SequenceEqual(result1));
            Assert.IsTrue(result2bis.SequenceEqual(result2));

            // Make the same research in a list of assemblies including test assembly -> get the same results
            List<Assembly> assemblies = new List<Assembly>() { testAssembly, otherAssembly, systemAssembly };
            Type[] result1ter = ReflectionUtils.GetTypesWithAttribute(typeof(DummyAttribute), assemblies);
            Type[] result2ter = ReflectionUtils.GetTypesWithAttribute(typeof(DummyInheritedAttribute), assemblies);
            Assert.IsTrue(result1ter.SequenceEqual(result1));
            Assert.IsTrue(result2ter.SequenceEqual(result2));

            // Make the same research in assemblies other than test assembly -> get empty results
            assemblies.Remove(testAssembly);
            Assert.AreEqual(0, ReflectionUtils.GetTypesWithAttribute(typeof(DummyAttribute), assemblies).Length);
            Assert.AreEqual(0, ReflectionUtils.GetTypesWithAttribute(typeof(DummyInheritedAttribute), assemblies).Length);
        }

        [TestMethod]
        public void GetTypesMatchingConditionTest()
        {
            Assembly testAssembly = Assembly.GetExecutingAssembly();
            Assembly otherAssembly = Assembly.GetAssembly(typeof(ReflectionUtils));
            Assembly systemAssembly = Assembly.GetAssembly(typeof(System.String));
            bool condition(Type type) => typeof(IDummyInterface).IsAssignableFrom(type) 
                && type.GetCustomAttribute<DummyInheritedAttribute>() != null;

            // Search type implementing IDummyInterface and inheriting DummyInheritedAttribute -> find defined type C
            Type[] result = ReflectionUtils.GetTypesMatchingCondition(condition, testAssembly);
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result.Contains(typeof(DummyTypeC)));

            // Make the same research without specifying the assembly (test assembly is the calling assembly) -> get the same results
            Type[] resultbis = ReflectionUtils.GetTypesMatchingCondition(condition);
            Assert.IsTrue(resultbis.SequenceEqual(result));

            // Make the same research in a list of assemblies including test assembly -> get the same results
            List<Assembly> assemblies = new List<Assembly>() { testAssembly, otherAssembly, systemAssembly };
            Type[] resultter = ReflectionUtils.GetTypesMatchingCondition(condition, assemblies);
            Assert.IsTrue(resultter.SequenceEqual(result));

            // Make the same research in assemblies other than test assembly -> get empty results
            assemblies.Remove(testAssembly);
            Assert.AreEqual(0, ReflectionUtils.GetTypesMatchingCondition(condition, assemblies).Length);
        }
    }
}
