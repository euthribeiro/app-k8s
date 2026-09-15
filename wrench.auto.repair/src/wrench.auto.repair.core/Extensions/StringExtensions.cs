using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace wrench.auto.repair.core.Extensions
{
    public static class StringExtensions
    {
        public static string RemoverAcentos(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoverEspacosDuplicados(this string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        public static string RemoverCaracteresNaoNumericos(this string text)
        {
            return Regex.Replace(text, @"\D", "");
        }

        public static string RemoverCaracteresNaoAlfaNumericos(this string text)
        {
            return Regex.Replace(text, @"\W", "");
        }
    }
}
