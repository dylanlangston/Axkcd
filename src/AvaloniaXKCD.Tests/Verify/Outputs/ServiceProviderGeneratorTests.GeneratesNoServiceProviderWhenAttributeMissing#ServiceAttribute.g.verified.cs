//HintName: ServiceAttribute.g.cs
using System;

namespace AvaloniaXKCD.Generators
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ServiceAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }
    }
}