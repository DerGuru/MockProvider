using Microsoft.Extensions.DependencyInjection;

public class InstanceDescriptor<T> : MockDescriptor where T : class
{ 
    public InstanceDescriptor(T instance) : base(typeof(T), instance)
    {
    }
}

