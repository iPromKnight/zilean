namespace Zilean.Scraper.Features.LzString;

public class Decompressor
{
    private const string KeyStrUriSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$";
    private static readonly IDictionary<char, char> _keyStrUriSafeDict = CreateBaseDict(KeyStrUriSafe);

    private static IDictionary<char, char> CreateBaseDict(string alphabet)
    {
        var dict = new Dictionary<char, char>();
        for (var i = 0; i < alphabet.Length; i++)
        {
            dict[alphabet[i]] = (char)i;
        }
        return dict;
    }

    public static string FromEncodedUriComponent(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        input = input.Replace(" ", "+");
        return Decompress(input.Length, 32, index => _keyStrUriSafeDict[input[index]]);
    }

    private static string Decompress(int length, int resetValue, Func<int, char> getNextValue)
    {
        var dictionary = new List<Memory<char>>();
        var enlargeIn = 4;
        var numBits = 3;
        var result = StringBuilderCache.Acquire();
        int i;
        Memory<char> w;
        int bits = 0, resb, maxpower, power;
        var c = '\0';

        var data_val = getNextValue(0);
        var data_position = resetValue;
        var data_index = 1;

        for (i = 0; i < 3; i += 1)
        {
            var rentedArray = ArrayPool<char>.Shared.Rent(1);
            rentedArray[0] = (char)i;
            dictionary.Add(rentedArray.AsMemory(0, 1));
        }

        maxpower = (int)Math.Pow(2, 2);
        power = 1;
        while (power != maxpower)
        {
            resb = data_val & data_position;
            data_position >>= 1;
            if (data_position == 0)
            {
                data_position = resetValue;
                data_val = getNextValue(data_index++);
            }
            bits |= (resb > 0 ? 1 : 0) * power;
            power <<= 1;
        }

        switch (bits)
        {
            case 0:
                bits = 0;
                maxpower = (int)Math.Pow(2, 8);
                power = 1;
                while (power != maxpower)
                {
                    resb = data_val & data_position;
                    data_position >>= 1;
                    if (data_position == 0)
                    {
                        data_position = resetValue;
                        data_val = getNextValue(data_index++);
                    }
                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }
                c = (char)bits;
                break;
            case 1:
                bits = 0;
                maxpower = (int)Math.Pow(2, 16);
                power = 1;
                while (power != maxpower)
                {
                    resb = data_val & data_position;
                    data_position >>= 1;
                    if (data_position == 0)
                    {
                        data_position = resetValue;
                        data_val = getNextValue(data_index++);
                    }
                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }
                c = (char)bits;
                break;
            case 2:
                return StringBuilderCache.GetStringAndRelease(result);
        }
        var rentedW = ArrayPool<char>.Shared.Rent(1);
        rentedW[0] = c;
        w = rentedW.AsMemory(0, 1);
        dictionary.Add(w);
        result.Append(c);
        while (true)
        {
            if (data_index > length)
            {
                return StringBuilderCache.GetStringAndRelease(result);
            }

            bits = 0;
            maxpower = (int)Math.Pow(2, numBits);
            power = 1;
            while (power != maxpower)
            {
                resb = data_val & data_position;
                data_position >>= 1;
                if (data_position == 0)
                {
                    data_position = resetValue;
                    data_val = getNextValue(data_index++);
                }
                bits |= (resb > 0 ? 1 : 0) * power;
                power <<= 1;
            }

            int c2;
            switch (c2 = bits)
            {
                case 0:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 8);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }

                    c2 = dictionary.Count;
                    var rentedArray = ArrayPool<char>.Shared.Rent(1);
                    rentedArray[0] = (char)bits;
                    dictionary.Add(rentedArray.AsMemory(0, 1));
                    enlargeIn--;
                    break;
                case 1:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 16);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c2 = dictionary.Count;
                    var rentedArray16 = ArrayPool<char>.Shared.Rent(1);
                    rentedArray16[0] = (char)bits;
                    dictionary.Add(rentedArray16.AsMemory(0, 1));
                    enlargeIn--;
                    break;
                case 2:
                    return StringBuilderCache.GetStringAndRelease(result);
            }

            if (enlargeIn == 0)
            {
                enlargeIn = (int)Math.Pow(2, numBits);
                numBits++;
            }

            Memory<char> entry;
            if (dictionary.Count - 1 >= c2)
            {
                entry = dictionary[c2];
            }
            else
            {
                if (c2 == dictionary.Count)
                {
                    entry = w.Span.ToArray().AsMemory();
                }
                else
                {
                    return null;
                }
            }
            result.Append(entry.Span);

            // Add w+entry[0] to the dictionary.
            var newEntry = ArrayPool<char>.Shared.Rent(w.Length + 1);
            w.Span.CopyTo(newEntry);
            newEntry[w.Length] = entry.Span[0];
            dictionary.Add(newEntry.AsMemory(0, w.Length + 1));
            enlargeIn--;

            w = entry;
            if (enlargeIn == 0)
            {
                enlargeIn = (int)Math.Pow(2, numBits);
                numBits++;
            }
        }
    }
}
