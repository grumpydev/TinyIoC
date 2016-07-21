using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if !PORTABLE
using System.Web;
#endif

namespace TinyIoC
{
#if TINYIOC_INTERNAL
    internal
#else
    public
#endif
    class HttpContextLifetimeProvider : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        private readonly string _KeyName = String.Format("TinyIoC.HttpContext.{0}", Guid.NewGuid());
        private static readonly Func<IDictionary> GetHttpContextCurrentItems;
        
        static HttpContextLifetimeProvider()
        {
#if PORTABLE
            var httpContextType = Type.GetType("System.Web.HttpContext");
            var current = httpContextType.GetRuntimeProperty("Current").GetMethod.Invoke(null, null);
            var getItemsProperty = httpContextType.GetRuntimeProperty("Items");
            GetHttpContextCurrentItems = (Func<IDictionary>)getItemsProperty.GetMethod.CreateDelegate(typeof(Func<IDictionary>),current);
#else
            GetHttpContextCurrentItems = () => HttpContext.Current.Items;
#endif
        }

        public object GetObject()
        {
            return GetHttpContextCurrentItems()[_KeyName];
        }

        public void SetObject(object value)
        {
            GetHttpContextCurrentItems()[_KeyName] = value;
        }

        public void ReleaseObject()
        {
            var item = GetObject() as IDisposable;

            if (item != null)
                item.Dispose();

            SetObject(null);
        }
    }

#if TINYIOC_INTERNAL
    internal
#else
    public
#endif
    static class TinyIoCAspNetExtensions
    {
        public static TinyIoC.TinyIoCContainer.RegisterOptions AsPerRequestSingleton(this TinyIoC.TinyIoCContainer.RegisterOptions registerOptions)
        {
            return TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registerOptions, new HttpContextLifetimeProvider(), "per request singleton");
        }

        public static TinyIoCContainer.MultiRegisterOptions AsPerRequestSingleton(this TinyIoCContainer.MultiRegisterOptions registerOptions)
        {
            return TinyIoCContainer.MultiRegisterOptions.ToCustomLifetimeManager(registerOptions, new HttpContextLifetimeProvider(), "per request singleton");
        }
    }
}
