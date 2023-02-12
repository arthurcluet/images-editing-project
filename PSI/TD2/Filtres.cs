using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD2
{
    /// <summary>
    /// Classe comportant uniquement des champs statiques contenant différents noyaux pour effectuer une convolution.
    /// </summary>
    class Filtres
    {

        private static double[,] boxBlur = new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        private static double[,] gauss3 = new double[,] { { 1, 2, 1 } , { 2, 4, 2 }, { 1, 2, 1 } };
        private static double[,] gauss5 = new double[,] { { 1, 4, 6, 4, 1 }, { 4, 16, 24, 16, 4 }, { 6, 24, 36, 24, 6 }, { 4, 16, 24, 16, 4 }, { 1, 4, 6, 4, 1 } };
        private static double[,] detectionContour = new double[,] { { -1, -1, -1 } , { -1, 8, -1 } , { -1, -1, -1} };
        private static double[,] Emboss = new double[,] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };

        public static double[,] BoxBlur
        {
            get { return Utils.Multiply(boxBlur, 1.0 / 9); }
        }

        public static double[,] FlouGaussien3
        {
            get { return Utils.Multiply(gauss3, 1.0 / 16); }
        }

        public static double[,] FlouGaussien5
        {
            get { return Utils.Multiply(gauss5, 1.0 / 256); }
        }

        public static double[,] DetectionBords
        {
            get { return detectionContour;  }
        }

        public static double[,] Repoussage
        {
            get
            {
                return Emboss;
            }
        }

    }
}
