using Autofac;
using WateryTart.Core.Playback;

namespace WateryTart.Core;

public class InstancePlatformSpecificRegistration<T>(object InstanceToRegister) : IPlatformSpecificRegistration
{
    object InstanceToRegister { get; } = InstanceToRegister;

    public void Register(ContainerBuilder builder)
    {
        builder.RegisterInstance(InstanceToRegister).As<T>().SingleInstance();
    }
}