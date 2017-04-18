using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;


namespace ImageCruncher
{
    public partial class Form1 : Form
    {
        string outputDirPrefix = ConfigurationManager.AppSettings.Get("outputDirPrefix");
        string fileFormatSuffix = ConfigurationManager.AppSettings.Get("fileExtension");
        string[] validInputFormats = { ".jpg", ".bmp", ".png" };

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.ShowDialog();
                textBox1.Text = openFileDialog1.FileName;
                textBox2.Text = outputDirPrefix + new FileInfo(textBox1.Text).Name.Split('.')[0] + fileFormatSuffix;
                pictureBox1.Image = Image.FromFile(textBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllBytes(textBox2.Text, CrunchImage(pictureBox1.Image));
                MessageBox.Show("Successfully saved to:\r\n" + textBox2.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnRecreate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(textBox2.Text);
                pictureBox2.Image = UncrunchImage(bytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnConvertAll_Click(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(textBox1.Text);
                foreach (FileInfo file in dir.GetFiles())
                {
                    string newName = outputDirPrefix + file.Name.Split('.')[0] + fileFormatSuffix;
                    if (validInputFormats.Any(x => file.Name.EndsWith(x)))
                        File.WriteAllBytes(newName, CrunchImage(Image.FromFile(file.FullName)));
                }

                MessageBox.Show("Crunched all images in " + dir.Name + " to:\r\n" + outputDirPrefix);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        //
        // The real logic is here. My custom format is defined by the logic in CrunchImage() and UncrunchImage().
        //

        // Current file format:
        //  bytes[0:1] = Image height
        //  bytes[2:3] = Image width
        //  remaining  = 3 bytes per pixel, column first
        private byte[] CrunchImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            byte[] retVal = new byte[4 + 3 * bmp.Height * bmp.Width];
            retVal[0] = (byte)((bmp.Height & 0xFF00) >> 8);
            retVal[1] = (byte)(bmp.Height & 0xFF);
            retVal[2] = (byte)((bmp.Width & 0xFF00) >> 8);
            retVal[3] = (byte)(bmp.Width & 0xFF);

            for (int col = 0; col < bmp.Width; col++)
            {
                for (int row = 0; row < bmp.Height; row++)
                {
                    Color pix = bmp.GetPixel(col, row);
                    retVal[4 + 3 * (row + col * bmp.Height)] = pix.R;
                    retVal[4 + 3 * (row + col * bmp.Height) + 1] = pix.G;
                    retVal[4 + 3 * (row + col * bmp.Height) + 2] = pix.B;
                }
            }

            return retVal;
        }
        private Bitmap UncrunchImage(byte[] bytes)
        {
            int height = (bytes[0] << 8) + bytes[1];
            int width = (bytes[2] << 8) + bytes[3];
            Bitmap bmp = new Bitmap(width, height);

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int R = bytes[4 + 3 * (row + col * bmp.Height)];
                    int G = bytes[4 + 3 * (row + col * bmp.Height) + 1];
                    int B = bytes[4 + 3 * (row + col * bmp.Height) + 2];
                    bmp.SetPixel(col, row, Color.FromArgb(R, G, B));
                }
            }

            return bmp;
        }
    }
}
