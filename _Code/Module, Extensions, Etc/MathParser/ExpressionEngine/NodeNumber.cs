namespace SimpleExpressionEngine
{
    // NodeNumber represents a literal number in the expression
    class NodeNumber : Node
    {
        public NodeNumber(object number)
        {
            _number = number;
        }

        object _number;             // The number

        public override object Eval(IContext ctx)
        {
            // Just return it.  Too easy.
            return _number;
        }
    }
}
