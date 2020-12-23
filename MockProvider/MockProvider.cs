using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MockProvider : IList<ServiceDescriptor>, IServiceProvider, IServiceCollection
{
    private List<MockDescriptor> _mocks = new List<MockDescriptor>();
    private static readonly Type _ienumerableType = typeof(IEnumerable<>);
    public MockProvider()
    {
        Initialize();
    }

    private void Initialize()
    {
        _mocks.Add(new InstanceDescriptor(typeof(IServiceProvider), this));
        _mocks.Add(new InstanceDescriptor(typeof(IServiceCollection), this));
    }

    public MockProvider(IEnumerable<Mock> mocks) : this()
    {
        foreach (var mock in mocks)
        {
            var t = mock.GetType().GenericTypeArguments.First();
            Add(t, mock);
        }
    }

    public MockProvider(params Mock[] mocks) : this(mocks.AsEnumerable()) { }

    #region IServiceProvider
    public object GetService(Type serviceType)
    {
        //find direct type
        var retVal = _mocks.FirstOrDefault(x => x.ServiceType.FullName == serviceType.FullName);
        if (retVal != null) 
            return retVal.Instance;

        if (_mocks.Any(x => x.ServiceType.Name == serviceType.Name))
        {
            return CreateMock(serviceType).Instance;
        }
        
        if (serviceType.Name == _ienumerableType.Name && serviceType.IsGenericType)
        {
            var type = serviceType.GetGenericArguments().FirstOrDefault();
            if (type != null)
            {
                var arr = _mocks.Where(x => x.ServiceType.FullName == type.FullName).Select(x => x.Instance).ToArray();
                var array = Array.CreateInstance(type, arr.Length);
                for(int i = 0; i< arr.Length; ++i)
                {
                    array.SetValue(arr[i],i);
                }

                return array;
            }
        }
        
        return retVal?.Instance;
    }
    #endregion

    public Mock<T> GetMock<T>() where T : class => GetMock(typeof(T)) as Mock<T>;

    public Mock GetMock(Type serviceType)
    {
        var retVal = _mocks.FirstOrDefault(x => x.ServiceType.FullName == serviceType.FullName);
        if (retVal == null && _mocks.Any(x => x.ServiceType.Name == serviceType.Name))
        {
            return CreateMock(serviceType).Mock;
        }
        return retVal?.Mock;
    }

    public void Add<T>(Mock<T> mock) where T : class
    {
        Add(typeof(T), mock);
    }

    public MockDescriptor Add(Type t, Mock mock)
    {
        var e = new MockDescriptor(t, mock, this);
        _mocks.Add(e);
        return e;
    }

    public Mock<U> CreateMock<U>(params object[] o) where U : class
    {
        return CreateMock(typeof(U), o).Mock as Mock<U>;
    }

    public Mock<U> CreateMock<U>() where U : class
    {
        return CreateMock(typeof(U)).Mock as Mock<U>;
    }

    public MockDescriptor CreateMock(Type serviceType) => CreateMock(serviceType, GetConstructorParameters(serviceType));

    public MockDescriptor CreateMock(Type serviceType, params object[] o) => CreateMock(serviceType, o.AsEnumerable());

    public MockDescriptor CreateMock(Type serviceType, IEnumerable<object> o)
    {
        var mockType = typeof(Mock<>);

        mockType = mockType.MakeGenericType(serviceType);
        Mock m = Activator.CreateInstance(mockType, o) as Mock;
        return Add(serviceType, m);
    }

    public object[] GetConstructorParameters(Type t)
    {
        var c = t.GetConstructors().FirstOrDefault(x => !x.GetParameters().Any() || x.GetParameters().All(y => _mocks.Any(xz => xz.ServiceType.Name == y.ParameterType.Name)));
        if (c != null && c.GetParameters().Any())
        {
            var parameters = c.GetParameters();
            return parameters.Select(p => GetService(p.ParameterType)).ToArray();
        }
        else
            return new object[] { };
    }

    public void Verify()
    {
        foreach (var m in _mocks)
            m.Verify();
    }

    #region IList<ServiceDescriptor>

    public int Count => _mocks.Count;

    public bool IsReadOnly => false;

    public ServiceDescriptor this[int index]
    {
        get => _mocks[index];
        set => _mocks[index] = value as MockDescriptor ?? new MockDescriptor(value.ServiceType, this);
    }

    public void Add(ServiceDescriptor item)
    {
        _mocks.Add(item as MockDescriptor ?? new MockDescriptor(item.ServiceType, this));
    }

    public int IndexOf(ServiceDescriptor item)
    {
        var mock = _mocks.FirstOrDefault(x => x.ServiceType.FullName == item.ServiceType.FullName);
        return mock == null ? -1 : _mocks.IndexOf(mock);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        _mocks.Insert(index, item as MockDescriptor ?? new MockDescriptor(item.ServiceType, this));
    }

    public void RemoveAt(int index)
    {
        _mocks.RemoveAt(index);
    }

    public void Clear()
    {
        _mocks.Clear();
        Initialize();
    }

    public bool Contains(ServiceDescriptor item) => _mocks.Any(x => x.ServiceType.FullName == item.ServiceType.FullName);


    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        _mocks.Select(x => x as ServiceDescriptor).ToArray().CopyTo(array, arrayIndex);
    }

    public bool Remove(ServiceDescriptor item)
    {
        var mock = _mocks.FirstOrDefault(x => x.ServiceType.FullName == item.ServiceType.FullName);
        return (mock != null)
            ? _mocks.Remove(mock)
            : false;
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator() => _mocks.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
