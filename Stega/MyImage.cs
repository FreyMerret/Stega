using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stega
{
    public class MyImage
    {
        public string imagePath;
        public Image image;
        public int imageWidth, imageHeight;
        public int normalizedImageWidth, normalizedImageHeigth;

        public void OnenImage()
        {
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы png|*.png";
            if (OPF.ShowDialog() == DialogResult.OK)
            {                
                imagePath = OPF.FileName;
                Bitmap MyImage = new Bitmap(imagePath);
                imageWidth = MyImage.Width;
                imageHeight = MyImage.Height;
                double k = (imageHeight > imageWidth) ? imageHeight / 450d : imageWidth / 400d;

                normalizedImageWidth = (int)Math.Round(imageWidth / k);
                normalizedImageHeigth = (int)Math.Round(imageHeight / k);

                image = (Image)MyImage.Clone();
                MyImage.Dispose();
            }
        }


    }
}
