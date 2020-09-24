using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

public class MockDescriptor : ServiceDescriptor
{
    private MockProvider _mocks;
    private Lazy<Mock> _mock;
    

    public MockDescriptor(Type serviceType, MockProvider provider) : base (serviceType, serviceType, ServiceLifetime.Transient)
    {
        _mocks = provider;
        _mock = new Lazy<Mock>(() => _mocks.CreateMock(serviceType));
    }

    public MockDescriptor(Type serviceType, Mock instance, MockProvider provider) : base(serviceType, serviceType, ServiceLifetime.Transient)
    {
        _mocks = provider;
        _mock = new Lazy<Mock>(instance);
    }

    public object Instance => _mock.Value.Object;

    public Mock Mock => _mock.Value;

    public bool IsCreated => _mock.IsValueCreated;

    public void Verify()
    {
        if (_mock.IsValueCreated)
            _mock.Value.Verify();
    }
    public override string ToString()
    {
        return ServiceType.Name;
    }
}

