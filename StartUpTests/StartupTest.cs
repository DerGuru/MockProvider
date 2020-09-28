using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class StartupTest
{
    private static List<Type> FromInterface(Assembly asm, Type tBase)
           => asm.GetTypes().Where(x => x.GetInterfaces().Contains(tBase)).ToList();

    private static List<Type> FromClass(Assembly asm, Type tBase)
        => asm.GetTypes().Where(x => x.IsSubclassOf(tBase)).ToList();

    public static void TestIocConfiguration<TBase>(Action<IServiceCollection> configureServices)
    {
        var mocks = new MockProvider();
        var tBase = typeof(TBase);

        var asm = configureServices.Method.DeclaringType.Assembly;
        List<Type> types = tBase.IsInterface ? FromInterface(asm, tBase) : FromClass(asm, tBase);

        configureServices(mocks);
        foreach (var t in types)
        {
            foreach (var c in t.GetConstructors())
            {
                var parameters = c.GetParameters();
                foreach (var p in parameters)
                {
                    Assert.IsNotNull(mocks.GetService(p.ParameterType), $"{t.FullName} -> {c.Name} -> {p.Name}");
                }
            }

            foreach (var m in t.GetMethods())
            {
                var parameters = m.GetParameters().Where(x => x.GetCustomAttributes<FromServicesAttribute>().Any());
                foreach (var p in parameters)
                {
                    Assert.IsNotNull(mocks.GetService(p.ParameterType), $"{t.FullName} -> {m.Name} -> {p.Name}");
                }
            }
        }
    }
}
