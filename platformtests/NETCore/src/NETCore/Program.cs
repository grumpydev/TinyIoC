using System;
using TinyIoC;

namespace NETCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
	        var container = TinyIoCContainer.Current;
			container.Register<IFoo, Foo>();

	        container.Resolve<IFoo>().SayHello();
	        Console.ReadKey();
        }
    }

	public interface IFoo
	{
		void SayHello();
	}

	public class Foo : IFoo
	{
		public void SayHello()
		{
			Console.WriteLine("Hello World!");
		}
	}
}
