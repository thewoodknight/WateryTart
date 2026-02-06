using Autofac;

namespace WateryTart.Core;

public interface IPlatformSpecificRegistration
{
    void Register(ContainerBuilder builder);
}