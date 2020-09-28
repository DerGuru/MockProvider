using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;

public class MockDescriptor : ServiceDescriptor
{
    protected MockProvider _mockProvider;
    private Mock _mock = null;

    

    private Mock CreateMock()
    {
        var mockType = typeof(Mock<>);

        mockType = mockType.MakeGenericType(ServiceType);
        return Activator.CreateInstance(mockType, _mockProvider.GetConstructorParameters(ServiceType)) as Mock;
    }
    
    public MockDescriptor(Type serviceType, MockProvider provider) : base (serviceType, serviceType, ServiceLifetime.Transient)
    {
        _mockProvider = provider;
    }

    public MockDescriptor(Type serviceType, Mock instance, MockProvider provider) : base(serviceType, serviceType, ServiceLifetime.Transient)
    {
        _mockProvider = provider;
        _mock = instance;
    }

    public virtual object Instance => Mock.Object;

    public virtual Mock Mock
    {
        get
        {
            if (_mock == null)
            {
                Interlocked.CompareExchange<Mock>(ref _mock, CreateMock(), null);
            }
            return _mock;
        }
    }


  
     
    public virtual void Verify()
    {
        if (_mock !=  null)
            _mock.Verify();
    }
    public override string ToString()
    {
        return ServiceType.Name;
    }
}

public class RealInstanceDescriptor : MockDescriptor
{ 
    public RealInstanceDescriptor(Type serviceType, object instance) : base(serviceType, null)
    {
        Instance = instance;
    }
    public override object Instance { get; }
    public override Mock Mock => null;
    public override void Verify() { }
}

