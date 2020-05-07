using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using JetBrains.Util.DataStructures;

namespace JetBrains.ReSharper.Plugins.Spring
{
    public class Scope
    {
        public readonly Scope ParentScope;
        private readonly ISet<IDeclaredElement> _declarations = new HashSet<IDeclaredElement>();

        public Scope(Scope parentScope = null)
        {
           ParentScope = parentScope;
        }

        public IDeclaredElement GetOrNull(string ident)
        {
            var decl = _declarations.FirstOrDefault(d => d.ShortName == ident);
            if (decl != null)
            {
                return decl;
            }

            return ParentScope?.GetOrNull(ident);
        }

        public void Add(IDeclaredElement ident)
        {
            _declarations.Add(ident);
        }
    }
    public class SpringFile : FileElementBase
    {
        public Scope Scope = new Scope();
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
        // Filthy Hack
        public Scope ParentScope = new Scope(); 
        public override NodeType NodeType => SpringCompositeNodeType.DEFINE;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringLambda : CompositeElement
    {
        public Scope Scope = new Scope(); 
        public override NodeType NodeType => SpringCompositeNodeType.LAMBDA;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringList : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.LIST;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }


    public class SpringLit : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.LIT;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }


    public class SpringExpr : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.EXPR;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringIf : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.IF;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringCond : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.COND;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringQuote : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.QUOTE;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }
    
    public class SpringLet : CompositeElement
    {
        public Scope Scope = new Scope();
        public override NodeType NodeType => SpringCompositeNodeType.LET;

        public override PsiLanguageType Language => SpringLanguage.Instance;
    }

    public class SpringBindingExpr : CompositeElement
    {
        public override NodeType NodeType => SpringCompositeNodeType.BIND;

        public override PsiLanguageType Language => SpringLanguage.Instance;
        
    }

    public class SpringIdentDecl : CompositeElement, IDeclaration
    {
        private TreeTextRange IdentRange
        {
            get
            {
                foreach (var child in this.Children())
                {
                    if (child.NodeType == SpringTokenType.IDENT)
                    {
                        return child.GetTreeTextRange();
                    }
                    
                }
                return TreeTextRange.InvalidRange;
            }
        }
        

        public override NodeType NodeType => SpringCompositeNodeType.IDENT_DECL;

        public override PsiLanguageType Language => SpringLanguage.Instance;

        public XmlNode GetXMLDoc(bool inherit)
        {
            return null;
        }

        public void SetName(string name)
        {
        }

        public TreeTextRange GetNameRange()
        {
            return IdentRange;
        }

        public bool IsSynthetic()
        {
            return false;
        }

        public IDeclaredElement DeclaredElement => new SpringIdentDeclDeclared(this);

        public string DeclaredName => GetText();
    }

    public class SpringIdentDeclDeclared : IDeclaredElement
    {
        private SpringIdentDecl _springIdentDecl;

        private ISet<IReference> _references = new HashSet<IReference>();
        public SpringIdentDeclDeclared(SpringIdentDecl springIdentDecl)
        {
            _springIdentDecl = springIdentDecl;
        }

        public void AddReference(IReference reference)
        {
            _references.Add(reference);
        }

        public IPsiServices GetPsiServices()
        {
            return _springIdentDecl.GetPsiServices();
        }

        public IList<IDeclaration> GetDeclarations()
        {
            return new List<IDeclaration> { _springIdentDecl };
        }

        public IList<IDeclaration> GetDeclarationsIn(IPsiSourceFile sourceFile)
        {
            // TODO: sourceFile?
            return new List<IDeclaration> { _springIdentDecl };
        }

        public DeclaredElementType GetElementType()
        {
            // why not?
            return CLRDeclaredElementType.CONSTANT;
        }

        public XmlNode GetXMLDoc(bool inherit)
        {
            return null;
        }

        public XmlNode GetXMLDescriptionSummary(bool inherit)
        {
            return null;
        }

        public bool IsValid()
        {
            return _springIdentDecl.IsValid();
        }

        public bool IsSynthetic()
        {
            return false;
        }

        public HybridCollection<IPsiSourceFile> GetSourceFiles()
        {
            return new HybridCollection<IPsiSourceFile>(_springIdentDecl.GetSourceFile());
        }

        public bool HasDeclarationsIn(IPsiSourceFile sourceFile)
        {
            return sourceFile.Equals(_springIdentDecl.GetSourceFile());
        }

        public string ShortName => _springIdentDecl.DeclaredName;
        public bool CaseSensitiveName => true;
        public PsiLanguageType PresentationLanguage => SpringLanguage.Instance;
    }

    public class SpringIdent : CompositeElement
    {
        public Scope ParentScope = new Scope();
        public TreeTextRange IdentRange
        {
            get
            {
                foreach (var child in this.Children())
                {
                    if (child.NodeType == SpringTokenType.IDENT)
                    {
                        return child.GetTreeTextRange();
                    }
                    
                }
                return TreeTextRange.InvalidRange;
            }
        }


        public override NodeType NodeType => SpringCompositeNodeType.IDENT;
        public override PsiLanguageType Language => SpringLanguage.Instance;

        public override ReferenceCollection GetFirstClassReferences()
        {
            return new ReferenceCollection(new SpringIdentReference(this));
        }
    }

    public class SpringIdentReference : TreeReferenceBase<SpringIdent>
    {
        private readonly SpringIdent _ident;

        public SpringIdentReference([Annotations.NotNull] SpringIdent owner) : base(owner)
        {
            _ident = owner;
        }

        public override ResolveResultWithInfo ResolveWithoutCache()
        {
            var declared =  _ident.ParentScope.GetOrNull(GetName());
            if (declared != null)
            {
                return new ResolveResultWithInfo(new SimpleResolveResult(declared),
                    ResolveErrorType.OK);
            }

            return ResolveResultWithInfo.Unresolved;
        }

        public override string GetName()
        {
            return _ident.GetText();
        }

        // clr stuff
        public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName)
        {
            throw new NotImplementedException();
        }

        public override TreeTextRange GetTreeTextRange()
        {
            return _ident.IdentRange;
        }

        public override IReference BindTo(IDeclaredElement element)
        {
            ((SpringIdentDeclDeclared) element).AddReference(this);
            return this;
        }

        public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
        {
            return BindTo(element);
        }

        public override IAccessContext GetAccessContext()
        {
            return null;
        }

        public override bool IsValid()
        {
            return myOwner.IsValid();
        }
    }
}