using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExpressionEngine
{
    public class NodeFunctionCall : Node
    {
        public NodeFunctionCall(string functionName, Node[] arguments)
        {
            _functionName = functionName;
            _arguments = arguments;
        }

        string _functionName;
        Node[] _arguments;

        public override object Eval(IContext ctx)
        {
            ctx.
            // Evaluate all arguments
            var argVals = new object[_arguments.Length];
            for (int i=0; i<_arguments.Length; i++)
            {
                argVals[i] = _arguments[i].Eval(ctx);
            }

            // Call the function
            return ctx.CallFunction(_functionName, argVals);
        }
    }
}
