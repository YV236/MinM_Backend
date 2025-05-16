using System.Text;
using System.Text.RegularExpressions;

namespace MinM_API.Extension;

public static partial class SlugExtension
{
    private static readonly Dictionary<char, string> ConvertedLetters = new()
    {
        ['а'] = "a",
        ['б'] = "b",
        ['в'] = "v",
        ['г'] = "h",
        ['ґ'] = "g",
        ['д'] = "d",
        ['е'] = "e",
        ['є'] = "ie",
        ['ж'] = "zh",
        ['з'] = "z",
        ['и'] = "y",
        ['і'] = "i",
        ['ї'] = "i",
        ['й'] = "i",
        ['к'] = "k",
        ['л'] = "l",
        ['м'] = "m",
        ['н'] = "n",
        ['о'] = "o",
        ['п'] = "p",
        ['р'] = "r",
        ['с'] = "s",
        ['т'] = "t",
        ['у'] = "u",
        ['ф'] = "f",
        ['х'] = "kh",
        ['ц'] = "ts",
        ['ч'] = "ch",
        ['ш'] = "sh",
        ['щ'] = "shch",
        ['ь'] = "",
        ['ю'] = "iu",
        ['я'] = "ia",

        ['А'] = "A",
        ['Б'] = "B",
        ['В'] = "V",
        ['Г'] = "H",
        ['Ґ'] = "G",
        ['Д'] = "D",
        ['Е'] = "E",
        ['Є'] = "Ye",
        ['Ж'] = "Zh",
        ['З'] = "Z",
        ['И'] = "Y",
        ['І'] = "I",
        ['Ї'] = "Yi",
        ['Й'] = "Y",
        ['К'] = "K",
        ['Л'] = "L",
        ['М'] = "M",
        ['Н'] = "N",
        ['О'] = "O",
        ['П'] = "P",
        ['Р'] = "R",
        ['С'] = "S",
        ['Т'] = "T",
        ['У'] = "U",
        ['Ф'] = "F",
        ['Х'] = "Kh",
        ['Ц'] = "Ts",
        ['Ч'] = "Ch",
        ['Ш'] = "Sh",
        ['Щ'] = "Shch",
        ['Ь'] = "",
        ['Ю'] = "Iu",
        ['Я'] = "Ia"
    };

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex InvalidSlugCharsRegex();

    public static string GenerateSlug(string input)
    {
        string transliterated = TransliterateCyrillicToLatin(input);

        transliterated = transliterated.ToLowerInvariant();

        transliterated = WhitespaceRegex().Replace(transliterated, "-");

        transliterated = InvalidSlugCharsRegex().Replace(transliterated, "");

        return transliterated.Trim('-');
    }

    private static string TransliterateCyrillicToLatin(string input)
    {
        var result = new StringBuilder();

        foreach (char c in input)
        {
            if (ConvertedLetters.TryGetValue(c, out var converted))
            {
                result.Append(converted);
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
