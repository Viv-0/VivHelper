using System;

namespace SimpleExpressionEngine
{
    // NodeUnary for unary operations such as Negate
    class NodeUnary : Node
    {
        // Constructor accepts the two nodes to be operated on and function
        // that performs the actual operation
        public NodeUnary(Node rhs, Func<object, float> op)
        {
            _rhs = rhs;
            _op = op;
        }

        Node _rhs;                              // Right hand side of the operation
        Func<object, float> _op;               // The callback operator

        public override object Eval(IContext ctx)
        {
            // Evaluate RHS
            var rhsVal = (float)_rhs.Eval(ctx);

            // Evaluate and return
            var result = _op(rhsVal);
            return (float)result;
        }
    }
}
