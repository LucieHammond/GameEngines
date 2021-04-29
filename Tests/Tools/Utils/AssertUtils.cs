using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.Tools.Utils
{
    public static class AssertUtils
    {
        public static void ThrowsDerivativeException<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Threw exception {e.GetType().Name} which does not derive from expected {typeof(T).Name}.");
            }

            Assert.Fail($"No exception thrown. {typeof(T).Name} or derivatives were expected.");
        }

        public static void ThrowsNoException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.Fail($"Threw exception {e.GetType().Name} when none was excepted.");
            }
        }
    }
}
