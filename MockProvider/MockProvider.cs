using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MockProvider : IList<ServiceDescriptor>, IServiceProvider, IServiceCollection
{

    #region IList<ServiceDescriptor>

    public int Count => _mocks.Count;

    public bool IsReadOnly => false;

    public ServiceDescriptor this[int index]
    {
        get => _mocks[index];
        set => _mocks[index] = value;
    }

    public void Add(ServiceDescriptor item)
    {
        if (item is MockDescriptor)
            _mocks.Add(item);
        else
            _mocks.Add(new MockDescriptor(item));
    }

    public int IndexOf(ServiceDescriptor item)
    {
        var mock = _mocks.FirstOrDefault(x => x.ServiceType.FullName == item.ServiceType.FullName);
        return mock == null ? -1 : _mocks.IndexOf(mock);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        _mocks.Insert(index, item);
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

    private List<ServiceDescriptor> _mocks = new List<ServiceDescriptor>();
    private static readonly Type _ienumerableType = typeof(IEnumerable<>);
    public MockProvider()
    {
        Initialize();
    }

    private void Initialize()
    {
        _mocks.Add(new InstanceDescriptor<IServiceProvider>(this));
        _mocks.Add(new InstanceDescriptor<IServiceCollection>(this));
    }

    public MockProvider(IEnumerable mocks) : this()
    {
        foreach (var mock in mocks)
        {
            var t = mock.GetType();
            while (t.Namespace == "Castle.Proxies")
                t = t.BaseType;
            Add(t, mock);
        }
    }

    public MockProvider(params object[] mocks) : this(mocks.AsEnumerable()) { }

    #region iserviceprovider
    public object GetService(Type serviceType)
    {
        //find direct type
        var retval = _mocks.FirstOrDefault(x => x.ServiceType.FullName == serviceType.FullName);
        if (retval != null)
            return retval.ImplementationInstance ?? retval.ImplementationFactory.Invoke(this);

        if (_mocks.Any(x => x.ServiceType.Name == serviceType.Name))
        {
            return CreateMock(serviceType).ImplementationInstance ?? retval.ImplementationFactory.Invoke(this);
        }

        if (serviceType.Name == _ienumerableType.Name && serviceType.IsGenericType)
        {
            var type = serviceType.GetGenericArguments().FirstOrDefault();
            if (type != null)
            {
                var arr = _mocks.Where(x => x.ServiceType.FullName == type.FullName).Select(x => x.ImplementationInstance).ToArray();
                var array = Array.CreateInstance(type, arr.Length);
                for (int i = 0; i < arr.Length; ++i)
                {
                    array.SetValue(arr[i], i);
                }

                return array;
            }
        }

        return retval?.ImplementationInstance ?? retval?.ImplementationFactory?.Invoke(this);
    }
    #endregion

    public T GetMock<T>() where T : class => GetMock(typeof(T)) as T;

    public object GetMock(Type serviceType)
    {
        var retVal = _mocks.FirstOrDefault(x => x.ServiceType.FullName == serviceType.FullName);
        if (retVal == null && _mocks.Any(x => x.ServiceType.Name == serviceType.Name))
        {
            return CreateMock(serviceType).ImplementationInstance;
        }
        return retVal.ImplementationInstance;
    }

    public IServiceCollection AddTransient<T>() where T : class
        => Add(typeof(T), ServiceLifetime.Transient);

    public IServiceCollection AddTransient(Type t)
        => Add(t, ServiceLifetime.Transient);

    public IServiceCollection AddSingleton<T>() where T : class
        =>Add(typeof(T), ServiceLifetime.Singleton);
    

    public IServiceCollection AddSingleton(Type t)
    => Add(t, ServiceLifetime.Singleton);
    
    public IServiceCollection AddScoped<T>() where T : class
        => Add(typeof(T), ServiceLifetime.Scoped);

    public IServiceCollection AddScoped(Type t) 
        => Add(t, ServiceLifetime.Scoped);

    public IServiceCollection Add(Type t, ServiceLifetime lifeTime)
    {
        var sd = new MockDescriptor(t, lifeTime);
        _mocks.Add(sd);
        return this;
    }
    public MockDescriptor<T>? Add<T>(T mock) where T : class
    {
        if (mock is ServiceDescriptor)
        {
            Add(mock as ServiceDescriptor);
            return mock as MockDescriptor<T>;
        }
        else
        {
            var e = new MockDescriptor<T>(mock);
            _mocks.Add(e);
            return e;
        }

    }

    public ServiceDescriptor Add(Type t, object mock)
    {
        var e = new ServiceDescriptor(t, mock);
        _mocks.Add(e);
        return e;
    }

    public InstanceDescriptor<T> CreateMock<T>(params object[] o) where T : class
    {

        var descriptor = new InstanceDescriptor<T>(NSubstitute.Substitute.For<T>(o.ToArray()));
        Add(descriptor);
        return descriptor;
    }

    public InstanceDescriptor<T> CreateMock<T>(Type serviceType) where T : class => CreateMock<T>(GetConstructorParameters(serviceType));

    public ServiceDescriptor CreateMock(Type serviceType) => CreateMock(serviceType, GetConstructorParameters(serviceType));
    public ServiceDescriptor CreateMock(Type serviceType, params object[] o)
    {
        var descriptor = new ServiceDescriptor(serviceType, NSubstitute.Substitute.For(new Type[] { serviceType }, o.ToArray()));
        Add(descriptor);
        return descriptor;
    }
    public object[] GetConstructorParameters<T>() => GetConstructorParameters(typeof(T));
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



}
