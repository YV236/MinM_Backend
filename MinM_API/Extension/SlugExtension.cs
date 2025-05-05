using System.Text;
using System.Text.RegularExpressions;

public static class SlugExtension
{
    private static readonly Dictionary<char, string> ConvertedLetters = new Dictionary<char, string>
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

    public static string GenerateSlug(string input)
    {
        string transliterated = TransliterateCyrillicToLatin(input);

        transliterated = transliterated.ToLowerInvariant();

        transliterated = Regex.Replace(transliterated, @"\s+", "-");

        transliterated = Regex.Replace(transliterated, @"[^a-z0-9\-]", "");

        return transliterated.Trim('-');
    }

    private static string TransliterateCyrillicToLatin(string input)
    {
        var result = new StringBuilder();

        foreach (char c in input)
        {
            result.Append(ConvertedLetters.ContainsKey(c) ? ConvertedLetters[c] : c.ToString());
        }

        return result.ToString();
    }
}
