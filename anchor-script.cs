using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnchorScript
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = @"
            div int a = 5;
            div int b = 10;
            div int c = var(a) + var(b) * 2;
            div float d = 2.5;
            div bool e = var(c) > var(b);
            print<c>;
            print<d>;
            print<e>;
            ";

            Tokenizer tokenizer = new Tokenizer(code);
            Parser parser = new Parser(tokenizer);
            
            while (!tokenizer.IsEnd())
            {
                parser.ParseExpression();
            }
        }
    }

    class Tokenizer
    {
        private readonly string[] tokens;
        private int current = 0;

        public Tokenizer(string code)
        {
            tokens = Regex.Split(code, @"\s+|(?<=\W)|(?=\W)");
        }

        public string GetNextToken()
        {
            if (current < tokens.Length)
            {
                return tokens[current++];
            }
            return null;
        }

        public bool IsEnd()
        {
            return current >= tokens.Length;
        }
    }

    class Parser
    {
        private readonly Tokenizer tokenizer;
        private readonly Dictionary<string, dynamic> variables = new Dictionary<string, dynamic>();

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public void ParseExpression()
        {
            string token = tokenizer.GetNextToken();
            if (token == "div")
            {
                string datatype = tokenizer.GetNextToken();
                string varName = tokenizer.GetNextToken();
                tokenizer.GetNextToken(); // skip "="
                string expr = tokenizer.GetNextToken();
                dynamic value = Evaluate(expr);
                variables[varName] = value;
                Console.WriteLine($"{varName} = {value}");
            }
            else if (token.StartsWith("print<"))
            {
                string expr = token.Substring(6, token.Length - 7);
                dynamic value = Evaluate(expr);
                Console.WriteLine(value);
            }
        }

        private dynamic Evaluate(string expr)
        {
            if (variables.ContainsKey(expr))
            {
                return variables[expr];
            }
            else if (int.TryParse(expr, out int intValue))
            {
                return intValue;
            }
            else if (float.TryParse(expr, out float floatValue))
            {
                return floatValue;
            }
            else if (expr.StartsWith("var(") && expr.EndsWith(")"))
            {
                string varName = expr.Substring(4, expr.Length - 5);
                return variables.ContainsKey(varName) ? variables[varName] : null;
            }
            else
            {
                string[] parts = expr.Split(new char[] { '+', '-', '*', '/' });
                string left = parts[0].Trim();
                string right = parts[1].Trim();
                char op = expr[left.Length];

                dynamic leftValue = Evaluate(left);
                dynamic rightValue = Evaluate(right);

                return op switch
                {
                    '+' => leftValue + rightValue,
                    '-' => leftValue - rightValue,
                    '*' => leftValue * rightValue,
                    '/' => leftValue / rightValue,
                    _ => throw new Exception("Invalid operator")
                };
            }
        }
    }
}
