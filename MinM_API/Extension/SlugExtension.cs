using System.Text.RegularExpressions;

namespace MinM_API.Extension
{
    public class SlugExtension
    {
        public static string GenerateSlug(string input)
        {
            var normalized = input.ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
            normalized = Regex.Replace(normalized, @"\s+", "-");
            return normalized.Trim('-');
        }

    }
}
