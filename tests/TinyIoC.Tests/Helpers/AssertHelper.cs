using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TinyIoC.Tests
{
    internal class AssertHelper
    {
        internal static T ThrowsException<T>(Action callback)
            where T : Exception
        {
            try
            {
                callback();
                throw new InvalidOperationException("No exception was thrown.");
            }
            catch (Exception ex)
            {
                if (typeof(T).IsAssignableFrom(ex.GetType()))
                    return (T)ex;
                else
                    throw new InvalidOperationException(string.Format("Exception of type '{0}' expected, but got exception of type '{1}'.",
                        typeof(T), ex.GetType()), ex);
            }
        }
    }
}
