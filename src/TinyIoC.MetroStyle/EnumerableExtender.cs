using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyMessenger
{
    internal static class EnumerableExtender
    {
        internal static void ForEach<T>(this IEnumerable<T> list, Action<T> each)
        {
            foreach (T item in list)
                each(item);
        }
    }
}
