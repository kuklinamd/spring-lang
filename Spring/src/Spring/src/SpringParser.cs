using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.I18n.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;
using JetBrains.Util.Console;

namespace JetBrains.ReSharper.Plugins.Spring
{
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
                builder.Error("F: " + b);

                ParseDefines(builder);

                builder.Done(fileMark, SpringFileNodeType.Instance, null);
                var file = (IFile) builder.BuildTree();
                
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
                    builder.AdvanceLexer();
                    SkipWhitespace(builder);
                    var startBlock = builder.GetTokenType();
                    if (startBlock == SpringTokenType.DEFINE)
                    {
                        builder.AdvanceLexer();
                        SkipWhitespace(builder);
                        ParseIdent(builder);
                        SkipWhitespace(builder);
                        ParseExpr(builder);
                    }
                    else
                    {
                        builder.Error("Expected definition");
                    }

                    SkipWhitespace(builder);
                    if (builder.GetTokenType() == SpringTokenType.RPAREN)
                        builder.AdvanceLexer();
                    else
                        builder.Error("Expected ')'");

                    builder.Done(start, SpringCompositeNodeType.DEFINE, null);
                }
                else
                {
                    builder.Error("Expected '('");
                    builder.AdvanceLexer();
                }
                SkipWhitespace(builder);
            }
        }

        private void ParseIdent(PsiBuilder builder)
        {
            var start = builder.Mark();
            var defineName = builder.GetTokenType();
            if (defineName != SpringTokenType.IDENT)
                builder.Error("Expected definition name");
            else
                builder.AdvanceLexer();
            builder.Done(start, SpringCompositeNodeType.IDENT, null);
        }

        private void ParseBlock(PsiBuilder builder)
        {
            var tt = builder.GetTokenType();
            if (tt == SpringTokenType.LPAREN)
            {
                var start = builder.Mark();
                
                /* code here */
                
                if (builder.GetTokenType() != SpringTokenType.RPAREN)
                    builder.Error("Expected ')'");
                else
                    builder.AdvanceLexer();

                builder.Done(start, SpringCompositeNodeType.DEFINE, null);
            }
            else if (tt == SpringTokenType.RPAREN)
                return;
            else builder.AdvanceLexer();
        }

        private void ParseExpr(PsiBuilder builder)
        {
            var markExpr = builder.Mark();
            var expr = builder.GetTokenType();
            if (expr == SpringTokenType.LIT)
            {
                var mark = builder.Mark();
                builder.AdvanceLexer();
                builder.Done(mark, SpringCompositeNodeType.LIT, null);
            }
            else
            {
                builder.Error("Expected an expression!");
            }
            builder.Done(markExpr, SpringCompositeNodeType.EXPR, null);

            /*
            if (tt == SpringTokenType.LPAREN)
            {
                var start = builder.Mark();
                builder.AdvanceLexer();
                var startBlock = builder.GetTokenType();
                if (startBlock == SpringTokenType.LAMBDA)
                {
                    ParseLambda(builder);
                }
                else 
                {
                    builder.Error("Expected lambda");
                }

                if (builder.GetTokenType() != SpringTokenType.RPAREN)
                    builder.Error("Expected ')'");
                else
                    builder.AdvanceLexer();

                builder.Done(start, SpringCompositeNodeType.BLOCK, null);
            }
            else if (tt == SpringTokenType.RPAREN)
                return;
            else builder.AdvanceLexer();
            */
        }

        /*
        private void ParseLambda(PsiBuilder builder)
        {
            var tt = builder.GetTokenType();
            if (tt == SpringTokenType.LPAREN)
            {
                var start = builder.Mark();
                ParseParamList(builder);
                if (builder.GetTokenType() != SpringTokenType.RPAREN)
                    builder.Error("Expected ')'");
                else
                    builder.AdvanceLexer();

                builder.Done(start, SpringCompositeNodeType.PARAM_LIST, null);
            }
            else
            {
                ParseExpr(builder);   
            }
        }

        private void ParseParamList(PsiBuilder builder)
        {
            
        }
        */

        private static void SkipWhitespace(PsiBuilder builder)
        {
            while (builder.GetTokenType() == SpringTokenType.WS)
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
                    if (treeNode is PsiBuilderErrorElement error)
                    {
                        var range = error.GetDocumentRange();
                        highlightings.Add(new HighlightingInfo(range,
                            new CSharpSyntaxError(error.ErrorDescription, range)));
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