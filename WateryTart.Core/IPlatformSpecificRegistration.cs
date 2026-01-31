using Autofac;

namespace WateryTart.Core;

public interface IPlatformSpecificRegistration
{
    public void Register(ContainerBuilder builder);
}