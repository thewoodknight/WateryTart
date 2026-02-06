using System;
using Autofac;

namespace WateryTart.Core;

public class LambdaRegistration<T> : IPlatformSpecificRegistration where T : class
{
    private readonly Func<IComponentContext, T> _factory;

    public LambdaRegistration(Func<IComponentContext, T> factory)
    {
        _factory = factory;
    }

    public void Register(ContainerBuilder builder)
    {
        builder.Register(_factory).AsSelf().SingleInstance();
    }
}