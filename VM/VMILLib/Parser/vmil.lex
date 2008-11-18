%namespace VMILLib.Parser
%using System.Text;
%using Sekhmet.Logging;

digit [0-9]
int ([0]|[-]?[1-9][0-9]*)
string \"[^\"\n\r]*\"
identifier [_a-zA-Z][_a-zA-Z0-9]*

%{
	public string SourceFile {get; set;}
%}

%%
%{
%}

<0> "public" return (int) Tokens.Public;
<0> "protected" return (int) Tokens.Protected;
<0> "private" return (int) Tokens.Private;
<0> ".class" return (int) Tokens.Class;
<0> "extends" return (int) Tokens.Extends;
<0> ".handler" return (int) Tokens.Handler;
<0> ".default" return (int) Tokens.Default;
<0> ".locals" return (int) Tokens.Locals;
<0> ".fields" return (int) Tokens.Fields;
<0> ".entrypoint" return (int) Tokens.Entrypoint;

<0> "store-field" return (int) Tokens.StoreField;
<0> "load-field" return (int) Tokens.LoadField;
<0> "store-local" return (int) Tokens.StoreLocal;
<0> "load-local" return (int) Tokens.LoadLocal;
<0> "load-argument" return (int) Tokens.LoadArgument;
<0> "push-literal" return (int) Tokens.PushLiteral;
<0> "pop" return (int) Tokens.Pop;
<0> "dup" return (int) Tokens.Dup;
<0> "new-instance" return (int) Tokens.NewInstance;
<0> "send-message" return (int) Tokens.SendMessage;
<0> "return" return (int) Tokens.Return;
<0> "return-void" return (int) Tokens.ReturnVoid;
<0> "jump" return (int) Tokens.Jump;
<0> "jump-if-true" return (int) Tokens.JumpIfTrue;
<0> "jump-if-false" return (int) Tokens.JumpIfFalse;
<0> "throw" return (int) Tokens.Throw;
<0> ".try" return (int) Tokens.Try;
<0> "catch" return (int) Tokens.Catch;
<0> "{" return (int) Tokens.LeftCBrace;
<0> "}" return (int) Tokens.RightCBrace;
<0> "(" return (int) Tokens.LeftParen;
<0> ")" return (int) Tokens.RightParen;
<0> "," return (int) Tokens.Comma;
<0> ":" return (int) Tokens.Colon;

<0> {identifier} {
	yylval = yytext.ToNode();
	return (int) Tokens.Identifier;
}

<0> {string} {
	yylval = yytext.Substring(1, yytext.Length - 2).ToNode();
	return (int) Tokens.String;
}

<0> {int} {
	yylval = int.Parse(yytext, System.Globalization.CultureInfo.InvariantCulture).ToNode();
	return (int) Tokens.Integer;
}

<0> [\ \t\r\n] {}
<0> //[^\n\r]* {}
<0> . yyerror("Unexpected character: " + yytext);

%{
	yylloc = new LexLocation( SourceFile, tokLin, tokCol, tokELin, tokECol );
%}
%%

public override void yyerror(string msg, params object[] args) {
	Logger.PostError( new PositionedError( string.Format(msg, args), yylloc ) );
}
