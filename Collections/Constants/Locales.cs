using System.Globalization;

namespace Collections.Constants
{
    public class Locales
    {
        public static string[] Languages { get; } = new string[] { "en", "uz" };

        public static CultureInfo[] GetCultures { get; } = new CultureInfo[]
        {
            new(Languages[0]),
            new(Languages[1])
        };
    }
}
