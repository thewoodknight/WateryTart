using Autofac;

namespace WateryTart.Core;

public class TypePlatformSpecificRegistration<T> : IPlatformSpecificRegistration
{
    public void Register(ContainerBuilder builder)
    {
        builder.RegisterType<T>().AsImplementedInterfaces().SingleInstance();
    }
}