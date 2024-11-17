import re

class Tokenizer:
    def __init__(self, code):
        self.tokens = re.findall(r'\bdiv\s+\w+\s+\w+\s*=\s*[^\s]+|var\(\w+\)|[\+\-\*\/\(\);]|==|!=|<=|>=|<|>', code)
        self.current = 0

    def get_next_token(self):
        if self.current < len(self.tokens):
            token = self.tokens[self.current]
            self.current += 1
            return token
        return None

class Parser:
    def __init__(self, tokenizer):
        self.tokenizer = tokenizer
        self.variables = {}

    def parse_expression(self):
        token = self.tokenizer.get_next_token()
        if token is None:
            return None

        if token.startswith("div"):
            datatype, var_name, expr = re.findall(r'div\s+(\w+)\s+(\w+)\s*=\s*(.+)', token)[0]
            value = self.evaluate(expr)
            self.variables[var_name] = value
            return (datatype, var_name, value)

        elif token.startswith("var("):
            var_name = re.findall(r'var\((\w+)\)', token)[0]
            return var_name

        elif token.isdigit() or re.match(r'\d+\.\d+', token):
            return float(token) if '.' in token else int(token)

        elif token in self.variables:
            return self.variables[token]

        elif token in '+-*/':
            left = self.parse_expression()
            right = self.parse_expression()
            return (token, left, right)

        elif token in '== != <= >= < >':
            left = self.parse_expression()
            right = self.parse_expression()
            return (token, left, right)

        elif token == '(':
            expr = self.parse_expression()
            self.tokenizer.get_next_token()  # consume ')'
            return expr

    def evaluate(self, node):
        if isinstance(node, (int, float)):
            return node
        elif isinstance(node, str):
            return self.variables.get(node, node)
        elif isinstance(node, tuple):
            if node[0] in '+-*/':
                op, left, right = node
                left = self.evaluate(left)
                right = self.evaluate(right)
                if op == '+':
                    return left + right
                elif op == '-':
                    return left - right
                elif op == '*':
                    return left * right
                elif op == '/':
                    return left / right
            elif node[0] in '== != <= >= < >':
                op, left, right = node
                left = self.evaluate(left)
                right = self.evaluate(right)
                if op == '==':
                    return left == right
                elif op == '!=':
                    return left != right
                elif op == '<=':
                    return left <= right
                elif op == '>=':
                    return left >= right
                elif op == '<':
                    return left < right
                elif op == '>':
                    return left > right

def main():
    code = """
    div int a = 5;
    div int b = 10;
    div int c = var(a) + var(b) * 2;
    div float d = 2.5;
    div bool e = var(c) > var(b);
    print<c>;
    print<d>;
    print<e>;
    """
    tokenizer = Tokenizer(code)
    parser = Parser(tokenizer)

    while tokenizer.current < len(tokenizer.tokens):
        parser.parse_expression()

if __name__ == "__main__":
    main()

