using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.TestData
{
    namespace NestedInterfaceDependencies
    {
        internal interface IService1
        {
        }

        internal interface IService2
        {
        }

        internal interface IService3
        {
        }

        internal class Service1 : IService1
        {
        }

        internal class Service2 : IService2
        {
            IService3 service3;

            public Service2(IService3 service3)
            {
                this.service3 = service3;
            }
        }

        internal class Service3 : IService3
        {
        }

        internal class RootClass
        {
            IService1 service1;
            IService2 service2;

            public string StringProperty { get; set; }
            public int IntProperty { get; set; }

            public RootClass(IService1 service1, IService2 service2) : this(service1, service2, "DEFAULT", 1976)
            {
            }

            public RootClass(IService1 service1, IService2 service2, string stringProperty, int intProperty)
            {
                this.service1 = service1;
                this.service2 = service2;
                StringProperty = stringProperty;
                IntProperty = intProperty;
            }
        }
    }
}
