using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;

public class MockDescriptor : ServiceDescriptor
{
    public static object CreateMock(Type t, IServiceProvider sp)
       => Substitute.For(new Type[] { t},(sp as MockProvider).GetConstructorParameters(t));
   
    public MockDescriptor(ServiceDescriptor sd) : this(sd.ServiceType,sd.Lifetime)
    {
    }
    public MockDescriptor(Type serviceType, object instance) : base(serviceType, instance)
    {
    }
    public MockDescriptor(Type serviceType, ServiceLifetime lifetime) : base(serviceType, (sp) => CreateMock(serviceType,sp), lifetime)
    {
    }
    public MockDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime)
    {
    }
    
}

public class MockDescriptor<T> : MockDescriptor where T : class
{
    private static T CreateMockP(IServiceProvider sp) => MockDescriptor.CreateMock(typeof(T), sp) as T;
       
    public MockDescriptor() : base (typeof(T), CreateMockP,  ServiceLifetime.Transient)
    {
        
    }
    public MockDescriptor(Func<IServiceProvider,T> factory,ServiceLifetime serviceLifetime) : base(typeof(T), factory, serviceLifetime)
    {

    }

    public MockDescriptor(T instance) : base(typeof(T),instance)
    { 
        
    }
}

public class MockDescriptor<T,U> : MockDescriptor<T> where U : class, T where T :  class
{
    private static T CreateMock(IServiceProvider sp) => MockDescriptor.CreateMock(typeof(U), sp) as T;
        

    public MockDescriptor() : base( CreateMock, ServiceLifetime.Transient)
    {}
}
