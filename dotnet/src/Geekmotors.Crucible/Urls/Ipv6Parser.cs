using System.Globalization;

namespace CrucibleAI.Urls;

internal sealed class Ipv6Parser
{
    private const int Pieces = 8;

    public string? Parse(string input)
    {
        var address = new ushort[Pieces];
        var pieceIndex = 0;
        int? compress = null;
        var pointer = 0;
        char? At(int index) => index < input.Length ? input[index] : null;

        if (At(pointer) == ':')
        {
            if (At(pointer + 1) != ':')
            {
                return null;
            }
            pointer += 2;
            pieceIndex++;
            compress = pieceIndex;
        }
        while (At(pointer) is { } current)
        {
            if (pieceIndex == Pieces)
            {
                return null;
            }
            if (current == ':')
            {
                if (compress is not null)
                {
                    return null;
                }
                pointer++;
                pieceIndex++;
                compress = pieceIndex;
                continue;
            }
            var value = 0;
            var length = 0;
            while (length < 4 && At(pointer) is { } hex && char.IsAsciiHexDigit(hex))
            {
                value = (value * 0x10) + Convert.ToInt32(hex.ToString(), 16);
                pointer++;
                length++;
            }
            if (At(pointer) == '.')
            {
                if (length == 0)
                {
                    return null;
                }
                pointer -= length;
                if (pieceIndex > Pieces - 2 || !ReadIpv4(input, ref pointer, address, ref pieceIndex))
                {
                    return null;
                }
                break;
            }
            if (At(pointer) == ':')
            {
                pointer++;
                if (At(pointer) is null)
                {
                    return null;
                }
            }
            else if (At(pointer) is not null)
            {
                return null;
            }
            address[pieceIndex] = (ushort)value;
            pieceIndex++;
        }
        if (compress is { } start)
        {
            var swaps = pieceIndex - start;
            pieceIndex = Pieces - 1;
            while (pieceIndex != 0 && swaps > 0)
            {
                (address[pieceIndex], address[start + swaps - 1]) = (address[start + swaps - 1], address[pieceIndex]);
                pieceIndex--;
                swaps--;
            }
        }
        else if (pieceIndex != Pieces)
        {
            return null;
        }
        return Serialize(address);
    }

    private static bool ReadIpv4(string input, ref int pointer, ushort[] address, ref int pieceIndex)
    {
        var numbersSeen = 0;
        while (pointer < input.Length)
        {
            int? piece = null;
            if (numbersSeen > 0)
            {
                if (input[pointer] != '.' || numbersSeen >= 4)
                {
                    return false;
                }
                pointer++;
            }
            if (pointer >= input.Length || !char.IsAsciiDigit(input[pointer]))
            {
                return false;
            }
            while (pointer < input.Length && char.IsAsciiDigit(input[pointer]))
            {
                var number = input[pointer] - '0';
                if (piece is null)
                {
                    piece = number;
                }
                else if (piece == 0)
                {
                    return false;
                }
                else
                {
                    piece = (piece * 10) + number;
                }
                if (piece > byte.MaxValue)
                {
                    return false;
                }
                pointer++;
            }
            address[pieceIndex] = (ushort)((address[pieceIndex] * 0x100) + piece!.Value);
            numbersSeen++;
            if (numbersSeen is 2 or 4)
            {
                pieceIndex++;
            }
        }
        return numbersSeen == 4;
    }

    private static string Serialize(ushort[] address)
    {
        var (compressStart, compressLength) = LongestZeroRun(address);
        var output = new System.Text.StringBuilder("[");
        var ignoringZeroes = false;
        for (var index = 0; index < Pieces; index++)
        {
            if (ignoringZeroes && address[index] == 0)
            {
                continue;
            }
            ignoringZeroes = false;
            if (compressLength > 1 && index == compressStart)
            {
                output.Append(index == 0 ? "::" : ":");
                ignoringZeroes = true;
                continue;
            }
            output.Append(address[index].ToString("x", CultureInfo.InvariantCulture));
            if (index != Pieces - 1)
            {
                output.Append(':');
            }
        }
        return output.Append(']').ToString();
    }

    private static (int Start, int Length) LongestZeroRun(ushort[] address)
    {
        var best = (Start: -1, Length: 0);
        var index = 0;
        while (index < Pieces)
        {
            if (address[index] != 0)
            {
                index++;
                continue;
            }
            var start = index;
            while (index < Pieces && address[index] == 0)
            {
                index++;
            }
            if (index - start > best.Length)
            {
                best = (start, index - start);
            }
        }
        return best;
    }
}
