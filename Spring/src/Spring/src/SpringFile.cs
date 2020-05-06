using System.Xml;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.Spring
{
    public class SpringFile : FileElementBase
    {
        public override NodeType NodeType => SpringFileNodeType.Instance;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringBlock : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.BLOCK;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringDefine : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.DEFINE;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringLambda : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.LAMBDA;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringParamList: CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.PARAM_LIST;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    
    public class SpringLit : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.LIT;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringIdent : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.IDENT;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringExpr : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.EXPR;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
}