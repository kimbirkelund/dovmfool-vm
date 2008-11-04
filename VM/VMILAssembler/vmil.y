%partial

%namespace VMILAssembler

%YYSTYPE VMILParser.ASTNode
%YYLTYPE VMILParser.LexLocation

%token Class Extends Fields Handler Default Locals StoreField LoadField StoreLocal LoadLocal PushLiteral Pop NewInstance Return Jump JumpIfTrue JumpIfFalse Throw Try Catch Colon LeftParen RightParen LeftCBrace RightCBrace Identifier String Integer

%start program

%%

program :
	classlist;
	
classlist : 
|	classlist class;

class :
	Class Identifier extendspart LeftCBrace classbody RightCBrace {
		var body = $4.As<ClassBody>();
		$$ = new Class($2.As<string>(), body.Fields, body.DefaultHandler, body.Handlers, body.Classes);
	};
	
classbody :
	fields defaulthandler handlers classes {
		$$ = new ClassBody($1.As<List<Field>>(), $2.As<MessageHandler>(), $3.As<List<MessageHandler>>(), $4.As<List<Class>>());
	}
	
fields : { $$ = new List<Name>(); }
|	Identifier fields { $$ = new Name(@$, $1.As<string>()) + $2.As<List<Name>>(); };

handlers : { $$ = new List<MessageHandler>(); }
|	Handler handler handlers { $$ = $1.As<MessageHandler>() + $2.As<List<MessageHandler>>(); };

handler : Identifier LeftParen identifiers RightParen LeftCBrace handlerbody RightCBrace {
	var body = $6.As<MessageHandlerBody>();
	$$ = new Handler($1.As<string>(), $3.As<List<Name>>(), body.Locals, body.Instructions);
}