
%using System.Collections;
%using System.Text;
%using GPLEX.Parser;
%namespace GPLEX.Lexer

%option stack, minimize, parser, verbose, out:Scanner.cs

%{
        // User code from gplex.lex
        
        public ErrorHandler yyhdlr = null;

        Tokens GetIdToken(string str)
        {
            switch (str[0])
            {
                case 'a':
                    if (str.Equals("abstract") || str.Equals("as"))
                        return Tokens.csKeyword;
                    break;
                case 'b':
                    if (str.Equals("base") || str.Equals("bool") ||
                        str.Equals("break") || str.Equals("byte"))
                        return Tokens.csKeyword;
                    break;
                case 'c':
                    if (str.Equals("case") || str.Equals("catch")
                     || str.Equals("char") || str.Equals("checked")
                     || str.Equals("class") || str.Equals("const")
                     || str.Equals("continue"))
                        return Tokens.csKeyword;
                    break;
                case 'd':
                    if (str.Equals("decimal") || str.Equals("default")
                     || str.Equals("delegate") || str.Equals("do")
                     || str.Equals("double"))
                        return Tokens.csKeyword;
                    break;
                case 'e':
                    if (str.Equals("else") || str.Equals("enum")
                     || str.Equals("event") || str.Equals("explicit")
                     || str.Equals("extern"))
                        return Tokens.csKeyword;
                    break;
                case 'f':
                    if (str.Equals("false") || str.Equals("finally")
                     || str.Equals("fixed") || str.Equals("float")
                     || str.Equals("for") || str.Equals("foreach"))
                        return Tokens.csKeyword;
                    break;
                case 'g':
                    if (str.Equals("goto"))
                        return Tokens.csKeyword;
                    break;
                case 'i':
                    if (str.Equals("if") 
                     || str.Equals("int") || str.Equals("implicit")
                     || str.Equals("in") || str.Equals("interface")
                     || str.Equals("internal") || str.Equals("is"))
                        return Tokens.csKeyword;
                    break;
                case 'l':
                    if (str.Equals("lock") || str.Equals("long"))
                        return Tokens.csKeyword;
                    break;
                case 'n':
                    if (str.Equals("namespace") || str.Equals("new")
                     || str.Equals("null"))
                        return Tokens.csKeyword;
                    break;
                case 'o':
                    if (str.Equals("object") || str.Equals("operator")
                     || str.Equals("out") || str.Equals("override"))
                        return Tokens.csKeyword;
                    break;
                case 'p':
                    if (str.Equals("params") || str.Equals("private")
                     || str.Equals("protected") || str.Equals("public"))
                        return Tokens.csKeyword;
                    break;
                case 'r':
                    if (str.Equals("readonly") || str.Equals("ref")
                     || str.Equals("return"))
                        return Tokens.csKeyword;
                    break;
                case 's':
                    if (str.Equals("sbyte") || str.Equals("sealed")
                     || str.Equals("short") || str.Equals("sizeof")
                     || str.Equals("stackalloc") || str.Equals("static")
                     || str.Equals("string") || str.Equals("struct")
                     || str.Equals("switch"))
                        return Tokens.csKeyword;
                    break;
                case 't':
                    if (str.Equals("this") || str.Equals("throw")
                     || str.Equals("true") || str.Equals("try")
                     || str.Equals("typeof"))
                        return Tokens.csKeyword;
                    break;
                case 'u':
                    if (str.Equals("uint") || str.Equals("ulong")
                     || str.Equals("unchecked") || str.Equals("unsafe")
                     || str.Equals("ushort") || str.Equals("using"))
                        return Tokens.csKeyword;
                    break;
                case 'v':
                    if (str.Equals("virtual") || str.Equals("void"))
                        return Tokens.csKeyword;
                    break;
                case 'w':
                    if (str.Equals("while") || str.Equals("where"))
                        return Tokens.csKeyword;
                    break;
            }
            return Tokens.csIdent;
        }
        
        Tokens GetTagToken(string str)
        {
            switch (str)
            {
                case "%x":
                    yy_push_state(NMLST); return Tokens.exclTag;
                case "%s":
                    yy_push_state(NMLST); return Tokens.inclTag;
                case "%using":
                    yy_push_state(LCODE); return Tokens.usingTag;
                case "%namespace":
                    yy_push_state(LCODE); return Tokens.namespaceTag;
                case "%option":
                    yy_push_state(VRBTM); return Tokens.optionTag;
                default:
                    Error(77, TokenSpan()); return Tokens.repErr;
            }
        }
        
        public override void yyerror(string format, params object[] args)
        { if (yyhdlr != null) yyhdlr.ListError(TokenSpan(), 1, format); }   

        internal void Error(int n, LexSpan s)
        {
            if (yyhdlr != null) yyhdlr.ListError(s, n);
        }
        
        internal LexSpan TokenSpan() 
        { return new LexSpan(tokLin, tokCol, tokELin, tokECol, tokPos, tokEPos, buffer); }
        
        public string StateStr(int s)
        {
            switch (s) 
            {
        case INITIAL: return "0";
        case RULES: return "RULES";
        case UCODE: return "UCODE";
        case LCODE: return "LCODE";
        case BCODE: return "BCODE";
        case INDNT: return "INDNT";
        case CMMNT: return "CMMNT";
        case SMACT: return "SMACT";
        case XPEOL: return "XPEOL";
        case REGEX: return "REGEX";
        case NMLST: return "NMLST";
        case SPACE: return "SPACE";
        case VRBTM: return "VRBTM";
        default: return "state " + s.ToString();
            }
         }
         
         public string StateStack(int s)
         {
             string rslt = StateStr(s);
             int[] arry = scStack.ToArray();
             for (int i = 0; i < scStack.Count; i++)
                 rslt += (":" + StateStr(arry[i]));
             return rslt;
          }
             
         int depth = 0;
         
         // End user code from definitions section
%}

Eol             (\r\n?|\n)
NotWh           [^ \t\r\n]
Space           [ \t]
Ident           [a-zA-Z_][a-zA-Z0-9_]*
Number          [0-9]+
OctDig          [0-7]
HexDig          [0-9a-fA-F]

CmntStrt     \/\*
CmntEnd      \*\/
ABStar       [^\*\n\r]*

DotChr       [^\r\n]
EscChr       \\{DotChr}

ClsChs       [^\\\]\r\n]
ChrCls       \[({ClsChs}|{EscChr})+\]

StrChs       [^\\\"\a\b\f\n\r\t\v\0]
ChrChs       [^\\'\a\b\f\n\r\t\v\0]

LitStr       \"({StrChs}|{EscChr})*\"
ClsRef       \{{Ident}\}
RepMrk       \{{Number}(,{Number}?)?\}

CShOps       [+\-*/%&|<>=@?:!]
Dgrphs       (\+\+|--|==|!=|\+=|-=|\*=|\/=|%=|>=|<=|<<|>>)

RgxChs       [^ \t\r\n\\[\"\{]
Regex        ({RgxChs}|{ClsRef}|{RepMrk}|{EscChr}|{LitStr}|{ChrCls})

%x RULES
%x UCODE
%x BCODE
%x LCODE
%x INDNT
%x CMMNT
%x SMACT
%x XPEOL
%x REGEX
%x NMLST
%x SPACE
%x VRBTM

%%

<SPACE>{Space}+             { yy_pop_state(); }
<SPACE>{NotWh}+             { Error(78, TokenSpan()); return (int)Tokens.repErr; }
<SPACE>{Eol}                { yy_pop_state(); Error(78, TokenSpan()); return (int)Tokens.EOL; }

<VRBTM>{Space}+             { /* skip */ }
<VRBTM>{DotChr}+            { yy_pop_state(); return (int)Tokens.verbatim; }

/* All the states terminated by EOL */
<XPEOL,VRBTM,LCODE,NMLST,SMACT>{Eol} { 
                                yy_pop_state(); return (int)Tokens.EOL; 
                            }
<XPEOL>{NotWh}{DotChr}*     { Error(80, TokenSpan()); return (int)Tokens.repErr; }

/* In the INITIAL start condition there is nothing to pop */
^{Ident}                  { 
                              yy_push_state(XPEOL); 
                              yy_push_state(REGEX); 
                              yy_push_state(SPACE); 
                              return (int)Tokens.name; 
                          }

^%%                       { yy_clear_stack(); BEGIN(RULES); yy_push_state(XPEOL); return (int)Tokens.PCPC; }
^%{Ident}                 { return (int)GetTagToken(yytext); }
^{CmntStrt}{ABStar}\**          { yy_push_state(CMMNT); return (int)Tokens.defCommentS; }
^{CmntStrt}{ABStar}\**{CmntEnd} { return (int)Tokens.defCommentE; }

/* Code inclusion in either definitions or rules sections */
<0,RULES>^{Space}+/{NotWh}               { yy_push_state(INDNT); return (int)Tokens.lxIndent; }
<0,RULES>^%\{                            { yy_push_state(UCODE); yy_push_state(XPEOL); return (int)Tokens.lPcBrace; }
<UCODE>^%\}                              { yy_pop_state(); yy_push_state(XPEOL); return (int)Tokens.rPcBrace; }
<0,RULES>^%\}                            { return (int)Tokens.rPcBrace; /* error! */ }

/* In the "NameList" start condition */
<NMLST>{Space}+           { /* skip */ }
<NMLST>{Number}           { return (int)Tokens.csNumber; }
<NMLST>\*                 { return (int)Tokens.csStar; }
<NMLST>,                  { return (int)Tokens.comma; }
<NMLST>{Ident}            { return (int)Tokens.name; }
<NMLST>>                  { yy_pop_state(); return (int)Tokens.rCond; }

/* In the regular expression start condition */                                       
<REGEX>{Regex}+         { yy_pop_state(); return (int)Tokens.pattern; }
 
/* In the indented code start condition */
<INDNT>^{NotWh}{DotChr}*                    { yy_pop_state(); yyless(0); return (int)Tokens.lxEndIndent; }

/* In either indented code or user code start conditions */
<INDNT,UCODE,BCODE,LCODE,RULES>{CmntStrt}{ABStar}\**          {yy_push_state(CMMNT); return (int)Tokens.csCommentS; }
<INDNT,UCODE,BCODE,LCODE,RULES>{CmntStrt}{ABStar}\**{CmntEnd} { return (int)Tokens.csCommentE; }
<INDNT,UCODE,BCODE,LCODE>\/\/{DotChr}*                  { return (int)Tokens.csCommentL; }

/* more CSharp tokens ... */
<INDNT,UCODE,BCODE,LCODE>{Ident}                  { return (int)GetIdToken(yytext); }
<INDNT,UCODE,BCODE,LCODE>{Number}                 { return (int)Tokens.csNumber; }
<INDNT,UCODE,BCODE,LCODE>{CShOps}                 |
<INDNT,UCODE,BCODE,LCODE>{Dgrphs}                 { return (int)Tokens.csOp; }
<INDNT,UCODE,BCODE,LCODE>{LitStr}                 { return (int)Tokens.csLitstr; }
<INDNT,UCODE,BCODE,LCODE>'{ChrChs}'               |
<INDNT,UCODE,BCODE,LCODE>'{EscChr}'               |
<INDNT,UCODE,BCODE,LCODE>'\\{OctDig}{3}'          |
<INDNT,UCODE,BCODE,LCODE>'\\x{HexDig}{2}'         { return (int)Tokens.csLitchr; }
<INDNT,UCODE,BCODE,LCODE>\*                       { return (int)Tokens.csStar; }
<INDNT,UCODE,BCODE,LCODE>,                        { return (int)Tokens.comma; }
<INDNT,UCODE,BCODE,LCODE>;                        { return (int)Tokens.semi; }
<INDNT,UCODE,BCODE,LCODE>\.                       { return (int)Tokens.csDot; }
<INDNT,UCODE,BCODE,LCODE>\[                       { return (int)Tokens.csLBrac; }
<INDNT,UCODE,BCODE,LCODE>\]                       { return (int)Tokens.csRBrac; }
<INDNT,UCODE,BCODE,LCODE>\(                       { return (int)Tokens.csLPar; }
<INDNT,UCODE,BCODE,LCODE>\)                       { return (int)Tokens.csRPar; }
<INDNT,UCODE,LCODE>\{                             { return (int)Tokens.csLBrace; }
<INDNT,UCODE,LCODE>\}                             { return (int)Tokens.csRBrace; }
<BCODE>\{                                         { depth++; return (int)Tokens.csLBrace; }
<BCODE>\}                                         {
                                          if (depth > 0) { depth--; return (int)Tokens.csRBrace; }
                                          else           { yy_pop_state(); return (int)Tokens.lxRBrace; }
                                      }

/* Inside a CSharp comment or a LEX comment */
<CMMNT>{ABStar}\**                          { return (int)Tokens.csCommentS; }
<CMMNT>{ABStar}\**{CmntEnd}                 { yy_pop_state(); return (int)Tokens.csCommentE; }

/* Inside the rules section */
<RULES>^<                                   { 
                                                yy_push_state(SMACT);
                                                yy_push_state(SPACE); 
                                                yy_push_state(REGEX); 
                                                yy_push_state(NMLST); 
                                                return (int)Tokens.lCond; 
                                            }
<RULES>^{NotWh}                             { 
                                                yy_push_state(SMACT); 
                                                yy_push_state(SPACE); 
                                                yy_push_state(REGEX); 
                                                yyless(0); 
                                            }
<RULES>"<<EOF>>"                            { yy_push_state(SMACT); yy_push_state(SPACE); return (int)Tokens.pattern; }
<RULES>^%%                                  { yy_clear_stack(); BEGIN(UCODE); yy_push_state(XPEOL); return (int)Tokens.PCPC; }

<SMACT>\|                                   { yy_pop_state(); return (int)Tokens.lxBar; }
<SMACT>\{                                   { yy_pop_state(); yy_push_state(BCODE); depth = 0; return (int)Tokens.lxLBrace; }
<SMACT>{NotWh}                              { yy_pop_state(); yy_push_state(LCODE); yyless(0); }

/* Catch-all EOL actions for productions not otherwise defined */
<*>{Eol}                  { return (int)Tokens.EOL; }

/* Catch all non-whitespace not part of any other token */
<*>{NotWh}                { Error(79, TokenSpan()); return (int)Tokens.repErr; }

%{
        // Epilog from LEX file
		    yylloc = new LexSpan(tokLin, tokCol, tokELin, tokECol, tokPos, tokEPos, buffer);
%}

%%

  /*  User code is in ParseHelper.cs  */
