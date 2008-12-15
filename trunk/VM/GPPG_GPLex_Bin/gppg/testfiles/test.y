%{
    int[] regs = new int[26];
    int _base;
%}

%start list

%token DIGIT LETTER

%left '|'
%left '&'
%left '+' '-'
%left '*' '/' '%'
%left UMINUS

%%

list    :   /*empty */
        |   list stat '\n'
        |   list error '\n'
                {
                    yyerrok();
                }
        |   list '\n' stat '\n' { /* skip */ }
        ;


stat    :   expr
                {
                    System.Console.WriteLine($1);
                }
        |   LETTER '=' expr
                {
                    regs[$1] = $3;
                }
        ;

expr    :   '(' expr ')'
                {
                    $$ = $2;
                }
        |   expr '*' expr
                {
                    $$ = $1 * $3;
                }
        |   expr '/' expr
                {
                    $$ = $1 / $3;
                }
        |   expr '%' expr
                {
                    $$ = $1 % $3;
                }
        |   expr '+' expr
                {
                    $$ = $1 + $3;
                }
        |   expr '-' expr
                {
                    $$ = $1 - $3;
                }
        |   expr '&' expr
                {
                    $$ = $1 & $3;
                }
        |   expr '|' expr
                {
                    $$ = $1 | $3;
                }
        |   '-' expr %prec UMINUS
                {
                    $$ = -$2;
                }
        |   LETTER
                {
                    $$ = regs[$1];
                }
        |   number
        ;

number  :   DIGIT
                {
                    $$ = $1;
                    _base = ($1==0) ? 8 : 10;
                }
        |   number DIGIT
                {
                    $$ = _base * $1 + $2;
                }
        ;

%%

static void Main(string[] args)
{
    Parser parser = new Parser();
    
    System.IO.TextReader reader;
    if (args.Length > 0)
        reader = new System.IO.StreamReader(args[0]);
    else
        reader = System.Console.In;
        
    parser.scanner = new Scanner(reader);
    //parser.Trace = true;
    
    parser.Parse();
}

class Scanner: gppg.IScanner<int,LexLocation>
{
    private System.IO.TextReader reader;

    public Scanner(System.IO.TextReader reader)
    {
        this.reader = reader;
    }

    public override int yylex()
    {
        char ch = (char) reader.Read();

        if (ch == '\n')
            return ch;
        else if (char.IsWhiteSpace(ch))
            return yylex();
        else if (char.IsDigit(ch))
        {
            yylval = ch - '0';
            return (int)Tokens.DIGIT;
        }
        else if (char.IsLetter(ch))
        {
            yylval = char.ToLower(ch) - 'a';
            return (int)Tokens.LETTER;
        }
        else
            switch (ch)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '(':
                case ')':
                case '%':
                case '|':
                case '&':
                case '=':
                    return ch;
                default:
                    Console.Error.WriteLine("Illegal character '{0}'", ch);
                    return yylex();
            }
    }

    public override void yyerror(string format, params object[] args)
    {
        Console.Error.WriteLine(format, args);
    }
}
