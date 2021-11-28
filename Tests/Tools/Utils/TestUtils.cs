using GameEngine.Core.Utilities;
using System;

namespace GameEnginesTest.Tools.Utils
{
    public static class TestUtils
    {
        public static string GetResourcesPath()
        {
            return PathUtils.GetFullPath($"{Environment.CurrentDirectory}/../../../Resources");
        }
    }
}
