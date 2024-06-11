# [MockProvider](https://github.com/DerGuru/MockProvider)
## What is this about?
This little helper provides you with an implementation for `IServiceProvider` and `IServiceCollection` to be used as a replacement for the default .NET CORE types during unit tests and uses Mocks from the [Moq-Framework](https://github.com/moq/).

I personally use it to test, whether the startup type registrations to the IoC-Container are complete.

## Why did you build something like that?!
I was looking for an `IServiceCollection/IServiceProvider` using Mocks instead of the real thing, in one class (or two dependend classes), to test my StartUp-Code for multiple WebSites/WebServices, which I recently converted to IoC and which gave me some headaches at first, because I forgot to register some of the later needed types to the IoC-Container. 
And since I did not find any, I decided to build one of my own. 

You can use DI in ASP.NET CORE in multiple ways.

1. Constructor Injection into classes, that are created by the Framework. In my case the controllers.
```csharp
    public class MyController : Controller
    {
        public MyController(IFoo myFoo){ ... }
    }
``` 
2. Method Injection, into methods that are called from ASP.NET CORE. Like Controller Methods using `[FromServices]` Attribute.
```csharp
public class MyController : Controller
    {
        public async Task<IActionResult> Index([FromServices]IFoo myFoo){ ... }
    }
```
3. You could always use it "manually" providing an instance of your `IServiceProvider` to many constructors and heve the Dependency resolved in the constructor, the method where it is needed. I don't say you should, but you could.

Well, I was interested in the first two, so I created a small [Startup Tester](https://github.com/DerGuru/MockProvider) as well, to keep my boiler plate code to a minimum ... and because I like a challenge.

## What can I do with it?
You can use it as `IServiceCollection` and `IServiceProvider` within StartUp and later for e.g. Controllers/Methods, that make use of .NET CORE's Depedency Injection.

If you don't want to run through the complete setup every time, you may also add Mocks of your own.

### Use it as `IServiceCollection`
Create an empty MockProvider and use it as your `IServiceCollection` in any default `StartUp.ConfigureServices`.

### Use it as `IServiceProvider`
Create a filled MockProvider and use it as your `IServiceProvider` in any method, that wants one.

### Add you own Mocks
The easiest way to add specific mocks to it, is to use the `CreateMock<T>` method.

```csharp
    var m = new MockProvider();
    m.CreateMock<IFoo>();
    Assert.IsNotNull(m.GetService<IFoo>());
```

There are some overloads:
- `public Mock<U> CreateMock<U>()`
This is the most basic version. It finds constructor parameters and creates mocks for them as well. When done, it returns your created mock.
Since this will use previously registered types for constructor parameter matching and directly creates the mock, should only be used after putting the parameter types in the container or for constructor parameterless types.  

- `public Mock<U> CreateMock<U>(params object[] o)`
This lets you specify the constructor parameters and returns your created mock.

- `public MockDescriptor CreateMock(Type serviceType)`
Like the first one, this will do everything by itself, but handing back an internal representation of the IoC-Registration. The Mock is created lazily on first use. 

- `public MockDescriptor CreateMock(Type serviceType, params object[] o)`
Like the second one, this lets you specify the constructor parameters, but handing back an internal representation of the IoC-Registration. The Mock is created lazily on first use. 

- `public MockDescriptor CreateMock(Type serviceType, IEnumerable<object> o)`
Like the second one, this lets you specify the constructor parameters, but handing back an internal representation of the IoC-Registration. The Mock is created lazily on first use. 

And then there is: 

- `public void Add(ServiceDescriptor item)`
Using the `IServiceCollection`s internal type, there are three ways to go:
  1. The default `ServiceDescriptor` from .NET Core's IoC Container.
  This will use the ServiceType and register a Mock for it. The Mock is created lazily on first use. 
  1.  There is a `MockDescriptor` derived from `ServiceDescriptor`, which allows you to first create your Mock and then add it to the IoC-Container. The `MockDescriptor`-Instance is added to the Container, as it is. 
  2.  There is also a `InstanceDescriptor`, which will add a real object to the Container, allowing you to implement your own stub and use it in the IoC-Container. The `InstanceDescriptor`-Instance is added to the Container, as it is, since it derives from MockDescriptor.

## FAQ
### When registering the same service type multiple time, which one will be returned on `GetService<T>()`?
```csharp
    var fooMock1 = new Mock<IFoo>();
    var fooMock2 = new Mock<IFoo>();
    var m = new MockProvider();
    m.Add<IFoo>(fooMock1);
    m.Add<IFoo>(fooMock2);
```
The first entry wins. So the above will add  only `fooMock1` to the container.