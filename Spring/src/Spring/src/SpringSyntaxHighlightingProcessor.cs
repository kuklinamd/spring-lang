using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Host.Features.SyntaxHighlighting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;

namespace JetBrains.ReSharper.Plugins.Spring
{
    [Language(typeof(SpringLanguage))]
    internal class SpringSyntaxHighlightingManager : RiderSyntaxHighlightingManager
    {
        public override SyntaxHighlightingProcessor CreateProcessor()
        {
            return new SpringSyntaxHighlightingProcessor();
        }
    }

    
    class SpringSyntaxHighlightingProcessor : SyntaxHighlightingProcessor
    {
        protected override bool IsLineComment(TokenNodeType tokenType)
        {
            return base.IsLineComment(tokenType) || tokenType.IsComment;
        }

        protected override bool IsNumber(TokenNodeType tokenType)
        {
            return base.IsNumber(tokenType) || tokenType.IsConstantLiteral;
        }
    }
}