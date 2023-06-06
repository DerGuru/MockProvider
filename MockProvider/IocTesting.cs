using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class IocTesting
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBase">The Base Class or Interface, every class to test has in common</typeparam>
    /// <param name="configureServices">populate the mockprovider in this method</param>
    /// <param name="asm">optional assembly, if not provided it is taken from the assembly where "configureServices" is taken from</param>
    /// <returns>Missing Type Registrations, with infos about Type, Method and Parameter names</returns>
    public static IEnumerable<MissingTypeRegistration> FindMissingRegistrations<TBase>(Action<IServiceCollection> configureServices, Assembly asm = null)
    {
        var missing = new LinkedList<MissingTypeRegistration>();
        var mocks = new MockProvider();
        var tBase = typeof(TBase);

        asm = asm ?? configureServices.Method.DeclaringType.Assembly;
        List<Type> types = asm.GetTypes().Where(x => tBase.IsAssignableFrom(x)).ToList();

        configureServices(mocks);
        foreach (var t in types)
        {
            foreach (var c in t.GetConstructors())
            {
                var parameters = c.GetParameters();
                foreach (var p in parameters)
                {
                    if (mocks.GetService(p.ParameterType) == null)
                        missing.AddLast(new MissingTypeRegistration(t, c, p));
                }
            }

            foreach (var m in t.GetMethods())
            {
                var parameters = m.GetParameters().Where(x => x.GetCustomAttributes().Any(xz => xz.GetType().FullName == "Microsoft.AspNetCore.Mvc.FromServicesAttribute"));
                foreach (var p in parameters)
                {
                    if (mocks.GetService(p.ParameterType) == null)
                        missing.AddLast(new MissingTypeRegistration(t, m, p));
                }
            }

        }
        return missing;
    }

    public class MissingTypeRegistration
    {
        public MissingTypeRegistration(Type type, MemberInfo method, ParameterInfo parameter)
        {
            Type = type;
            Method = method;
            Parameter = parameter;
        }

        public Type Type { get; }
        public MemberInfo Method { get; }
        public ParameterInfo Parameter { get; }

        public override string ToString() => $"{Type.FullName} -> {Method.Name} -> {Parameter.Name}";

    }
}
