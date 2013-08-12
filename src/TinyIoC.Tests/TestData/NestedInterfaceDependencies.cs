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
    namespace NestedInterfaceDependencies
    {
        internal interface IService1
        {
        }

        internal interface IService2
        {
            IService3 Service3 { get; }
        }

        internal interface IService3
        {
        }

        internal interface IService4
        {
            IService2 Service2 { get; }
        }

        internal interface IService5
        {
            IService4 Service4 { get; }
        }

        internal class Service1 : IService1
        {
        }
        
        internal class Service2 : IService2
        {
            public IService3 Service3 { get; private set; }

            public Service2(IService3 service3)
            {
                this.Service3 = service3;
            }
        }

        internal class Service3 : IService3
        {
        }

        internal class Service4 : IService4
        {
            public IService2 Service2 { get; private set; }

            public Service4(IService2 service1)
            {
                Service2 = service1;
            }
        }

        internal class Service5 : IService5
        {
            public Service5(IService4 service4)
            {
                Service4 = service4;
            }

            public IService4 Service4 { get; private set; }
        }


        internal interface IRoot
        {
            
        }

        internal class RootClass : IRoot
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

    // Nested deps with func
    public interface IStateManager
    {
        bool MoveToState(string state, object data);
        void Init();
    }

    public interface IView
    {
        object GetView();
    }

    public interface IViewManager
    {
        bool LoadView(IView view);
    }

    public class MainView : IView, IViewManager
    {

        public IView LoadedView { get; private set; }

        public object GetView()
        {
            return this;
        }

 

        public bool LoadView(IView view)
        {
            if (view == null)
                throw new ArgumentNullException("view");

            LoadedView = view;
            return true;
        }

    }

    public class ViewCollection
    {
        private readonly IEnumerable<IView> _views;

        public ViewCollection(IEnumerable<IView> views)
        {
            _views = views;
        }

        public IEnumerable<IView> Views
        {
            get { return _views; }
        }
    }

    public class SplashView : IView
    {
        public object GetView()
        {
            return this;
        }
    }

    public class StateManager : IStateManager
    {
        IViewManager _ViewManager;
        Func<string, IView> _ViewFactory;

        public bool MoveToState(string state, object data)
        {
            var view = _ViewFactory.Invoke(state);
            return _ViewManager.LoadView(view);
        }

        public void Init()
        {
            this.MoveToState("SplashView", null);
        }

        /// <summary>
        /// Initializes a new instance of the StateManager class.
        /// </summary>
        /// <param name="viewManager"></param>
        /// <param name="viewFactory"></param>
        public StateManager(IViewManager viewManager, Func<string, IView> viewFactory)
        {
            _ViewManager = viewManager;
            _ViewFactory = viewFactory;
        }
    }



}
