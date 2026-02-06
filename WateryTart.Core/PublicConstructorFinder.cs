using System;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace WateryTart.Core;
public class PublicConstructorFinder : IConstructorFinder
{
    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        return constructors.Length > 0
            ? constructors
            : throw new NoConstructorsFoundException(targetType, this);
    }
}