using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;

public class MockDescriptor : ServiceDescriptor
{
    protected MockProvider _mockProvider;
    private Mock _mock = null;

    private object[] GetConstructorParameters()
    {
        var c = ServiceType.GetConstructors().FirstOrDefault(x => !x.GetParameters().Any() || x.GetParameters().All(y => _mockProvider.Any(x => x.ServiceType.Name == y.ParameterType.Name)));
        if (c != null && c.GetParameters().Any())
        {
            var parameters = c.GetParameters();
            return parameters.Select(p => _mockProvider.GetService(p.ParameterType)).ToArray();
        }
        else
            return new object[] { };
    }

    private Mock CreateMock()
    {
        var mockType = typeof(Mock<>);

        mockType = mockType.MakeGenericType(ServiceType);
        return Activator.CreateInstance(mockType, GetConstructorParameters()) as Mock;
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
                IsCreated = true;
            }
            return _mock;
        }
    }


    public virtual bool IsCreated { get; private set; } = false;
     
    public void Verify()
    {
        if (IsCreated)
            _mock.Verify();
    }
    public override string ToString()
    {
        return ServiceType.Name;
    }
}

public class InstanceDescriptor : MockDescriptor
{ 
    public InstanceDescriptor(Type serviceType, object instance) : base(serviceType, null)
    {
        Instance = instance;
    }
    public override object Instance { get; }
    public override bool IsCreated => true;
    public override Mock Mock => null;
}

