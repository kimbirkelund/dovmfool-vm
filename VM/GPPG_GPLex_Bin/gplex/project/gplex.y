/*
 *  Parser spec for GPLEX
 *  Process with > GPPG /gplex /no-lines gplex.y > Parser.cs
 */
 
%using System.Collections
%namespace GPLEX.Parser

%YYLTYPE LexSpan
%partial

%token csKeyword csIdent csNumber csLitstr csLitchr csOp csBar csDot semi
%token csStar csLT csGT comma csSlash csLBrac csRBrac csLPar csRPar csLBrace csRBrace
%token verbatim pattern name lCond rCond lxLBrace lxRBrace lxBar
%token defCommentS defCommentE csCommentL csCommentS csCommentE
%token usingTag namespaceTag optionTag
%token inclTag exclTag lPcBrace rPcBrace PCPC lxIndent lxEndIndent

%token maxParseToken EOL errTok repErr
  
%%

Program
    : DefinitionSection RulesSection UserCodeSection
    ;
    
DefinitionSection
    : DefinitionSeq  PCPC    { 
              typedeclOK = false;
              if (aast.nameString == null) handler.ListError(@2, 73); 
            }
    | PCPC
    | error PCPC { handler.ListError(@1, 62, "%%"); }
    ;
 
RulesSection
    : Rules PCPC     { typedeclOK = true; }
    | Rules 
    | error PCPC     { handler.ListError(@1, 62, "%%"); typedeclOK = true; }
    | error          { handler.ListError(@1, 62, "EOF"); }
    ;
    
UserCodeSection
    : CSharp         { aast.UserCode = @1; }
    | /* empty */    {  /* empty */  }
    | error          { handler.ListError(@1, 62, "EOF"); }
    ;
    
DefinitionSeq
    : DefinitionSeq Definition
    | Definition
    ;

Definition
    : name verbatim            { AddLexCategory(@1, @2); }
    | name pattern             { AddLexCategory(@1, @2); }
    | exclTag NameList         { AddNames(true); }
    | inclTag NameList         { AddNames(false); }
    | usingTag DottedName semi { aast.usingStrs.Add(@2.Merge(@3)); }
    | namespaceTag DottedName  { aast.nameString = @2; }
    | optionTag verbatim       { ParseOption(@2); }
    | PcBraceSection           { aast.AddCodeSpan(Dest,@1); }
    | DefComment               { aast.AddCodeSpan(Dest,@1); }
    | IndentedCode             { aast.AddCodeSpan(Dest,@1); }
    ;
    

IndentedCode
    : lxIndent CSharp lxEndIndent             { @$ = @2; }
    | lxIndent error lxEndIndent              { handler.ListError(@2, 64); }
    ;

NameList
    : NameSeq
    | error                                  { handler.ListError(@1, 67); }
    ;

NameSeq
    : NameSeq comma name                     { AddName(@3); }
    | name                                   { AddName(@1); }
    | csNumber                               { AddName(@1); }
    | NameSeq comma error                    { handler.ListError(@2, 67); }
    ;

PcBraceSection
    : lPcBrace rPcBrace                 { @$ = Blank; /* skip blank lines */ }
    | lPcBrace CSharpN rPcBrace         { @$ = @2; }
    | lPcBrace error rPcBrace           { handler.ListError(@2, 62, "%}"); }
    | lPcBrace error PCPC               { handler.ListError(@2, 62, "%%"); }
    ;
    

DefComment
    : DefComStart CommentEnd
    | defCommentE                     
    | DefComStart error               { handler.ListError(@1, 60); }
    ;
    
CommentEnd
    : defCommentE
    | csCommentE
    ;

DefComStart
    : DefComStart defCommentS
    | DefComStart csCommentS
    | defCommentS                     
    ;
    
BlockComment
    : CsComStart CommentEnd
    | CsComStart error                { handler.ListError(@1, 60); }
    ;

CsComStart
    : CsComStart csCommentS 
    | CsComStart defCommentS 
    | csCommentS                      
    ;
    
Rules
    : RuleList     { rb.FinalizeCode(aast); aast.FixupBarActions(); }
    | /* empty */ 
    ;
    
RuleList
    : RuleList Rule
    | Rule
    ;
    
Rule
    : PcBraceSection                    { rb.AddSpan(@1); }
    | Production
    | IndentedCode                      { rb.AddSpan(@1); }
    | BlockComment                      { /* ignore */ }
    | csCommentE                        { /* ignore */ }
    ;
    
Production
    :  ListInit ARule           {
			int thisLine = @2.sLin;
			rb.LLine = thisLine;
			if (rb.FLine == 0) rb.FLine = thisLine;
		  }
    ;
          
ARule                                          
    : StartCondition pattern Action  {
			RuleDesc rule = new RuleDesc(@2, @3, scList, isBar);
			aast.ruleList.Add(rule);
			rule.ParseRE(aast);
		  }
    | pattern Action               {
			RuleDesc rule; 
			// scList.Add(StartState.initState);
			// rule = new RuleDesc(@1, @2, scList, isBar);
			rule = new RuleDesc(@1, @2, null, isBar); 
			aast.ruleList.Add(rule);
			rule.ParseRE(aast); 
		  }
    | error                    { handler.ListError(@1, 68); }
    ;
    
ListInit
    : /* empty */              { scList = new List<StartState>(); isBar = false; }
    ; 
    
StartCondition
    : lCond NameList rCond          { CheckScList(scList); }
    | lCond csStar rCond            { scList.Add(StartState.allState); }
    | lCond error rCond             { handler.ListError(@$, 67); }
    ;

CSharp
    : CSharpN                      
    ;

CSharpN
    : CSharpN WFCSharpN
    | WFCSharpN                     
    ;

WFCSharpN
    : NonPairedToken                   
    | csLBrace csRBrace
    | csLBrace CSharpN csRBrace
    | csLPar csRPar
    | csLPar CSharpN csRPar
    | csLBrac csRBrac
    | csLBrac CSharpN csRBrac
    | csLPar error                     { handler.ListError(@2, 61, "')'"); }
    | csLBrac error                    { handler.ListError(@2, 61, "']'"); }
    | csLBrace error                   { handler.ListError(@2, 61, "'}'"); }
    ;
    
DottedName
    : csIdent                          { /* skip1 */ }
    | DottedName csDot csIdent         { /* skip2 */ }
    ;

NonPairedToken
    : BlockComment                     
    | DottedName                              
    | csCommentE                       
    | csCommentL                       
    | csKeyword                        { 
        string text = aast.scanner.yytext;
        if (text.Equals("using")) {
            handler.ListError(@1, 56);
        } else if (text.Equals("namespace")) {
            handler.ListError(@1, 57);
        } else {
            if ((text.Equals("class") || text.Equals("struct") ||
                 text.Equals("enum")) && !typedeclOK) handler.ListError(@1,58);
        }
      }
    | csNumber                             
    | csLitstr                             
    | csLitchr                             
    | csOp
    | csDot                                  
    | csStar                                
    | csLT                                  
    | csGT
    | semi                                 
    | comma                                 
    | csSlash                               
    | csBar                                 
    ;
    
Action
    : lxLBrace CSharp lxRBrace         { @$ = @2; }
    | CSharp                           
    | lxBar                            { isBar = true; }
    | lxLBrace lxRBrace
    | lxLBrace error lxRBrace          { handler.ListError(@$, 65); }
    | error                            { handler.ListError(@1, 63); }
    ;
    
%%















