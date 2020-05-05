namespace TinyIoC.Tests.Helpers
{
    using System;

    public static class ExceptionHelper
    {
         public static Exception Record(Action action)
         {
             try
             {
                 action.Invoke();

                 return null;
             }
             catch (Exception e)
             {
                 return e;
             }
         }
    }
}