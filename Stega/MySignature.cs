using System;
using System.Text;

namespace Stega
{
    public class MySignature
    {
        public static byte[] SignatureFormation(string signatureText, int useBits)
        {
            signatureText = Transliteration(signatureText);
            var signatureTextLength = signatureText.Length;
            var signatureBytes = new byte[signatureTextLength + 9];
            byte[] signatureTextLengthInBytes = BitConverter.GetBytes(signatureTextLength);

            signatureBytes[0] = 255;
            signatureBytes[1] = 255;
            signatureBytes[2] = 255;
            signatureBytes[3] = 255;
            signatureBytes[4] = (byte)useBits;
            signatureBytes[5] = signatureTextLengthInBytes[0];
            signatureBytes[6] = signatureTextLengthInBytes[1];
            signatureBytes[7] = signatureTextLengthInBytes[2];
            signatureBytes[8] = signatureTextLengthInBytes[3];

            for (int i = 0; i < signatureTextLength; i++)
                signatureBytes[i+9] = Convert.ToByte(signatureText[i]);

            return signatureBytes;
        }

        private static string Transliteration(string s)
        {
            StringBuilder res = new StringBuilder();
            string[] rus = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й",
                            "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц",
                            "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", 
                            "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й",
                            "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц",
                            "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я", " " };

            string[] eng = { "A", "B", "V", "G", "D", "E", "E", "ZH", "Z", "I", "Y",
                            "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "KH", "TS",
                            "CH", "SH", "SHCH", null, "Y", "'", "E", "YU", "YA",
                            "a", "b", "v", "g", "d", "e", "e", "zh", "z", "i", "y",
                            "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts",
                            "ch", "sh", "shch", null, "y", "'", "e", "yu", "ya"," " };
            bool flag = false;
            for (int j = 0; j < s.Length; j++)
            {
                for (int i = 0; i < rus.Length; i++)
                    if (s.Substring(j, 1) == rus[i])
                    {
                        res.Append(eng[i]);
                        flag = true;
                        continue;
                    }
                if (flag)
                {
                    flag = false;
                    continue;
                }
                res.Append(s[j]);
            }
            return res.ToString();
        }
    }
}
