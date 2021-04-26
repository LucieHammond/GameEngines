using System;

namespace GameEnginesTest.Tools.Dummy
{
    [Dummy]
    [DummyInherited]
    public class DummyTypeA { }

    public class DummyTypeB : DummyTypeA { }
    
    public class DummyTypeC : DummyTypeA, IDummyInterface { }

    public class DummyTypeD : IDummyInterface { }

    public interface IDummyInterface { }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DummyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DummyInheritedAttribute : Attribute { }
}
