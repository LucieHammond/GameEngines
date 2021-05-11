using GameEngine.Core.Pools;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class TestObject
    {
        public string Type;
        public bool Initialized = false;
        public bool Activated = false;
        public bool Cleared = false;

        public TestObject(string type)
        {
            Type = type;
        }
    }

    public class DummyPooler : IObjectPooler<TestObject>
    {
        public List<TestObject> CreatedObjects;
        private readonly string m_ObjectType;

        public DummyPooler(string objectType)
        {
            CreatedObjects = new List<TestObject>();
            m_ObjectType = objectType;
        }

        public TestObject CreateObject()
        {
            TestObject pooledObject = new TestObject(m_ObjectType);
            CreatedObjects.Add(pooledObject);
            pooledObject.Initialized = true;
            return pooledObject;
        }

        public void PrepareObject(TestObject pooledObject)
        {
            pooledObject.Activated = true;
        }

        public void RestoreObject(TestObject pooledObject)
        {
            pooledObject.Activated = false;
        }

        public void DestroyObject(TestObject pooledObject)
        {
            pooledObject.Cleared = true;
        }
    }
}
