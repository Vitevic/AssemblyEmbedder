using System;
using System.Collections.Generic;
using System.Linq;

namespace Vitevic.Foundation.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<T> AttributesOfType<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public static bool ImplementsGenericType(this Type type, Type generic)
        {
            if( !generic.IsGenericTypeDefinition || !generic.IsInterface)
                return false;

            return type.GetInterfaces().Any(t => t.IsGenericType && !t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == generic);
        }

        public static Type GetImplementorOfGenericInterface(this Type type, Type generic)
        {
            return type.GetInterfaces().First( t => t.IsGenericType && !t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == generic);
        }
    }
}
