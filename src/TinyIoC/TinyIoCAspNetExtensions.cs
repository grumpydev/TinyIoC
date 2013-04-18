using System;
using System.Web;

namespace TinyIoC
{
    public class HttpContextLifetimeProvider : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        private readonly string _KeyName = String.Format("TinyIoC.HttpContext.{0}", Guid.NewGuid());

        public object GetObject()
        {
            return HttpContext.Current == null ? null : HttpContext.Current.Items[_KeyName];
        }

        public void SetObject(object value)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items[_KeyName] = value;
        }

        public void ReleaseObject()
        {
            var item = GetObject() as IDisposable;

            if (item != null)
                item.Dispose();

            SetObject(null);
        }
    }

    public static class TinyIoCAspNetExtensions
    {
        public static TinyIoC.TinyIoCContainer.RegisterOptions AsPerRequestSingleton(this TinyIoC.TinyIoCContainer.RegisterOptions registerOptions)
        {
            return TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registerOptions, new HttpContextLifetimeProvider(), "per request singleton");
        }
    }
}
