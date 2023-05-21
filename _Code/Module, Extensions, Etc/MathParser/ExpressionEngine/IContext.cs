using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExpressionEngine
{
    public interface IContext
    {
        object ResolveVariable(string name);
        object CallFunction(string name, object[] arguments);
    }
}
