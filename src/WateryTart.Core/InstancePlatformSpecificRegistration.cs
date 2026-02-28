using Autofac;

namespace WateryTart.Core;

public class InstancePlatformSpecificRegistration<T>(object InstanceToRegister) : IPlatformSpecificRegistration
{
    object InstanceToRegister { get; } = InstanceToRegister;

    public void Register(ContainerBuilder builder)
    {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        builder.RegisterInstance(InstanceToRegister).As<T>().SingleInstance();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    }
}