using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class IocTesting
{
    public class IocException : Exception
    {
        public static IocException Create(MissingTypeRegistration missingTypeRegistration) => new IocException(missingTypeRegistration);

        public IocException(MissingTypeRegistration missingTypeRegistration) : base() 
        {
            Type = missingTypeRegistration.Type;
            Method = missingTypeRegistration.Method;
            Parameter = missingTypeRegistration.Parameter;
        }

        public Type Type { get; }
        public MemberInfo Method { get; }
        public ParameterInfo Parameter { get; }

        public override string ToString() => $"{Type.FullName} -> {Method.Name} -> {Parameter.Name} of {Parameter.ParameterType}";
    }

    public class IocExceptions : Exception
    {
        private List<IocException> exceptions ;
        public IocExceptions(IEnumerable<MissingTypeRegistration> missingTypeRegistrations) : base()
        {
            this.exceptions = missingTypeRegistrations.Select(IocException.Create).ToList();
        }

        public override string ToString() => string.Join(Environment.NewLine, exceptions.Select(x => x.ToString()));
    }

    public static void TestIocConfiguration<TBase>(this Assembly asm, Action<IServiceCollection> configureServices)
    {
        var missing = asm.FindMissingRegistrations<TBase>(s =>
        {
            configureServices(s);
        });

        if (missing.Any())
            throw new IocExceptions(missing);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBase">The Base Class or Interface, every class to test has in common</typeparam>
    /// <param name="configureServices">populate the mockprovider in this method</param>
    /// <param name="asm">optional assembly, if not provided it is taken from the assembly where "configureServices" is taken from</param>
    /// <returns>Missing Type Registrations, with infos about Type, Method and Parameter names</returns>
    public static IEnumerable<MissingTypeRegistration> FindMissingRegistrations<TBase>(this Assembly asm, Action<IServiceCollection> configureServices)
    {
        var missing = new List<MissingTypeRegistration>();
        var mocks = new MockProvider();
        var tBase = typeof(TBase);

        List<Type> types = asm.GetTypes().Where(tBase.IsAssignableFrom).ToList();

        configureServices(mocks);
        foreach (var t in types)
        {
            var constructors = t.GetConstructors().ToDictionary(t => t, y => new List<MissingTypeRegistration>());
            foreach (var c in constructors)
            {
                
                var parameters = c.Key.GetParameters();
                foreach (var p in parameters)
                {
                    if (mocks.GetService(p.ParameterType) == null)
                        c.Value.Add(new MissingTypeRegistration(t, c.Key, p));
                }
            }
            if (constructors.Values.All(x => x.Any()))
            { 
                missing.AddRange(constructors.SelectMany(x => x.Value));
            }

            var methods = t.GetMethods().ToDictionary(t => t, y => new List<MissingTypeRegistration>());
            foreach (var m in methods)
            {
                var parameters = m.Key.GetParameters().Where(x => x.GetCustomAttributes().Any(xz => xz.GetType().FullName == "Microsoft.AspNetCore.Mvc.FromServicesAttribute"));
                foreach (var p in parameters)
                {
                    if (mocks.GetService(p.ParameterType) == null)
                        missing.Add(new MissingTypeRegistration(t, m.Key, p));
                }
            }
            if (methods.Values.All(x => x.Any()))
            {
                missing.AddRange(methods.SelectMany(x => x.Value));
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
