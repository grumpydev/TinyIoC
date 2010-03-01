using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC.Tests.TestData.BasicClasses;

namespace TinyIoC.Tests.TestData
{
    public class UtilityMethods
    {
        internal static TinyIoCContainer GetContainer()
        {
            return new TinyIoCContainer();
        }

        internal static void RegisterInstanceStrongRef(TinyIoCContainer container)
        {
            var item = new TestClassDefaultCtor();
            item.Prop1 = "Testing";
            container.Register<TestClassDefaultCtor>(item).WithStrongReference();
        }

        internal static void RegisterInstanceWeakRef(TinyIoCContainer container)
        {
            var item = new TestClassDefaultCtor();
            item.Prop1 = "Testing";
            container.Register<TestClassDefaultCtor>(item).WithWeakReference();
        }

        internal static void RegisterFactoryStrongRef(TinyIoCContainer container)
        {
            var source = new TestClassDefaultCtor();
            source.Prop1 = "Testing";

            var item = new Func<TinyIoCContainer, NamedParameterOverloads, TestClassDefaultCtor>((c, p) => source);
            container.Register<TestClassDefaultCtor>(item).WithStrongReference();
        }

        internal static void RegisterFactoryWeakRef(TinyIoCContainer container)
        {
            var source = new TestClassDefaultCtor();
            source.Prop1 = "Testing";

            var item = new Func<TinyIoCContainer, NamedParameterOverloads, TestClassDefaultCtor>((c, p) => source);
            container.Register<TestClassDefaultCtor>(item).WithWeakReference();
        }

    }
}
