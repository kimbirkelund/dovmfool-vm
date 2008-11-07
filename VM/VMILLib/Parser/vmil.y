%partial

%namespace VMILLib.Parser

%YYSTYPE VMILLib.Parser.ASTNode
%YYLTYPE VMILLib.Parser.LexLocation

%token Class Extends Fields Handler Default Locals StoreField LoadField StoreLocal LoadLocal PushLiteral Pop NewInstance SendMessage Return Jump JumpIfTrue JumpIfFalse Throw Try Catch Colon LeftParen RightParen LeftCBrace RightCBrace Comma Identifier String Integer Public Private Protected

%start program

%{
	public List<Class> Classes {get; private set;}
%}
%%

program :
	classlist { Classes = (List<Class>) $$; };
	
classlist : 
	{ $$ = new List<Class>(); }
|	class classlist { $$ = (Class) $1 + (List<Class>) $2; };

class : 
	Class visibility Identifier extendspart LeftCBrace classbody RightCBrace {
		var body = (ClassBody) $6;
		$$ = new Class(@$, $2.As<VisibilityModifier>(), $3.As<string>(), (List<string>) $4, body.Fields, body.DefaultHandler, body.Handlers, body.Classes);
	};
	
classbody :
	fields defaulthandler handlers classlist {
		$$ = new ClassBody((List<string>) $1, (MessageHandler) $2, (List<MessageHandler>) $3, (List<Class>) $4);
	};

fields : 
	{ $$ = new List<string>(); }
|	Fields LeftCBrace names RightCBrace { $$ = $3; };

extendspart : 
	{ $$ = new List<string>(); }
|	Extends names { $$ = $2; };

handlers : 
	{ $$ = new List<MessageHandler>(); }
|	Handler handler handlers { $$ = (MessageHandler) $2 + (List<MessageHandler>) $3; };

handler : 
	visibility Identifier LeftParen names RightParen LeftCBrace locals instructions RightCBrace {
		$$ = new MessageHandler(@$, 
				$1.As<VisibilityModifier>(), 
				$2.As<string>(), 
				(List<string>) $4, 
				(List<string>) $7, 
				(List<Instruction>) $8);
	};
	
defaulthandler :
	{ $$ = null; }
|	Default LeftCBrace locals instructions RightCBrace {
		$$ = new MessageHandler(@$, 
				VisibilityModifier.Private, 
				null, 
				new List<string>(), 
				(List<string>) $3, 
				(List<Instruction>) $4);
	};
	
locals : 
	{ $$ = new List<string>(); }
|	Locals LeftCBrace names RightCBrace { $$ = $3; };

names : 
	{ $$ = new List<string>(); }
|	Identifier names2 { $$ = $1.As<string>() + (List<string>) $2; };

names2 :
	{ $$ = new List<string>(); }
|	Comma Identifier names2 { $$ = $2.As<string>() + (List<string>) $3; };

instructions :
	{ $$ = new List<Instruction>(); }
|	Identifier Colon instructions { $$ = new Label(@$, $1.As<string>()) + (List<Instruction>) $3; }
|	StoreField Identifier instructions { $$ = new Instruction(@$, OpCode.StoreField, $2.As<string>()) + (List<Instruction>) $3; }
|	LoadField Identifier instructions { $$ = new Instruction(@$, OpCode.LoadField, $2.As<string>()) + (List<Instruction>) $3; }
|	StoreLocal Identifier instructions { $$ = new Instruction(@$, OpCode.StoreLocal, $2.As<string>()) + (List<Instruction>) $3; }
|	LoadLocal Identifier instructions { $$ = new Instruction(@$, OpCode.LoadLocal, $2.As<string>()) + (List<Instruction>) $3; }
|	PushLiteral Integer instructions { $$ = new Instruction(@$, OpCode.PushLiteral, $2.As<int>()) + (List<Instruction>) $3; }
|	PushLiteral String instructions { $$ = new Instruction(@$, OpCode.PushLiteral, $2.As<string>()) + (List<Instruction>) $3; }
|	Pop instructions { $$ = new Instruction(@$, OpCode.Pop) + (List<Instruction>) $2; }
|	NewInstance instructions { $$ = new Instruction(@$, OpCode.NewInstance) + (List<Instruction>) $2; }
|	SendMessage instructions { $$ = new Instruction(@$, OpCode.SendMessage) + (List<Instruction>) $2; }
|	Return instructions { $$ = new Instruction(@$, OpCode.Return) + (List<Instruction>) $2; }
|	Jump Identifier instructions { $$ = new Instruction(@$, OpCode.Jump, $1.As<string>()) + (List<Instruction>) $3; }
|	JumpIfTrue Identifier instructions { $$ = new Instruction(@$, OpCode.JumpIfTrue, $1.As<string>()) + (List<Instruction>) $3; }
|	JumpIfFalse Identifier instructions { $$ = new Instruction(@$, OpCode.JumpIfFalse, $1.As<string>()) + (List<Instruction>) $3; }
|	Throw instructions { $$ = new Instruction(@$, OpCode.Catch, $1.As<string>()) + (List<Instruction>) $2; }
|	Try LeftCBrace instructions RightCBrace catch instructions { 
		$$ = new Instruction(@$, OpCode.Try, $1.As<string>()) + (List<Instruction>) $3 + (Instruction) $5 
			+ new Instruction(new LexLocation(), OpCode.EndTryCatch) + (List<Instruction>) $6;
	};

catch : Catch LeftParen Identifier RightParen LeftCBrace instructions RightCBrace { 
		$$ = new Instruction(@$, OpCode.Catch, $3.As<string>()) + (List<Instruction>) $6;
	};

visibility :
	Public { $$ = VisibilityModifier.Public.ToNode(); }
|	Protected { $$ = VisibilityModifier.Protected.ToNode(); }
|	Private { $$ = VisibilityModifier.Private.ToNode(); };