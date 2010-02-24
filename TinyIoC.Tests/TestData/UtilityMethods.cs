using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC.Tests.TestData.BasicClasses;

namespace TinyIoC.Tests.TestData
{
    public class UtilityMethods
    {
        internal static TinyIoC GetContainer()
        {
            return new TinyIoC();
        }

        internal static void RegisterInstanceStrongRef(TinyIoC container)
        {
            var item = new TestClassDefaultCtor();
            item.Prop1 = "Testing";
            container.Register<TestClassDefaultCtor>(item).WithStrongReference();
        }

        internal static void RegisterInstanceWeakRef(TinyIoC container)
        {
            var item = new TestClassDefaultCtor();
            item.Prop1 = "Testing";
            container.Register<TestClassDefaultCtor>(item).WithWeakReference();
        }

        internal static void RegisterFactoryStrongRef(TinyIoC container)
        {
            var source = new TestClassDefaultCtor();
            source.Prop1 = "Testing";

            var item = new Func<TinyIoC, TinyIoC.NamedParameterOverloads, TestClassDefaultCtor>((c, p) => source);
            container.Register<TestClassDefaultCtor>(item).WithStrongReference();
        }

        internal static void RegisterFactoryWeakRef(TinyIoC container)
        {
            var source = new TestClassDefaultCtor();
            source.Prop1 = "Testing";

            var item = new Func<TinyIoC, TinyIoC.NamedParameterOverloads, TestClassDefaultCtor>((c, p) => source);
            container.Register<TestClassDefaultCtor>(item).WithWeakReference();
        }

    }
}
