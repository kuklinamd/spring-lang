using System.Collections.Generic;
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Spring
{
    class SpringTokenType : TokenNodeType
    {
        public static SpringTokenType LIT = new SpringTokenType("LIT", 0);
        public static SpringTokenType IDENT = new SpringTokenType("IDENT", 1);
        public static SpringTokenType DEFINE = new SpringTokenType("DEFINE", 2);
        public static SpringTokenType LAMBDA = new SpringTokenType("LAMBDA", 3);
        public static SpringTokenType LET = new SpringTokenType("LET", 4);
        public static SpringTokenType COND = new SpringTokenType("COND", 5);
        public static SpringTokenType ELSE = new SpringTokenType("ELSE", 6);
        public static SpringTokenType RPAREN= new SpringTokenType("RPAREN", 7);
        public static SpringTokenType LPAREN = new SpringTokenType("LPAREN", 8);
        public static SpringTokenType OP = new SpringTokenType("OP", 9);
        public static SpringTokenType IF = new SpringTokenType("IF", 10);
        public static SpringTokenType QUOTE = new SpringTokenType("QUOTE", 11);
        public static SpringTokenType WS = new SpringTokenType("WS", 12);
        public static SpringTokenType COMMENT = new SpringTokenType("COMMENT", 13);
        public static SpringTokenType BAD_CHARACTER = new SpringTokenType("BAD_CHARACTER", 14);
        
        private static ISet<SpringTokenType> _keywordTokenTypes = new HashSet<SpringTokenType>
        {
            DEFINE, LAMBDA, LET, COND, ELSE, IF
        };
        
        private string _tokenName;
        
        public SpringTokenType(string s, int index) : base(s, index)
        {
            _tokenName = s;
        }

        public override LeafElementBase Create(IBuffer buffer, TreeOffset startOffset, TreeOffset endOffset)
        {
            // throw new System.NotImplementedException(buffer.ToString() + " " + startOffset.ToString() + " " + endOffset.ToString());
            return new SpringLeafToken(buffer.GetText(new TextRange(startOffset.Offset, endOffset.Offset)), this);
        }

        public override bool IsWhitespace => this == WS;
        public override bool IsComment => this == COMMENT;
        public override bool IsStringLiteral => false;

        public override bool IsConstantLiteral => this == LIT;
        public override bool IsIdentifier => this == IDENT;

        public override bool IsKeyword => _keywordTokenTypes.Contains(this);
        public override string TokenRepresentation => _tokenName;


        public class SpringLeafToken : LeafElementBase, ITokenNode
        {
            private readonly string _text;
            private SpringTokenType _type;

            public SpringLeafToken(string text, SpringTokenType tokenType)
            {
                _text = text;
                _type = tokenType;
            }
            
            public override int GetTextLength()
            {
                return _text.Length;
            }

            public override string GetText()
            {
                return _text;
            }

            public override StringBuilder GetText(StringBuilder to)
            {
                to.Append(GetText());
                return to;
            }

            public override IBuffer GetTextAsBuffer()
            {
                return new StringBuffer(GetText());
            }
            
            public override NodeType NodeType => _type;
            public override PsiLanguageType Language => SpringLanguage.Instance;
            public TokenNodeType GetTokenType()
            {
                return _type;
            }
        }
        
    }
}