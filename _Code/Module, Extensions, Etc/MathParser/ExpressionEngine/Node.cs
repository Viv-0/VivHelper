﻿namespace SimpleExpressionEngine
{
    // Node - abstract class representing one node in the expression 
    public abstract class Node
    {
        public abstract object Eval(IContext ctx);
    }
}
