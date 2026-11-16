using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace CrucibleAI.Json;

internal sealed class JsonTextParser
{
    private const char ByteOrderMark = '\uFEFF';
    private const int MaxDepth = 512;
    private const string TrueLiteral = "true";
    private const string FalseLiteral = "false";
    private const string NullLiteral = "null";
    private const string BlockCommentEnd = "*/";

    private readonly string _text;
    private readonly JsonDialect _dialect;
    private int _position;
    private int _depth;

    private JsonTextParser(string text, JsonDialect dialect)
    {
        _text = text;
        _dialect = dialect;
    }

    public static JsonNode? Parse(string text, JsonDialect dialect) => new JsonTextParser(text, dialect).Document();

    private bool IsJsonc => _dialect == JsonDialect.Jsonc;

    private bool AllowsTrailingCommas => _dialect != JsonDialect.Strict;

    private JsonNode? Document()
    {
        if (IsJsonc && _text.Length > 0 && _text[0] == ByteOrderMark)
        {
            _position = 1;
        }
        SkipInsignificant();
        var value = Value();
        SkipInsignificant();
        return _position == _text.Length ? value : throw Failure();
    }

    private JsonNode? Value() => Current switch
    {
        '{' => Nested(Object),
        '[' => Nested(Array),
        '"' => JsonValue.Create(String()),
        't' => Literal(TrueLiteral, JsonValue.Create(true)),
        'f' => Literal(FalseLiteral, JsonValue.Create(false)),
        'n' => Literal(NullLiteral, null),
        '-' or (>= '0' and <= '9') => Number(),
        _ => throw Failure(),
    };

    private JsonNode Nested(Func<JsonNode> container)
    {
        if (++_depth > MaxDepth)
        {
            throw Failure();
        }
        var node = container();
        _depth--;
        return node;
    }

    private JsonObject Object()
    {
        var members = new JsonObject();
        _position++;
        SkipInsignificant();
        if (Current == '}')
        {
            _position++;
            return members;
        }
        while (true)
        {
            if (Current != '"')
            {
                throw Failure();
            }
            var key = String();
            SkipInsignificant();
            Expect(':');
            SkipInsignificant();
            members[key] = Value();
            if (ClosesAfterValue('}'))
            {
                return members;
            }
        }
    }

    private JsonArray Array()
    {
        var items = new JsonArray();
        _position++;
        SkipInsignificant();
        if (Current == ']')
        {
            _position++;
            return items;
        }
        while (true)
        {
            items.Add(Value());
            if (ClosesAfterValue(']'))
            {
                return items;
            }
        }
    }

    private bool ClosesAfterValue(char closer)
    {
        SkipInsignificant();
        if (Current == closer)
        {
            _position++;
            return true;
        }
        Expect(',');
        SkipInsignificant();
        if (AllowsTrailingCommas && Current == closer)
        {
            _position++;
            return true;
        }
        return false;
    }

    private string String()
    {
        var text = new StringBuilder();
        _position++;
        while (true)
        {
            var current = Current ?? throw Failure();
            _position++;
            switch (current)
            {
                case '"':
                    return text.ToString();
                case '\\':
                    text.Append(Escape());
                    break;
                case < ' ':
                    throw Failure();
                default:
                    text.Append(current);
                    break;
            }
        }
    }

    private char Escape()
    {
        var escape = Current ?? throw Failure();
        _position++;
        return escape switch
        {
            '"' => '"',
            '\\' => '\\',
            '/' => '/',
            'b' => '\b',
            'f' => '\f',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            'u' => Unicode(),
            _ => throw Failure(),
        };
    }

    private char Unicode()
    {
        if (_position + 4 > _text.Length
            || !ushort.TryParse(_text.AsSpan(_position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var unit))
        {
            throw Failure();
        }
        _position += 4;
        return (char)unit;
    }

    private JsonValue Number()
    {
        var start = _position;
        Accept('-');
        if (!Accept('0'))
        {
            RequireDigits();
        }
        if (Accept('.'))
        {
            RequireDigits();
        }
        if (Accept('e') || Accept('E'))
        {
            if (!Accept('+'))
            {
                Accept('-');
            }
            RequireDigits();
        }
        var number = double.Parse(_text.AsSpan(start, _position - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        if (IsJsonc && double.IsInfinity(number))
        {
            throw Failure();
        }
        return JsonValue.Create(number);
    }

    private void RequireDigits()
    {
        if (Current is not (>= '0' and <= '9'))
        {
            throw Failure();
        }
        while (Current is >= '0' and <= '9')
        {
            _position++;
        }
    }

    private JsonNode? Literal(string literal, JsonNode? value)
    {
        if (string.CompareOrdinal(_text, _position, literal, 0, literal.Length) != 0)
        {
            throw Failure();
        }
        _position += literal.Length;
        return value;
    }

    private void SkipInsignificant()
    {
        while (Current is { } current)
        {
            if (current is ' ' or '\t' or '\n' or '\r')
            {
                _position++;
            }
            else if (IsJsonc && current == '/' && Next == '/')
            {
                SkipLineComment();
            }
            else if (IsJsonc && current == '/' && Next == '*')
            {
                SkipBlockComment();
            }
            else
            {
                return;
            }
        }
    }

    private void SkipLineComment()
    {
        _position += 2;
        while (Current is not (null or '\r' or '\n'))
        {
            _position++;
        }
    }

    private void SkipBlockComment()
    {
        var end = _text.IndexOf(BlockCommentEnd, _position + 2, StringComparison.Ordinal);
        if (end < 0)
        {
            throw Failure();
        }
        _position = end + BlockCommentEnd.Length;
    }

    private void Expect(char expected)
    {
        if (!Accept(expected))
        {
            throw Failure();
        }
    }

    private bool Accept(char expected)
    {
        if (Current != expected)
        {
            return false;
        }
        _position++;
        return true;
    }

    private char? Current => _position < _text.Length ? _text[_position] : null;

    private char? Next => _position + 1 < _text.Length ? _text[_position + 1] : null;

    private JsonSyntaxException Failure()
    {
        var before = _text.AsSpan(0, Math.Min(_position, _text.Length));
        return new(1 + before.Count('\n') + before.Count('\r') - before.Count("\r\n"));
    }
}
