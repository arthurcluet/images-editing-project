using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using TD2;
using System.Windows.Interop;

namespace WPFApp
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private static void DeleteObject(IntPtr handle)
        {
            //throw new NotImplementedException();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnValider_Click(object sender, RoutedEventArgs e)
        {
            QRGenerator gen = new QRGenerator();

            int[,] codeqr = gen.Generate(inputData.Text);

            Bitmap image = new Img(codeqr, true).Agrandir(10).toBitmap();

            imgContainer.Source = ImageSourceFromBitmap(image);
        }

    }
}
