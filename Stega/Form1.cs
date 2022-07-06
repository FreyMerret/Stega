using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;


namespace Stega
{
    public partial class Form1 : Form
    {
        public MyImage myImage;

        public Form1()
        {
            InitializeComponent();
            textBox5.Text = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
            radioButton1.Checked = true;
        }

        private void buttonOpenImage_Click(object sender, EventArgs e)
        {
            myImage = new MyImage();
            myImage.OnenImage();
            if (myImage.image is null)
                return;

            textBox1.Text = myImage.imagePath;
            button2.Enabled = true;
            panel4.Enabled = true;
            trackBar1_Scroll(null, null);

            pictureBox1.ClientSize = new Size(myImage.normalizedImageWidth, myImage.normalizedImageHeigth);
            pictureBox1.Image = myImage.image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked || radioButton3.Checked)
            {
                var addNoise = checkBox1.CheckState == CheckState.Checked;
                var useBits = (int)Math.Pow(2, trackBar1.Value);
                var signature = MySignature.SignatureFormation(GetSignatureText(), useBits);
                var isDone = WorkWithPixels.InsertSignatureIntoPixels(myImage,signature, useBits, addNoise);
                if (isDone)
                    MessageBox.Show("Информация успешно вставлена в пиксели");
                else
                    MessageBox.Show("Произошла ошибка");
            }

            if (radioButton4.Checked)
            {
                var signature = WorkWithPixels.ReadSignatureFromPixels(myImage);
                textBox6.Text = signature;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = String.Format("Использовать младших бит в пикселе: {0}", Math.Pow(2, trackBar1.Value));
            label4.Text = String.Format("Доступно байт для записи: {0} ", myImage.imageWidth * myImage.imageHeight * 3 * (Math.Pow(2, trackBar1.Value)) / 8);
        }

        private string GetSignatureText()
        {
            string str = "";
            if (radioButton1.Checked)
                str = textBox2.Text;

            if (radioButton3.Checked)
                str = "Name: " + textBox4.Text + " Date: " + textBox5.Text;

            return str;
        }
    }
}
