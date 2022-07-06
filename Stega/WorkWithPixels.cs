using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stega
{
    public static class WorkWithPixels
    {
        private static Random r = new Random(DateTime.Now.Second);
        public static bool InsertSignatureIntoPixels(MyImage myImage, byte[] signature, int useBits, bool addNoise)
        {
            Bitmap myImageBitmap;
            Queue<byte> signatureBits = SignatureBytesToBitsQueue(signature);

            try
            {
                myImageBitmap = new Bitmap(myImage.imagePath, true);
                InsertStegoValuesIntoPixels(myImageBitmap, signatureBits);

                int skipPixels = 24;
                int yStartValue = skipPixels / myImageBitmap.Width;
                int xStartValue = skipPixels % myImageBitmap.Width;

                for (int y = yStartValue; y < myImageBitmap.Height; y++)
                {
                    for (int x = xStartValue; x < myImageBitmap.Width; x++)
                    {
                        Color pixel = myImageBitmap.GetPixel(x, y);
                        Color newColor = InsertBitsIntoPixel(pixel, useBits, signatureBits);

                        myImageBitmap.SetPixel(x, y, newColor);
                        if (signatureBits.Count == 0)
                            break;
                    }
                    if (signatureBits.Count == 0)
                        break;
                    xStartValue = 0;
                }

                if(addNoise)
                    AddNoises(myImageBitmap, useBits, signature.Length);

                string new_image_path = myImage.imagePath.Replace(".png", "_pix_podp.png");
                myImageBitmap.Save(new_image_path);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        private static void InsertStegoValuesIntoPixels(Bitmap myImageBitmap, Queue<byte> signatureBits)
        {

            int systemBits = 72;

            for (int y = 0; y < myImageBitmap.Height; y++)
            {
                for (int x = 0; x < myImageBitmap.Width; x++)
                {
                    Color pixel = myImageBitmap.GetPixel(x, y);
                    Color newColor = InsertBitsIntoPixel(pixel, 1, signatureBits);

                    myImageBitmap.SetPixel(x, y, newColor);
                    systemBits -= 3;
                    if (systemBits == 0)
                        break;
                }
                if (systemBits == 0)
                    break;
            }
        }

        private static Queue<byte> SignatureBytesToBitsQueue(byte[] signatureBytes)
        {
            Queue<byte> signatureBits = new Queue<byte>();
            //Получаем биты из байт подписи
            foreach (byte b in signatureBytes)
            {
                signatureBits.Enqueue((byte)(b >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 1) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 2) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 3) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 4) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 5) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 6) >> 7));
                signatureBits.Enqueue((byte)((byte)(b << 7) >> 7));
            }

            return signatureBits;
        }

        private static Color InsertBitsIntoPixel(Color pixel, int useBits, Queue<byte> bitsForInsert)
        {
            Color newColor = Color.FromArgb(pixel.R, pixel.G, pixel.B); ;
            switch (bitsForInsert.Count / useBits)
            {
                case 0:
                    break;
                case 1:
                    newColor = Color.FromArgb(
                        InsertBitsIntoColor(pixel.R, useBits, bitsForInsert),
                        pixel.G,
                        pixel.B);
                    break;
                case 2:
                    newColor = Color.FromArgb(
                        InsertBitsIntoColor(pixel.R, useBits, bitsForInsert),
                        InsertBitsIntoColor(pixel.G, useBits, bitsForInsert),
                        pixel.B);
                    break;
                default:
                    newColor = Color.FromArgb(
                        InsertBitsIntoColor(pixel.R, useBits, bitsForInsert),
                        InsertBitsIntoColor(pixel.G, useBits, bitsForInsert),
                        InsertBitsIntoColor(pixel.B, useBits, bitsForInsert));

                    break;
            }

            return newColor;
        }

        private static int InsertBitsIntoColor(int byteToInsert, int useBits, Queue<byte> bitsForInsert)
        {
            byteToInsert = byteToInsert >> useBits;    //убираем ненужные биты
            for (int i = 0; i < useBits; i++)
                byteToInsert = (byteToInsert << 1) + bitsForInsert.Dequeue();

            return byteToInsert;
        }

        private static Bitmap AddNoises(Bitmap myImageBitmap, int useBits, int signatureLength)
        {
            int skipPixels = 24 + (int)Math.Truncate((double)((signatureLength - 9) * 8) / useBits / 3) + 1;
            int yStartValue = skipPixels / myImageBitmap.Width;
            int xStartValue = skipPixels % myImageBitmap.Width;

            try
            {
                for (int y = yStartValue; y < myImageBitmap.Height; y++)
                {
                    for (int x = xStartValue; x < myImageBitmap.Width; x++)
                    {
                        Color pixel = myImageBitmap.GetPixel(x, y);
                        Color newColor = AddNoiseToPixel(pixel, useBits);

                        myImageBitmap.SetPixel(x, y, newColor);
                    }
                    xStartValue = 0;
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
            return myImageBitmap;
        }

        private static Color AddNoiseToPixel(Color pixel, int useBits)
        {
            int R = AddNoiseToColor(pixel.R, useBits);
            int G = AddNoiseToColor(pixel.G, useBits);
            int B = AddNoiseToColor(pixel.B, useBits);

            return Color.FromArgb(R, G, B);
        }

        private static int AddNoiseToColor(int color, int useBits)
        {
            color = color >> useBits;    //убираем ненужные биты
            for (int i = 0; i < useBits; i++)
                color = (color << 1) + r.Next(0, 2);
            return color;
        }

        //--------------------------------------------------------------------------------------------------------------------------

        public static string ReadSignatureFromPixels(MyImage myImage)
        {
            int useBits, signatureLength;
            GetMyStegoValuesFromBitmap(myImage, out useBits, out signatureLength);
            var signatureBytes = GetSignatureBytesFromBitmap(myImage, useBits, signatureLength);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < signatureLength; i++)
                stringBuilder.Append((char)signatureBytes[i]);

            return stringBuilder.ToString();
        }

        private static void GetMyStegoValuesFromBitmap(MyImage myImage, out int useBits, out int signatureLength)
        {
            Bitmap myImageBitmap;
            Queue<byte> systemBits = new Queue<byte>();
            useBits = 0;
            signatureLength = 0;

            try
            {
                myImageBitmap = new Bitmap(myImage.imagePath, false);

                for (int y = 0; y < myImageBitmap.Height; y++)
                {
                    for (int x = 0; x < myImageBitmap.Width; x++)
                    {
                        Color pixel = myImageBitmap.GetPixel(x, y);
                        GetBitsFromPixel(pixel, 1, systemBits);

                        if (systemBits.Count >= 72)
                            break;
                    }
                    if (systemBits.Count >= 72)
                        break;
                }

                //перевод очереди в данные
                for (int i = 0; i < 4; i++)
                    if (BitsFromQueueToByte(systemBits) != 255)
                        throw new Exception("Не удалось прочесть информацию из изображения.");

                useBits = BitsFromQueueToByte(systemBits);
                byte[] signatureLengthBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                    signatureLengthBytes[i] = BitsFromQueueToByte(systemBits);

                signatureLength = BitConverter.ToInt32(signatureLengthBytes, 0);

            }
            catch (ArgumentException)
            {
                throw new Exception("Не удалось открыть изображение");
            }
        }

        private static byte[] GetSignatureBytesFromBitmap(MyImage myImage, int useBits, int signatureLength)
        {
            Bitmap myImageBitmap;
            Queue<byte> signatureBits = new Queue<byte>();
            int signatureBitsLength = signatureLength * 8;
            byte[] signtureBytes = new byte[signatureLength];

            try
            {
                myImageBitmap = new Bitmap(myImage.imagePath, false);
                int skipPixels = 24;
                int yStartValue = skipPixels / myImageBitmap.Width;
                int xStartValue = skipPixels % myImageBitmap.Width;

                for (int y = yStartValue; y < myImageBitmap.Height; y++)
                {
                    for (int x = xStartValue; x < myImageBitmap.Width; x++)
                    {
                        Color pixel = myImageBitmap.GetPixel(x, y);
                        GetBitsFromPixel(pixel, useBits, signatureBits);

                        if (signatureBits.Count >= signatureBitsLength)
                            break;
                    }
                    xStartValue = 0;
                    if (signatureBits.Count >= signatureBitsLength)
                        break;
                }

                //перевод очереди в данные                
                for (int i = 0; i < signatureLength; i++)
                    signtureBytes[i] = BitsFromQueueToByte(signatureBits);

            }
            catch (ArgumentException)
            {
                throw new Exception("Не удалось открыть изображение");
            }
            return signtureBytes;
        }

        private static void GetBitsFromPixel(Color pixel, int useBits, Queue<byte> signatureBits)
        {
            GetBitsFromColor(pixel.R, useBits, signatureBits);
            GetBitsFromColor(pixel.G, useBits, signatureBits);
            GetBitsFromColor(pixel.B, useBits, signatureBits);
        }

        private static void GetBitsFromColor(int byteToGetBits, int useBits, Queue<byte> signatureBits)
        {
            for (int i = 0; i < useBits; i++)
                signatureBits.Enqueue((byte)((byte)(byteToGetBits << (8 - useBits + i)) >> 7));
        }

        private static byte BitsFromQueueToByte(Queue<byte> signatureBits)
        {
            byte res = 0;
            for (int i = 0; i < 8; i++)
                res = (byte)((byte)(res << 1) + signatureBits.Dequeue());
            return res;
        }
    }
}