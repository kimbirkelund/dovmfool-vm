
// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2007
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Text;
using System.Collections.Generic;


namespace gpcc
{
	public abstract class Symbol
	{
		private string name;
		public string kind;

		public abstract int num
		{
			get;
		}


		public Symbol(string name)
		{
			this.name = name;
		}


		public override string ToString()
		{
			return name;
		}


		public abstract bool IsNullable();
	}


	public class Terminal : Symbol
	{
        static int count = 0;
		static int max = 0;

		public Precedence prec = null;
		private int n;
		public bool symbolic;

		public override int num
		{
			get
			{
				if (symbolic)
					return max + n;
				else
					return n;
			}
		}

		public Terminal(bool symbolic, string name)
            //: base(symbolic ? name : "'" + name.Replace("\n", @"\n") + "'")
        	: base(symbolic ? name : UnEscape(name))
{
			this.symbolic = symbolic;

			if (symbolic)
				this.n = ++count;
			else
			{
				this.n = (int)name[0];
				if (n > max) max = n;
			}
		}


		public override bool IsNullable()
		{
			return false;
		}

        private static string UnEscape(string rep)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\'');
            // for (int i = 0; i < rep.Length; i++)
            foreach (char ch in rep)
            {
                // switch (rep[i])
                switch (ch)
                {
                    case '\a': builder.Append(@"\a"); break;
                    case '\b': builder.Append(@"\b"); break;
                    case '\f': builder.Append(@"\f"); break;
                    case '\n': builder.Append(@"\n"); break;
                    case '\r': builder.Append(@"\r"); break;
                    case '\t': builder.Append(@"\t"); break;
                    case '\v': builder.Append(@"\v"); break;
                    case '\0': builder.Append(@"\0"); break;
                    case '\\': builder.Append(@"\\"); break;
                    default: builder.Append(ch); break;
                }
            }
            builder.Append('\'');
            return builder.ToString();
        }

	}



	public class NonTerminal : Symbol
	{
        public bool reached = false;
        static int count = 0;
		private int n;
		public List<Production> productions = new List<Production>();


		public NonTerminal(string name)
			: base(name)
		{
            n = ++count;
		}

		public override int num
		{
			get
			{
				return -n;
			}
		}

		private object isNullable;
		public override bool IsNullable()
		{
			if (isNullable == null)
			{
				isNullable = false;
				foreach (Production p in productions)
				{
					bool nullable = true;
					foreach (Symbol rhs in p.rhs)
						if (!rhs.IsNullable())
						{
							nullable = false;
							break;
						}
					if (nullable)
					{
						isNullable = true;
						break;
					}
				}
			}

			return (bool)isNullable;
		}
	}
}