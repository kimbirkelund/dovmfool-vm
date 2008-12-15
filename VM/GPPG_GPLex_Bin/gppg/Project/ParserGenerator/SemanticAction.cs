// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2007
// (see accompanying GPPGcopyright.rtf)



using System;
using System.Text;


namespace gpcc
{
    public class SemanticAction
    {
        private Production production;
        private int pos;
        private string commands;


        public SemanticAction(Production production, int pos, string commands)
        {
            this.production = production;
            this.pos = pos;
            this.commands = commands;
        }

        void ErrReport(string tag, string msg)
        {
            Console.Error.Write(tag); Console.Error.Write(": ");  Console.Error.WriteLine(msg);
        }


        public void GenerateCode(CodeGenerator codeGenerator)
        {
            int i = 0;
            string lineTag = "";

            if (commands.StartsWith("#line"))
                lineTag = commands.Substring(0, commands.IndexOf('"')).Trim();

            int length = commands.Length;
            while (i < length)
            {
                switch (commands[i])
                {
                    case 'Y':
                        {
                            int j = i;
                            do { j++; } while (j < length && char.IsLetter(commands, j));
                            string substr = commands.Substring(i, j - i);
                            if (substr.Equals("YYACCEPT"))
                            {
                                i = j;
                                Console.Write("YYAccept()");
                            }
                            else if (substr.Equals("YYABORT"))
                            {
                                i = j;
                                Console.Write("YYAbort()");
                            }
                            else if (substr.Equals("YYERROR"))
                            {
                                i = j;
                                Console.Write("YYError()");
                            }
                            else
                                Output(i++);
                            break;
                        }
                    case '/':
                        Output(i++);
                        if (i < length && commands[i] == '/') // C++ style comments
                        {
                            while (i < length && commands[i] != '\n')
                                Output(i++);
                            if (i < length)
                                Output(i++);
                        }
                        else if (i < length && commands[i] == '*') // C style comments
                        {
                            Output(i++);
                            do
                            {
                                while (i < length && commands[i] != '*')
                                    Output(i++);
                                if (i < length)
                                    Output(i++);
                            } while (i < length && commands[i] != '/');
                            if (i < length)
                                Output(i++);
                        }
                        break;
                    case '"':       // start of string literal
                        Output(i++);
                        while (i < length && commands[i] != '"')
                        {
                            if (commands[i] == '\\')
                                Output(i++);
                            if (i < length)
                                Output(i++);
                        }
                        if (i < length)
                            Output(i++);
                        break;

                    case '@':		
                        // Possible start of verbatim string literal
                        // but also possible location marker access
                        if (i + 1 < length)
                        {
                            char la = commands[i + 1]; // lookahead character
                            if (la == '$')
                            {
                                i += 2; // read past '@', '$'
                                Console.Write("yyloc");
                            }
                            else if (Char.IsDigit(la))
                            {
                                i++;
                                int num = (int)commands[i++] - (int)'0';
                                while (i < length && char.IsDigit(commands[i]))
                                    num = num * 10 + (int)commands[i++] - (int)'0';

                                if (num > this.production.rhs.Count)
                                    ErrReport(lineTag, String.Format("Index @{0} is out of bounds", num));
                                Console.Write("location_stack.array[location_stack.top-{0}]", pos - num + 1);
                            }
                            else
                            {
                                Output(i++);
                                if (la == '"')
                                {
                                    Output(i++);
                                    while (i < length && commands[i] != '"')
                                        Output(i++);
                                    if (i < length)
                                        Output(i++);
                                    break;
                                }
                            }
                        }
                        else
                            ErrReport(lineTag, "Invalid use of '@'");
                        break;

                    case '\'':      // start of char literal
                        Output(i++);
                        while (i < length && commands[i] != '\'')
                        {
                            if (commands[i] == '\\')
                                Output(i++);
                            if (i < length)
                                Output(i++);
                        }
                        if (i < length)
                            Output(i++);
                        break;

                    case '$':       // $$ or $n placeholder
                        i++;
                        if (i < length)
                        {
                            string kind = null;
                            if (commands[i] == '<') // $<kind>n
                            {
                                i++;
                                StringBuilder builder = new StringBuilder();
                                while (i < length && commands[i] != '>')
                                {
                                    builder.Append(commands[i]);
                                    i++;
                                }
                                if (i < length)
                                {
                                    i++;
                                    kind = builder.ToString();
                                }
                            }

                            if (commands[i] == '$')
                            {
                                i++;
                                if (kind == null)
                                    kind = production.lhs.kind;

                                Console.Write("yyval");

                                if (kind != null)
                                    Console.Write(".{0}", kind);
                            }
                            else if (char.IsDigit(commands[i]))
                            {
                                int num = (int)commands[i++] - (int)'0';
                                while (i < length && char.IsDigit(commands[i]))
                                    num = num * 10 + (int)commands[i++] - (int)'0';

                                if (num > this.production.rhs.Count)
                                    ErrReport(lineTag, String.Format("Index ${0} is out of bounds", num));
                                
                                if (kind == null)
                                    kind = production.rhs[num - 1].kind;

                                Console.Write("value_stack.array[value_stack.top-{0}]", pos - num + 1);

                                if (kind != null)
                                    Console.Write(".{0}", kind);
                            }
                        }
                        else
                            ErrReport(lineTag, "Unexpected '$'");
                        break;

                    default:
                        Output(i++);
                        break;
                }
            }
            Console.WriteLine();
        }


        private void Output(int i)
        {
            if (commands[i] == '\n')
                Console.WriteLine();
            else
                Console.Write(commands[i]);
        }
    }
}