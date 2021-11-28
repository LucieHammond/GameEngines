using GameEngine.Core.Unity.System;
using UnityEngine;

namespace GameEngine.PMR.UnityTests.Runtime.Models
{
    /// <summary>
    /// A content descriptor used for tests
    /// </summary>
    [CreateAssetMenu(fileName = "NewDescriptor", menuName = "Content/Test/Test Descriptor", order = 10)]
    public class TestDescriptor : ContentDescriptor
    {
        public string ContentValue;
    }

    /// <summary>
    /// Another content descriptor used for tests
    /// </summary>
    public class OtherDescriptor : ContentDescriptor
    {

    }
}
