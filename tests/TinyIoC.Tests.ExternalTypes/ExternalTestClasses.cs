using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.ExternalTypes
{
    public interface IExternalTestInterface
    {
    }

    public class ExternalTestClass : IExternalTestInterface
    {
    }

    public interface IExternalTestInterface2
    {
    }

    class ExternalTestClassInternal : IExternalTestInterface2
    {
    }
}
