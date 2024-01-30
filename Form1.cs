
using AKG1;
using System.Drawing.Imaging;

namespace AKG1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var parser = new ObjFileParser();
            parser.ParseOBJFile();




            // Create a new bitmap.
            Bitmap bmp = new Bitmap("C:\\AKG\\models\\Plane_diffuse.jpg");

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
            for (int counter = 2; counter < rgbValues.Length; counter += 3)
                rgbValues[counter] = 255;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            //e.Graphics.DrawImage(bmp, 0, 150);
            pictureBox.CreateGraphics().DrawImage(bmp, rect);



        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}