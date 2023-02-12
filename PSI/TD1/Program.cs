using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace TD1
{
    class Program
    {

        static void TestsBitmap()
        {
            Bitmap image = new Bitmap("./src/208/default.bmp");

            // Rotation de 180° et sauvegarde
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            image.Save("./src/208/rotate180.bmp", ImageFormat.Bmp);

            // Remise à l'endroit
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);

            // Retournement sur l'axe horizontal
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            image.Save("./src/208/flipX.bmp");
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);

            // Rogner une image
            Rectangle rect = new Rectangle(150, 200, 200, 200);
            Bitmap image2 = image.Clone(rect, image.PixelFormat);
            image2.Save("./src/208/cropped.bmp");

            // INverison de couleur
            Bitmap image3 = new Bitmap(image);

            for (int i = 0; i < image3.Width; i++)
            {
                for (int j = 0; j < image3.Height; j++)
                {
                    Color pixel = image3.GetPixel(i, j);
                    image3.SetPixel(i, j, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }
            image3.Save("./src/208/inverted.bmp");

            // Nuances de gris
            Bitmap image4 = new Bitmap(image);
            for (int i = 0; i < image4.Width; i++)
            {
                for (int j = 0; j < image4.Height; j++)
                {
                    Color pixel = image4.GetPixel(i, j);
                    int moyenne = (pixel.R + pixel.G + pixel.B) / 3;
                    if (moyenne >= 0 && moyenne <= 255)
                    {
                        byte b = (byte)moyenne;
                        image4.SetPixel(i, j, Color.FromArgb(b, b, b));
                    }
                }
            }
            image4.Save("./src/208/grey.bmp");

            // Noir et blanc seulement
            Bitmap image5 = new Bitmap(image);
            for (int i = 0; i < image5.Width; i++)
            {
                for (int j = 0; j < image5.Height; j++)
                {
                    Color pixel = image5.GetPixel(i, j);
                    int moyenne = (pixel.R + pixel.G + pixel.B) / 3;
                    if (moyenne >= 0 && moyenne <= 255)
                    {
                        byte b = (byte)moyenne;
                        b = (b < 128) ? (byte)0 : (byte)255;
                        image5.SetPixel(i, j, Color.FromArgb(b, b, b));
                    }
                }
            }
            image5.Save("./src/208/bw.bmp");
        }

        static void InverserCouleurs(byte[] image)
        {
            for(int i = 54; i < image.Length; i++)
            {
                image[i] = (byte)(255 - image[i]);
            }
        }




        static void Main(string[] args)
        {
            //TestsBitmap();

            // Inversion des couleurs
            byte[] image = File.ReadAllBytes("./src/pulv/default.bmp");
            InverserCouleurs(image);
            File.WriteAllBytes("./src/pulv/inverse.bmp", image);      







        }
    }
}
