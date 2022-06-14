using System.Globalization;

namespace Collections.Utils
{
    public class Locales
    {
        public static string[] Languages { get; } = new string[] { "en", "uz" };

        public static CultureInfo[] GetCultures { get; } = new CultureInfo[]
        {
            new CultureInfo(Languages[0]),
            new CultureInfo(Languages[1])
        };
    }
}
