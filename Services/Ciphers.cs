using System.Text;
using System.Text.RegularExpressions;

namespace CipherJourney.Services
{
    public class Ciphers
    {
        public static string CaesarCipher(string text, int shift)
        {
            StringBuilder result = new StringBuilder();

            foreach (char ch in text)
            {
                if (char.IsLetter(ch))
                {
                    char offset = char.IsUpper(ch) ? 'A' : 'a';
                    result.Append((char)((ch - offset + shift) % 26 + offset));
                }
                else
                {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }



        public static string CaeserDecipher(string text, int shift)
        {
            return CaesarCipher(text, 26 - shift); 
        }
    }
}
