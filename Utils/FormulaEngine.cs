using System.Globalization;

public static class FormulaEngine
{
    public readonly struct Value
    {
        public double Scalar { get; }
        public IReadOnlyList<double>? Vector { get; }
        public bool IsVector => Vector != null;

        public Value(double scalar)
        {
            Scalar = scalar;
            Vector = null;
        }

        public Value(IReadOnlyList<double> vector)
        {
            Vector = vector;
            Scalar = vector.Count == 0 ? 0 : vector.Average();
        }

        public IReadOnlyList<double> AsVector() => Vector ?? new[] { Scalar };
        public double AsScalar() => IsVector ? (Vector!.Count == 0 ? 0 : Vector.Average()) : Scalar;
    }

    public static double EvaluateToScalar(string? formula, Dictionary<string, Value> variables, double fallback)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return fallback;

        try
        {
            string expr = NormalizeFormula(formula);
            var parser = new Parser(expr, variables);
            return parser.ParseExpression().AsScalar();
        }
        catch
        {
            return fallback;
        }
    }

    private static string NormalizeFormula(string formula)
    {
        string expr = formula.Trim();
        int assignIndex = expr.IndexOf('=');
        if (assignIndex >= 0)
            expr = expr[(assignIndex + 1)..].Trim();

        return expr;
    }

    private sealed class Parser
    {
        private readonly string _s;
        private readonly Dictionary<string, Value> _vars;
        private int _i;

        public Parser(string s, Dictionary<string, Value> vars)
        {
            _s = s;
            _vars = vars;
        }

        public Value ParseExpression()
        {
            var value = ParseTerm();
            while (true)
            {
                Skip();
                if (Match('+')) value = Add(value, ParseTerm());
                else if (Match('-')) value = Sub(value, ParseTerm());
                else break;
            }
            return value;
        }

        private Value ParseTerm()
        {
            var value = ParseFactor();
            while (true)
            {
                Skip();
                if (Match('*')) value = Mul(value, ParseFactor());
                else if (Match('/')) value = Div(value, ParseFactor());
                else break;
            }
            return value;
        }

        private Value ParseFactor()
        {
            Skip();
            if (Match('+')) return ParseFactor();
            if (Match('-')) return Mul(new Value(-1d), ParseFactor());

            if (Match('('))
            {
                var inner = ParseExpression();
                Match(')');
                return inner;
            }

            if (char.IsDigit(Current()) || Current() == '.')
                return new Value(ParseNumber());

            if (char.IsLetter(Current()) || Current() == '_')
            {
                string name = ParseIdentifier();
                Skip();
                if (Match('('))
                {
                    var args = new List<Value>();
                    Skip();
                    if (!Match(')'))
                    {
                        do { args.Add(ParseExpression()); Skip(); } while (Match(','));
                        Match(')');
                    }
                    return Call(name, args);
                }

                if (_vars.TryGetValue(name, out var value))
                    return value;

                return new Value(0d);
            }

            return new Value(0d);
        }

        private Value Call(string name, List<Value> args)
        {
            if (name.Equals("sum", StringComparison.OrdinalIgnoreCase))
                return new Value(args.SelectMany(a => a.AsVector()).Sum());

            if (name.Equals("avg", StringComparison.OrdinalIgnoreCase) || name.Equals("average", StringComparison.OrdinalIgnoreCase))
            {
                var values = args.SelectMany(a => a.AsVector()).ToList();
                return new Value(values.Count == 0 ? 0 : values.Average());
            }

            if (name.Equals("median", StringComparison.OrdinalIgnoreCase))
            {
                var values = args.SelectMany(a => a.AsVector()).OrderBy(x => x).ToList();
                if (values.Count == 0) return new Value(0d);
                int mid = values.Count / 2;
                return new Value(values.Count % 2 == 0 ? (values[mid - 1] + values[mid]) / 2d : values[mid]);
            }

            if (name.Equals("min", StringComparison.OrdinalIgnoreCase))
            {
                var values = args.SelectMany(a => a.AsVector()).ToList();
                return new Value(values.Count == 0 ? 0 : values.Min());
            }

            if (name.Equals("max", StringComparison.OrdinalIgnoreCase))
            {
                var values = args.SelectMany(a => a.AsVector()).ToList();
                return new Value(values.Count == 0 ? 0 : values.Max());
            }

            if (name.Equals("count", StringComparison.OrdinalIgnoreCase))
                return new Value(args.SelectMany(a => a.AsVector()).Count());

            if (name.Equals("clamp", StringComparison.OrdinalIgnoreCase) && args.Count >= 3)
            {
                double v = args[0].AsScalar();
                double lo = args[1].AsScalar();
                double hi = args[2].AsScalar();
                if (hi < lo) (lo, hi) = (hi, lo);
                return new Value(Math.Clamp(v, lo, hi));
            }

            return new Value(0d);
        }

        private static Value Add(Value a, Value b) => Broadcast(a, b, (x, y) => x + y);
        private static Value Sub(Value a, Value b) => Broadcast(a, b, (x, y) => x - y);
        private static Value Mul(Value a, Value b) => Broadcast(a, b, (x, y) => x * y);
        private static Value Div(Value a, Value b) => Broadcast(a, b, (x, y) => Math.Abs(y) < 1e-9 ? 0 : x / y);

        private static Value Broadcast(Value a, Value b, Func<double, double, double> op)
        {
            if (!a.IsVector && !b.IsVector)
                return new Value(op(a.AsScalar(), b.AsScalar()));

            var av = a.AsVector();
            var bv = b.AsVector();
            int len = Math.Max(av.Count, bv.Count);
            var outv = new double[len];
            for (int i = 0; i < len; i++)
            {
                double x = av.Count == 1 ? av[0] : (i < av.Count ? av[i] : 0);
                double y = bv.Count == 1 ? bv[0] : (i < bv.Count ? bv[i] : 0);
                outv[i] = op(x, y);
            }
            return new Value(outv);
        }

        private string ParseIdentifier()
        {
            int start = _i;
            while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_')) _i++;
            return _s[start.._i];
        }

        private double ParseNumber()
        {
            int start = _i;
            while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.')) _i++;
            return double.TryParse(_s[start.._i], NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0;
        }

        private char Current() => _i < _s.Length ? _s[_i] : '\0';
        private void Skip() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }
        private bool Match(char c)
        {
            Skip();
            if (Current() != c) return false;
            _i++;
            return true;
        }
    }
}
