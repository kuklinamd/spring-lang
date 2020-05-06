using System;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;
using JetBrains.ReSharper.Plugins.Spring;

%%
%{
TokenNodeType currentTokenType;
%}

%unicode

%init{
  currentTokenType = null;
%init}

%namespace Sample
%class SampleLexerGenerated

%function _locateToken
%public
%type TokenNodeType
%ignorecase

%eofval{
  currentTokenType = null; return currentTokenType;
%eofval}


ALPHA=[A-Za-z]
DIGIT=[0-9]
NEWLINE=((\r\n)|\n)
NONNEWLINE_WHITE_SPACE_CHAR=[\ \t\b\012]
WHITE_SPACE_CHAR=({NEWLINE}|{NONNEWLINE_WHITE_SPACE_CHAR})
START_SYMBOL=({ALPHA}|"+"|"-"|"*"|"^"|"&"|"%"|"$"|"?"|"=")
IDENT=({START_SYMBOL}({DIGIT}|{START_SYMBOL})*)

%% 
<YYINITIAL> "#" { return currentTokenType = SpringTokenType.COMMENT; }
<YYINITIAL> "(" { return currentTokenType = SpringTokenType.LPAREN; }
<YYINITIAL> ")" { return currentTokenType = SpringTokenType.RPAREN; }
<YYINITIAL> define { return currentTokenType = SpringTokenType.DEFINE; }
<YYINITIAL> if { return currentTokenType = SpringTokenType.IF; }
<YYINITIAL> cond { return currentTokenType = SpringTokenType.COND; }
<YYINITIAL> lambda { return currentTokenType = SpringTokenType.LAMBDA; }
<YYINITIAL> let { return currentTokenType = SpringTokenType.LET; }
<YYINITIAL> else { return currentTokenType = SpringTokenType.ELSE; }
<YYINITIAL> "'" { return currentTokenType = SpringTokenType.QUOTE; }
<YYINITIAL> {IDENT}+ { return currentTokenType = SpringTokenType.IDENT; }
<YYINITIAL> {DIGIT}+ { return currentTokenType = SpringTokenType.LIT; }	
<YYINITIAL> {WHITE_SPACE_CHAR}+ { return currentTokenType = SpringTokenType.WS; }	
<YYINITIAL> . { return currentTokenType = SpringTokenType.BAD_CHARACTER; }	
