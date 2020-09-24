﻿using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class MockProvider : IList<ServiceDescriptor>, IServiceProvider, IServiceCollection
{
    private List<MockDescriptor> _mocks = new List<MockDescriptor>();

    public MockProvider()
    {
        this.Add(new ServiceDescriptor(typeof(IServiceProvider), this));
        this.Add(new ServiceDescriptor(typeof(IServiceCollection), this));
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
    public object GetService(Type serviceType) => CreateMock(serviceType).Object;
    #endregion

    public void Add<T>(Mock<T> mock) where T : class
    {
        Add(typeof(T), mock);
    }

    public void Add(Type t, Mock mock)
    {
        var e = this.FirstOrDefault(x => x.ServiceType.FullName == t.FullName);
        if (e != null)
        {
            Remove(e);
        }
        _mocks.Add(new MockDescriptor(t, mock, this));
    }

    public Mock<U> CreateMock<U>(params object[] o) where U : class
    {
        return CreateMock(typeof(U), o) as Mock<U>;
    }

    public Mock<U> CreateMock<U>() where U : class
    {
        return CreateMock(typeof(U)) as Mock<U>;
    }

    public Mock CreateMock(Type serviceType) => CreateMock(serviceType, GetConstructorParameters(serviceType));
    
    public Mock CreateMock(Type serviceType, params object[] o) => CreateMock(serviceType, o.AsEnumerable());
    
    public Mock CreateMock(Type serviceType, IEnumerable<object> o)
    {
        var retVal = _mocks.FirstOrDefault(x => x.ServiceType.FullName == serviceType.FullName);
        if (retVal == null || !retVal.IsCreated)
        {
            var mockType = typeof(Mock<>);

            mockType = mockType.MakeGenericType(serviceType);
            Mock m = Activator.CreateInstance(mockType, o) as Mock;
            Add(serviceType, m);
            return m;
        }
        else
            return retVal.Mock;
    }

    private object[] GetConstructorParameters(Type t)
    {
        var c = t.GetConstructors().FirstOrDefault(x => !x.GetParameters().Any() || x.GetParameters().All(y => _mocks.Any(x => x.ServiceType.Name == y.ParameterType.Name)));
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
        set => _mocks[index] = new MockDescriptor(value.ServiceType, this);
    }

    void Add(ServiceDescriptor item)
    {
        _mocks.Add(new MockDescriptor(item.ServiceType, this));
    }

    public int IndexOf(ServiceDescriptor item)
    {
        var mock = _mocks.FirstOrDefault(x => x.ServiceType == item.ServiceType);
        return mock == null ? -1 : _mocks.IndexOf(mock);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        _mocks.Insert(index, new MockDescriptor(item.ServiceType, this));
    }

    public void RemoveAt(int index)
    {
        _mocks.RemoveAt(index);
    }

    void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
    {
        Add(item);
    }

    public void Clear()
    {
        _mocks.Clear();
    }

    public bool Contains(ServiceDescriptor item) => _mocks.Any(x => x.ServiceType == item.ServiceType);


    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        _mocks.Select(x => x as ServiceDescriptor).ToArray().CopyTo(array, arrayIndex);
    }

    public bool Remove(ServiceDescriptor item)
    {
        var mock = _mocks.FirstOrDefault(x => x.ServiceType == item.ServiceType);
        return (mock != null)
            ? _mocks.Remove(mock)
            : false;
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return _mocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _mocks.GetEnumerator();
    }
    #endregion
}
