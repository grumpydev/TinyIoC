using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.TestData
{
    namespace NestedClassDependencies
    {
        internal class Service1
        {
        }

        internal class Service2
        {
            Service3 service3;

            public Service2(Service3 service3)
            {
                this.service3 = service3;
            }
        }

        internal class Service3
        {
        }

        internal class RootClass
        {
            Service1 service1;
            Service2 service2;

            public string StringProperty { get; set; }
            public int IntProperty { get; set; }

            public RootClass(Service1 service1, Service2 service2)
                : this(service1, service2, "DEFAULT", 1976)
            {
            }

            public RootClass(Service1 service1, Service2 service2, string stringProperty, int intProperty)
            {
                this.service1 = service1;
                this.service2 = service2;
                StringProperty = stringProperty;
                IntProperty = intProperty;
            }
        }
    }
}

