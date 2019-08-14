using System;

namespace Vitevic.Foundation.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static T[] RemoveLast<T>(this T[] source)
        {
            if( source == null || source.Length == 0 )
                return source;

            return source.RemoveAt(source.Length - 1);
        }
    }
}
