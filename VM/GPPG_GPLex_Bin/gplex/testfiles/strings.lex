/*
   A simple example of using GPLEX to implement the unix "strings" 
   functionality.  Reads a (possibly binary) file, finding sequences
   of alphabetic ASCII characters.
 */
   
%namespace LexScanner
%option verbose, summary, noparser

alpha [a-zA-Z]
alphaplus [a-zA-Z\-']
%%
{alpha}{alphaplus}*{alpha}   Console.WriteLine(yytext);
%%
    public static void Main(string[] argp) {
        if (argp.Length == 0)  
            Console.WriteLine("Usage: words filename(s)");
        for (int i = 0; i < argp.Length; i++) {
            string name = argp[i];
            try {
                int tok;
                FileStream file = new FileStream(name, FileMode.Open); 
                Scanner scnr = new Scanner(file);
                Console.WriteLine("File: " + name);
                do {
                    tok = scnr.yylex();
                } while (tok > (int)Tokens.EOF);
            } catch (IOException) {
                Console.WriteLine("File " + name + " not found");
            }
        }
    }
