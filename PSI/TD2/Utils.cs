using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD2
{
    /// <summary>
    /// Classe regroupant des méthodes assez génériques
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Crée une copie d'une matrice de double en multipliant chaque valeur par une valeur spécifiée en paramètre
        /// </summary>
        /// <param name="mat">Matrice à copier</param>
        /// <param name="a">Facteur</param>
        /// <returns>Copie modifiée</returns>
        public static double[,] Multiply(double[,] mat, double a)
        {
            double[,] result = new double[mat.GetLength(0), mat.GetLength(1)];

            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    result[i, j] = a * mat[i, j];
                }
            }

            return result;
        }

        // Permet d'extraire une partie d'un tableau de bytes
        // Même méthode que "Copy" mais nous l'avons réalisé que plus tard ; cette méthode servant dans "Img.cs" et l'autre dans "QRGenerator.cs"
        public static byte[] Slice(byte[] array, int start, int length)
        {
            byte[] arr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = array[i + start];
            }
            return arr;
        }
        public static int ToIntFromLittleEndian(byte[] littleEndian)
        {
            int S = 0;
            for (int i = 0; i < littleEndian.Length; i++)
            {
                S += littleEndian[i] * (int)Math.Pow(256, i);
            }
            return S;
        }
        public static byte[] ToBytes(int n)
        {
            int l = 1;
            while (Math.Pow(n, 1.0 / l) >= 256) l++;

            byte[] result = new byte[l];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(n % 256);
                n /= 256;
            }

            return result;
        }
        public static void FillBytesArray(byte[] array, int offset, int value)
        {
            byte[] littleEndian = ToBytes(value);
            for (int i = 0; i < littleEndian.Length; i++)
            {
                array[offset + i] = littleEndian[i];
            }
        }

        /// <summary>
        /// Renvoie un tableau de byte sous forme de chaîne de caractères pour l'afficher dans la console
        /// </summary>
        /// <param name="tab">Tableau à convertir</param>
        /// <param name="separator">Caractères à mettre à chaque byte</param>
        /// <returns>Le tableau sous forme de chaîne de caractères</returns>
        public static string toString(byte[] tab, string separator = "")
        {
            string s = "";
            for (int i = 0; i < tab.Length; i++)
            {
                s += tab[i] + separator;
            }
            return s;
        }

        public static string RandomString(int length = 25, string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            Random rnd = new Random();
            string s = "";
            for (int i = 0; i < length; i++)
            {
                s += chars[rnd.Next(0, chars.Length)];
            }

            return s;
        }

        // Remplit le tableau "array" avec le contenu de "content" à l'index spécifié
        public static void Fill(byte[] array, byte[] content, int index)
        {
            if (index >= 0)
                for (int i = 0; i < content.Length; i++)
                    if (i + index < array.Length) array[i + index] = content[i];
        }

        // Remplit la matrice "matrix" avec le contenu de "content" aux coordonnées spécifiées
        public static void Fill(int[,] matrix, int[,] content, int x, int y)
        {
            if (x >= 0 && y >= 0)
                for (int i = 0; i < content.GetLength(0); i++)
                    for (int j = 0; j < content.GetLength(1); j++)
                        if (x + i < matrix.GetLength(0) && y + j < matrix.GetLength(1)) matrix[i + x, j + y] = content[i, j];
        }

        // Permet d'extraire, en la copiant, une partie de tableau à partir d'un indice spécifié et jusqu'à une certaine longueur
        public static byte[] Copy(byte[] og, int index, int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                b[i] = og[index + i];
            }
            return b;
        }

        // Détermine la valeur maximum qu'il est possible de placer dans une chaîne de bits de longueur placée en paramètres
        // Sert pour la fonction ToBinary uniquement
        private static int Max(int binaryLength)
        {
            int S = 0;
            for (int i = 0; i < binaryLength; i++)
            {
                S += (int)Math.Pow(2, i);
            }
            return S;
        }

        // Convertit un entier en binaire
        // Par défaut, place ce binaire dans un tableau d'octets de longueur MINIMALE pour contenir tous les bits
        public static byte[] ToBinary(int a, int length = -1)
        {
            if (length > 0) length++;
            int pMax = length - 1;
            while (a > Max(pMax) || pMax < 0) pMax++;
            byte[] tab = new byte[pMax];
            for (int i = pMax - 1; i >= 0; i--)
            {
                if (a >= Math.Pow(2, i))
                {
                    tab[pMax - 1 - i] = 1;
                    a -= (int)Math.Pow(2, i);
                }

            }
            return tab;
        }

        // Tableau de bits convertit en octet
        public static byte ToByte(byte[] a)
        {
            int b = 0;
            int c = 0;
            for (int i = a.Length - 1; i >= 0; i--)
            {
                b += (int)a[i] * (int)Math.Pow(2, c);
                c++;
            }

            return (byte)b;
        }

        // Tableau de bits convertit en entier
        public static int ToInt(byte[] a)
        {
            int b = 0;
            int c = 0;
            for (int i = a.Length - 1; i >= 0; i--)
            {
                b += (int)a[i] * (int)Math.Pow(2, c);
                c++;
            }

            return b;
        }



        // Opération OU EXCLUSIF sur deux tableaux de bits
        public static byte[] XOR(byte[] A, byte[] B)
        {
            if (A != null && B != null && A.Length == B.Length)
            {
                byte[] resultat = new byte[A.Length];
                for (int i = 0; i < resultat.Length; i++)
                {
                    if (A[i] != B[i]) resultat[i] = 1;
                }
                // Remove left zeros
                int z = 0;
                for (int i = 0; i < resultat.Length; i++)
                {
                    if (resultat[i] == 0) z++;
                    else break;
                }
                byte[] resultatReduced = new byte[resultat.Length - z];
                for (int i = 0; i < resultatReduced.Length; i++)
                {
                    resultatReduced[i] = resultat[i + z];
                }

                return resultatReduced;
            }
            else
            {
                return null;
            }
        }
    }
}
