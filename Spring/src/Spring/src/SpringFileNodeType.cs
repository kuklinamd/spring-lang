using System;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;

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
        public static readonly SpringCompositeNodeType IDENT = new SpringCompositeNodeType("IDENT", 6);
        public static readonly SpringCompositeNodeType LIT = new SpringCompositeNodeType("LIT", 7);
        public static readonly SpringCompositeNodeType QUOTE = new SpringCompositeNodeType("QUOTE", 8);
        public static readonly SpringCompositeNodeType LIST = new SpringCompositeNodeType("LIST", 9);
        public static readonly SpringCompositeNodeType EXPR = new SpringCompositeNodeType("EXPR", 10);
        public static readonly SpringCompositeNodeType IDENT_DECL = new SpringCompositeNodeType("IDENT_DECL", 11);

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
            if (this == LIST)
                return new SpringList();
            if (this == IF)
                return new SpringIf();
            if (this == COND)
                return new SpringCond();
            if (this == QUOTE)
                return new SpringQuote();
            if (this == IDENT_DECL)
                return new SpringIdentDecl();
            
            throw new InvalidOperationException();
        }
    }

}