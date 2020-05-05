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
using TinyIoC.Tests.TestData.BasicClasses;
using TinyMessenger;

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

        public static ITinyMessengerHub GetMessenger()
        {
            return new TinyMessengerHub();
        }

        public static void FakeDeliveryAction<T>(T message)
            where T:ITinyMessage
        {
        }

        public static bool FakeMessageFilter<T>(T message)
            where T:ITinyMessage
        {
            return true;
        }

        public static TinyMessageSubscriptionToken GetTokenWithOutOfScopeMessenger()
        {
            var messenger = UtilityMethods.GetMessenger();

            var token = new TinyMessageSubscriptionToken(messenger, typeof(TestMessage));

            return token;
        }
    }
}
