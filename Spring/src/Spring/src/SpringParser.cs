using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.I18n.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;

namespace JetBrains.ReSharper.Plugins.Spring
{
    class SprintTreeScopeUtl
    {
        public static void InitScopes(ITreeNode node)
        {
            if (node is SpringFile file)
            {
                var parentScope = file.Scope;
                foreach (var child in file.Children())
                {
                    AddScope(child, parentScope);
                }
            }
        }

        private static void AddScope(ITreeNode node, Scope parentScope)
        {
                var scope = parentScope;
                if (node is SpringDefine define)
                {
                    define.ParentScope = parentScope;
                }
                else if (node is SpringLambda lambda)
                {
                    lambda.Scope = new Scope(parentScope);
                    scope = lambda.Scope;
                }
                else if (node is SpringLet let)
                {
                    let.Scope = new Scope(parentScope);
                    scope = let.Scope;
                }
                else if (node is SpringIdent ident)
                {
                    ident.ParentScope = scope;
                }
                else if (node is SpringIdentDecl decl)
                {
                    scope.Add(decl.DeclaredElement);
                }
                else if (node is SpringBindingExpr)
                {
                    // Let node passes to Bind (<ident> <expr>)
                    // newly created scope, but <expr> have to have
                    // it's parent scope.
                    scope = scope.ParentScope;
                }

                foreach (var child in node.Children())
                {
                    AddScope(child, scope);
                }
        }
    }

    internal class SpringParser : IParser
    {
        private readonly ILexer myLexer;

        public SpringParser(ILexer lexer)
        {
            myLexer = lexer;
        }

        public IFile ParseFile()
        {
            using (var def = Lifetime.Define())
            {
                var builder = new PsiBuilder(myLexer, SpringFileNodeType.Instance, new TokenFactory(), def.Lifetime);
                var fileMark = builder.Mark();

                StringBuilder b = new StringBuilder();
                foreach (var tok in myLexer.Tokens())
                {
                    b.Append(tok + " ");
                }

                b.ToString();

                ParseDefines(builder);

                builder.Done(fileMark, SpringFileNodeType.Instance, null);
                var file = (IFile) builder.BuildTree();

                SprintTreeScopeUtl.InitScopes(file);

                var sb = new StringBuilder();
                DebugUtil.DumpPsi(new StringWriter(sb), file);
                sb.ToString();

                return file;
            }
        }

        private void ParseDefines(PsiBuilder builder)
        {
            SkipWhitespace(builder);
            while (!builder.Eof())
            {
                var tt = builder.GetTokenType();
                if (tt == SpringTokenType.LPAREN)
                {
                    var start = builder.Mark();
                    AdvanceSkippingWhitespace(builder);
                    if (builder.GetTokenType() == SpringTokenType.DEFINE)
                    {
                        AdvanceSkippingWhitespace(builder);
                        ParseIdentDecl(builder);
                        SkipWhitespace(builder);
                        ParseExpr(builder);
                        SkipWhitespace(builder);
                    }
                    else
                    {
                        builder.Error("Expected definition!");
                    }

                    /*
                    if (builder.GetTokenType() == SpringTokenType.RPAREN)
                        builder.AdvanceLexer();
                    else
                        builder.Error("Expected ')' to close definition!");

                    builder.Done(start, SpringCompositeNodeType.DEFINE, null);
                    */

                    SkipWhitespace(builder);
                    if (builder.GetTokenType() == SpringTokenType.RPAREN)
                    {
                        builder.AdvanceLexer();
                        builder.Done(start, SpringCompositeNodeType.DEFINE, null);
                    }
                    else
                    {
                        builder.Error(start, "Expected ')' to close definition!");
                    }
                }
                else
                {
                    var tokenType = builder.GetTokenType();
                    var mark = builder.Mark();
                    builder.AdvanceLexer();
                    builder.Error(mark, "Expected '(', but got: " + tokenType.TokenRepresentation);
                }

                SkipWhitespace(builder);
            }
        }

        private static bool ParseIdentDecl(PsiBuilder builder)
        {
            return ParseIdentCommon(builder, SpringCompositeNodeType.IDENT_DECL);
        }

        private static bool ParseIdent(PsiBuilder builder)
        {
            return ParseIdentCommon(builder, SpringCompositeNodeType.IDENT);
        }


        private static bool ParseIdentCommon(PsiBuilder builder, SpringCompositeNodeType type)
        {
            var lexerAdvanced = false;
            var start = builder.Mark();
            var defineName = builder.GetTokenType();
            if (defineName == SpringTokenType.IDENT)
            {
                lexerAdvanced = true;
                builder.AdvanceLexer();
                builder.Done(start, type, null);
            }
            else
            {
                builder.Error(start, "Expected identifier!");
            }

            return lexerAdvanced;
        }

        private bool ParseExpr(PsiBuilder builder)
        {
            var lexerAdvanced = false;
            var markExpr = builder.Mark();
            var expr = builder.GetTokenType();
            if (expr == SpringTokenType.LIT)
            {
                lexerAdvanced = true;
                var mark = builder.Mark();
                builder.AdvanceLexer();
                builder.Done(mark, SpringCompositeNodeType.LIT, null);
            }
            else if (expr == SpringTokenType.IDENT)
            {
                lexerAdvanced = true;
                var mark = builder.Mark();
                builder.AdvanceLexer();
                builder.Done(mark, SpringCompositeNodeType.IDENT, null);
            }
            else if (expr == SpringTokenType.QUOTE)
            {
                var mark = builder.Mark();
                builder.AdvanceLexer();
                lexerAdvanced = ParseExpr(builder);
                builder.Done(mark, SpringCompositeNodeType.QUOTE, null);
            }
            else if (expr == SpringTokenType.LPAREN)
            {
                ParseBlock(builder);
            }
            else
            {
                builder.Error("Expected an expression!");
            }

            builder.Done(markExpr, SpringCompositeNodeType.EXPR, null);
            return lexerAdvanced;
        }

        // Call when you know that the current mark is LPAREN.
        private void ParseBlock(PsiBuilder builder)
        {
            var mark = builder.Mark();
            AdvanceSkippingWhitespace(builder);
            var tt = builder.GetTokenType();
            if (tt == SpringTokenType.LAMBDA)
            {
                var markLambda = builder.Mark();
                AdvanceSkippingWhitespace(builder);
                ParseList(builder, ParseIdentDecl);
                SkipWhitespace(builder);
                ParseExpr(builder);

                builder.Done(markLambda, SpringCompositeNodeType.LAMBDA, null);
            }
            else if (tt == SpringTokenType.IF)
            {
                var markIf = builder.Mark();
                AdvanceSkippingWhitespace(builder);
                // Cond
                ParseExpr(builder);
                SkipWhitespace(builder);
                // Then 
                ParseExpr(builder);
                SkipWhitespace(builder);
                // Else
                ParseExpr(builder);

                builder.Done(markIf, SpringCompositeNodeType.IF, null);
            }
            else if (tt == SpringTokenType.COND)
            {
                var markCond = builder.Mark();
                AdvanceSkippingWhitespace(builder);
                ParseSpaceSeparatedList(builder, ParseCondClause);
                builder.Done(markCond, SpringCompositeNodeType.COND, null);
            }
            else if (tt == SpringTokenType.LET)
            {
                var markLet = builder.Mark();
                AdvanceSkippingWhitespace(builder);
                ParseList(builder, ParseLetBinding);
                SkipWhitespace(builder);
                ParseExpr(builder);

                builder.Done(markLet, SpringCompositeNodeType.LET, null);
            }
            else
            {
                ParseSpaceSeparatedList(builder, ParseExpr);
            }

            SkipWhitespace(builder);

            if (builder.GetTokenType() == SpringTokenType.RPAREN)
            {
                AdvanceSkippingWhitespace(builder);
                builder.Done(mark, SpringCompositeNodeType.BLOCK, null);
            }
            else
            {
                builder.Error(mark, "Expected ')' to close block!");
            }
        }

        private bool ParseLetBinding(PsiBuilder builder)
        {
            bool lexerAdvanced = false;
            var mark = builder.Mark();
            if (builder.GetTokenType() == SpringTokenType.LPAREN)
            {
                AdvanceSkippingWhitespace(builder);
                var identParsed = ParseIdentDecl(builder);
                SkipWhitespace(builder);

                var markExpr = builder.Mark();
                var exprParsed = ParseExpr(builder);
                SkipWhitespace(builder); 
                builder.Done(markExpr, SpringCompositeNodeType.BIND, null);
                
                lexerAdvanced = identParsed || exprParsed;

                if (builder.GetTokenType() == SpringTokenType.RPAREN)
                {
                    AdvanceSkippingWhitespace(builder);
                    builder.Drop(mark);
                }
                else
                {
                    builder.Error(mark, "Expect ')' to close binding");
                }
            }
            else
            {
                builder.Error(mark, "Expected binding!");
            }

            return lexerAdvanced;
        }

        private bool ParseCondClause(PsiBuilder builder)
        {
            bool lexerAdvanced = false;
            if (builder.GetTokenType() == SpringTokenType.LPAREN)
            {
                bool leftExprParsed;
                AdvanceSkippingWhitespace(builder);
                if (builder.GetTokenType() == SpringTokenType.ELSE)
                {
                    AdvanceSkippingWhitespace(builder);
                    leftExprParsed = true;
                }
                else
                {
                    leftExprParsed = ParseExpr(builder);
                }

                SkipWhitespace(builder);
                var rightExprParsed = ParseExpr(builder);
                SkipWhitespace(builder);
                lexerAdvanced = leftExprParsed && rightExprParsed;

                if (builder.GetTokenType() == SpringTokenType.RPAREN)
                {
                    AdvanceSkippingWhitespace(builder);
                }
                else
                {
                    builder.Error("Expect ')' in list!");
                }
            }
            else
            {
                builder.Error("Expected cond clause!");
            }

            return lexerAdvanced;
        }

        // Leaves mark after ')'
        private void ParseList(PsiBuilder builder, Predicate<PsiBuilder> elementParser)
        {
            if (builder.GetTokenType() == SpringTokenType.LPAREN)
            {
                AdvanceSkippingWhitespace(builder);
                ParseSpaceSeparatedList(builder, elementParser);
                SkipWhitespace(builder);
                if (builder.GetTokenType() == SpringTokenType.RPAREN)
                    AdvanceSkippingWhitespace(builder);
            }
            else
            {
                builder.Error("Expected list!");
            }
        }

        // Left mark on ')'
        private void ParseSpaceSeparatedList(PsiBuilder builder, Predicate<PsiBuilder> elementParser)
        {
            var mark = builder.Mark();
            while (builder.GetTokenType() != SpringTokenType.RPAREN && !builder.Eof())
            {
                if (!elementParser(builder) && !builder.Eof())
                {
                    builder.AdvanceLexer();
                }

                SkipWhitespace(builder);
            }

            if (builder.GetTokenType() != SpringTokenType.RPAREN)
            {
                builder.Error("Expect ')' in list!");
            }

            builder.Done(mark, SpringCompositeNodeType.LIST, null);
        }

        private void AdvanceSkippingWhitespace(PsiBuilder builder)
        {
            builder.AdvanceLexer();
            SkipWhitespace(builder);
        }

        private static void SkipWhitespace(PsiBuilder builder)
        {
            while (builder.GetTokenType() == SpringTokenType.WS || builder.GetTokenType() == SpringTokenType.COMMENT)
            {
                builder.AdvanceLexer();
            }
        }
    }

    [DaemonStage]
    class SpringDaemonStage : DaemonStageBase<SpringFile>
    {
        protected override IDaemonStageProcess CreateDaemonProcess(IDaemonProcess process,
            DaemonProcessKind processKind, SpringFile file,
            IContextBoundSettingsStore settingsStore)
        {
            return new SpringDaemonProcess(process, file);
        }

        internal class SpringDaemonProcess : IDaemonStageProcess
        {
            private readonly SpringFile myFile;

            public SpringDaemonProcess(IDaemonProcess process, SpringFile file)
            {
                myFile = file;
                DaemonProcess = process;
            }

            public void Execute(Action<DaemonStageResult> committer)
            {
                var highlightings = new List<HighlightingInfo>();
                foreach (var treeNode in myFile.Descendants())
                {
                    switch (treeNode)
                    {
                        case PsiBuilderErrorElement error:
                        {
                            var range = error.GetDocumentRange();
                            highlightings.Add(new HighlightingInfo(range,
                                new CSharpSyntaxError(error.ErrorDescription, range)));
                            break;
                        }
                        case SpringIdent refer:
                        {
                            var refs = refer.GetFirstClassReferences();
                            foreach (var reff in refs)
                            {
                                if (reff.Resolve().Info.ResolveErrorType == ResolveErrorType.OK) continue;
                                var rangeR = reff.GetDocumentRange();
                                if (!rangeR.IsEmpty)
                                    highlightings.Add(new HighlightingInfo(rangeR,
                                        new CSharpSyntaxError("Cannot resolve a symbol", rangeR)));
                            }

                            break;
                        }
                    }
                }

                var result = new DaemonStageResult(highlightings);
                committer(result);
            }

            public IDaemonProcess DaemonProcess { get; }
        }

        protected override IEnumerable<SpringFile> GetPsiFiles(IPsiSourceFile sourceFile)
        {
            yield return (SpringFile) sourceFile.GetDominantPsiFile<SpringLanguage>();
        }
    }

    internal class TokenFactory : IPsiBuilderTokenFactory
    {
        public LeafElementBase CreateToken(TokenNodeType tokenNodeType, IBuffer buffer, int startOffset, int endOffset)
        {
            return tokenNodeType.Create(buffer, new TreeOffset(startOffset), new TreeOffset(endOffset));
        }
    }

    [ProjectFileType(typeof(SpringProjectFileType))]
    public class SelectEmbracingConstructProvider : ISelectEmbracingConstructProvider
    {
        public bool IsAvailable(IPsiSourceFile sourceFile)
        {
            return sourceFile.LanguageType.Is<SpringProjectFileType>();
        }

        public ISelectedRange GetSelectedRange(IPsiSourceFile sourceFile, DocumentRange documentRange)
        {
            var file = (SpringFile) sourceFile.GetDominantPsiFile<SpringLanguage>();
            var node = file.FindNodeAt(documentRange);
            return new SpringTreeNodeSelection(file, node);
        }

        public class SpringTreeNodeSelection : TreeNodeSelection<SpringFile>
        {
            public SpringTreeNodeSelection(SpringFile fileNode, ITreeNode node) : base(fileNode, node)
            {
            }

            public override ISelectedRange Parent => new SpringTreeNodeSelection(FileNode, TreeNode.Parent);
        }
    }
}