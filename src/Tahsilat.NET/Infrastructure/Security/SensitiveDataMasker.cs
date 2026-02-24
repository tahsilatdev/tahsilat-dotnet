using System.Text.RegularExpressions;

namespace Tahsilat.NET.Infrastructure.Security
{
    internal static class SensitiveDataMasker
    {
        public static string MaskAll(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string output = input;

            // Secret Key
            output = Regex.Replace(output, @"sk_(test|live)_[0-9A-Za-z]+", m =>
            {
                var val = m.Value;
                if (val.Length <= 10)
                    return "*****";
                return val.Substring(0, 8) + "****" + val.Substring(val.Length - 4);
            });

            // Card Number
            output = Regex.Replace(output, @"\b[0-9]{13,19}\b", m =>
            {
                var card = m.Value;
                return card.Substring(0, 4) + "********" + card.Substring(card.Length - 4);
            });

            return output;
        }
    }
}
