using System;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace JetBrains.ReSharper.Plugins.Spring
{
    internal class SpringFileNodeType : CompositeNodeType
    {
        public SpringFileNodeType(string s, int index) : base(s, index)
        {
        }

        public static readonly SpringFileNodeType Instance = new SpringFileNodeType("Spring_FILE", 0);

        public override CompositeElement Create()
        {
            return new SpringFile();
        }
    }
    internal class SpringCompositeNodeType : CompositeNodeType
    {
        public SpringCompositeNodeType(string s, int index) : base(s, index)
        {
        }
        public static readonly SpringCompositeNodeType BLOCK = new SpringCompositeNodeType("BLOCK", 0);
        public static readonly SpringCompositeNodeType DEFINE = new SpringCompositeNodeType("DEFINE", 1);
        public static readonly SpringCompositeNodeType LAMBDA = new SpringCompositeNodeType("LAMBDA", 2);
        public static readonly SpringCompositeNodeType IF = new SpringCompositeNodeType("IF", 3);
        public static readonly SpringCompositeNodeType COND = new SpringCompositeNodeType("COND", 4);
        public static readonly SpringCompositeNodeType APP = new SpringCompositeNodeType("APP", 5);
        public static readonly SpringCompositeNodeType IDENT = new SpringCompositeNodeType("IDENT", 6);
        public static readonly SpringCompositeNodeType LIT = new SpringCompositeNodeType("LIT", 7);
        public static readonly SpringCompositeNodeType QUOTE = new SpringCompositeNodeType("QUOTE", 8);
        public static readonly SpringCompositeNodeType PARAM_LIST = new SpringCompositeNodeType("PARAM_LIST", 9);
        public static readonly SpringCompositeNodeType EXPR = new SpringCompositeNodeType("PARAM_LIST", 10);

        public override CompositeElement Create()
        {
            if (this == BLOCK)
                return new SpringBlock();
            if (this == DEFINE)
                return new SpringDefine();
            if (this == LAMBDA)
                return new SpringLambda();
            if (this == LIT)
                return new SpringLit();
            if (this == IDENT)
                return new SpringIdent();
            if (this == EXPR)
                return new SpringExpr();
            
            throw new InvalidOperationException();
        }
    }

}