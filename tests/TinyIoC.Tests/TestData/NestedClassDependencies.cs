//===============================================================================
// TinyIoC
//
// An easy to use, hassle free, Inversion of Control Container for small projects
// and beginners alike.
//
// http://hg.grumpydev.com/tinyioc
//===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

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
            Service3 Service3;

            public Service2(Service3 service3)
            {
                if (service3 == null)
                    throw new ArgumentNullException("service3");

                this.Service3 = service3;
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
                if (service1 == null)
                    throw new ArgumentNullException("service1");

                if (service2 == null)
                    throw new ArgumentNullException("service2");

                this.service1 = service1;
                this.service2 = service2;
                StringProperty = stringProperty;
                IntProperty = intProperty;
            }
        }
    }
}

