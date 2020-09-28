using Moq;
using System;

public class InstanceDescriptor : MockDescriptor
{ 
    public InstanceDescriptor(Type serviceType, object instance) : base(serviceType, null)
    {
        Instance = instance;
    }
    public override object Instance { get; }
    public override Mock Mock { get; } = null;
    public override void Verify() { }
}

