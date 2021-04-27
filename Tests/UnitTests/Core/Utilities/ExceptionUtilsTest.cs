using GameEngine.Core.Utilities;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the class ExceptionUtils
    /// <see cref="ExceptionUtils"/>
    /// </summary>
    [TestClass]
    public class ExceptionUtilsTest
    {
        [TestMethod]
        public void CheckNonNullTest()
        {
            // When one of the parameter is null -> throws ArgumentNullException
            object nullObject = null;
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(nullObject));
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(new object(), nullObject));
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(string.Empty, new object(), nullObject));
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(new int[0], string.Empty, new object(), nullObject));

            // When multiple parameters are null -> throws ArgumentNullException
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(null, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNull(null, null, null, null, null));

            // When none of the parameters is null -> doesn't throw any exception
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckNonNull(new object()));
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckNonNull(string.Empty, new object()));
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckNonNull(new int[0], string.Empty, new object()));
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckNonNull(new DateTime(), new int[0], string.Empty, new object()));
        }

        [TestMethod]
        public void CheckNonNullCollectionTest()
        {
            // When the collection is null -> throws ArgumentNullException
            string[] testCollection = null;
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNullCollection(testCollection));

            // When one of the elements is null -> throws ArgumentNullException
            testCollection = new string[3] { null, "", "a string" };
            Assert.ThrowsException<ArgumentNullException>(() => ExceptionUtils.CheckNonNullCollection(testCollection));

            // When none of the elements is null -> doesn't throw any exception
            testCollection[0] = "nonNull";
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckNonNullCollection(testCollection));
        }

        [TestMethod]
        public void CheckConditionTest()
        {
            bool condition(int x) => x >= 0 && x % 2 == 0;

            // When the parameter doesn't meet the condition
            Assert.ThrowsException<ArgumentException>(() => ExceptionUtils.CheckCondition(-2, condition));
            Assert.ThrowsException<ArgumentException>(() => ExceptionUtils.CheckCondition(5, condition));

            // When the parameter meets the condition
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckCondition(2, condition));
            AssertUtils.ThrowsNoException(() => ExceptionUtils.CheckCondition(36, condition));
        }
    }
}
