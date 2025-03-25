using System.Text;
using System.Text.RegularExpressions;

namespace CipherJourney.Services
{
    public class Ciphers
    {
        public static string[] CeaserCipher(string text, int shift)
        {
            string[] sentence = Regex.Split(text, @"(\W)");
            StringBuilder result = new StringBuilder();

            foreach (string word in sentence) 
            { 
                foreach (char ch in word)
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

                sentence[Array.IndexOf(sentence, result.ToString())] = result.ToString();
                result.Clear();
            }

            return sentence;
        }

        public static string[] CeaserDecipher(string text, int shift)
        {
            return CeaserCipher(text, 26 - shift); 
        }
    }
}
