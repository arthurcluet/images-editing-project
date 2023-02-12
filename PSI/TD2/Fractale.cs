using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TD2
{
    /// <summary>
    /// Classe regroupant les méthodes permettant de générer une fractale de Julia
    /// </summary>
    public static class Fractale
    {
        
        private static int Julia(Complex Z0, Complex c, int l = 100)
        {
            Complex Z = Z0;
            for(int i = 1; i <= l; i++)
            {
                Z = Z * Z + c;
                if(Z.Magnitude > 2.0)  return i;
            }
            return -1;
        }

        /// <summary>
        /// Génère une fractale de Julia sous forme de matrice d'entiers.
        /// Chaque entier représente le nombre d'itérations qu'il a fallu pour que la suite diverge.
        /// Le centre de la matrice obtenue représente les coordonnées (0,0) du plan complexe.
        /// Les valeurs height et width sont les dimensions de la matrice que l'on veut obtenir.
        /// Les valeurs rangeY et rangeX sont la hauteur et largeur de la matrice quand on la place dans le plan complexe (si on veut une fractale entre les coordonnées -1 et 1 pour chaque axe, on met rangeY et rangeX à la valeur 2).
        /// --> Pour les "range" nous essaierons de mettre un schéma explicatif sur le rapport.
        /// 
        /// Lorsque des images très grandes sont générées, les calculs sont précis et très longs et nous avions d'abord décidé d'afficher un % de progression dans la console.
        /// Comme l'idée était bien mais assez peu esthétique (100 fois Console.WriteLine()...) nous l'avons améliorer pour faire apparaître une barre de progression jolie.
        /// Le fonctionnement est assez simple il consiste à mettre à jour de nombreuses fois une ligne de la console en revenant au début avec "\r".
        /// </summary>
        /// <param name="c">Paramètre C de l'ensemble de Julia</param>
        /// <param name="height">Hauteur de la matrice obtenue</param>
        /// <param name="width">Largeur de la matrice obtenue</param>
        /// <param name="rangeY">Hauteur de la matrice dans le plan complexe</param>
        /// <param name="rangeX">Largeur de la matrice dans le plan complexe</param>
        /// <param name="progressBar">Affiche ou non une barre de progression</param>
        /// <returns>Matrice d'entiers représentant une fractale de Julia</returns>
        public static int[,] From(Complex c, int height, int width, double rangeY, double rangeX, bool progressBar = false)
        {
            int[,] mat = new int[height, width];

            double stepX = rangeX / (double)width;
            double stepY = rangeY / (double)height;
            double startX = -0.5 * rangeX;
            double startY = -0.5 * rangeY;

            Random r = new Random();

            int total = height * width;
            int k = 0;
            int last = 0;

            if (progressBar)
            {
                // Affichage de la barre de progression
                if(Program.SUPPORT_CONSOLE_COLORS) Console.BackgroundColor = ConsoleColor.DarkGray;
                for (int Q = 1; Q <= 100; Q++)
                {
                    Console.Write(" ");
                }
                if(Program.SUPPORT_CONSOLE_COLORS) Console.ResetColor();
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    k++;
                    int perc = (int)(100.0 * ((double)k / (double)total));
                    if (perc != last)
                    {
                        last = perc;

                        if (progressBar)
                        {
                            // Barre de progression
                            Console.Write("\r");
                            if(Program.SUPPORT_CONSOLE_COLORS) Console.BackgroundColor = ConsoleColor.Gray;
                            for (int Q = 0; Q < 100; Q++)
                            {
                                if (Q > perc) Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.Write(" ");
                            }
                            if (Program.SUPPORT_CONSOLE_COLORS) Console.ResetColor();
                            Console.Write(" " + perc + "%");
                        }
                        
                    }

                    double X = startX + j * stepX;
                    double Y = startY + i * stepY;
                    Complex Z = new Complex(X, Y);
                    mat[i, j] = Julia(Z, c);

                }
            }

            Console.WriteLine();

            return mat;
        }

        /*
         
        private int[,] mat;
        float step;
        public Fractale(Complex c, int height, int width, double rangeY, double rangeX)
        {
            this.mat = new int[height, width];

            double stepX = rangeX / (double)width;
            double stepY = rangeY / (double)height;
            double startX = -0.5 * rangeX;
            double startY = -0.5 * rangeY;

            Random r = new Random();

            int total = height * width;
            int k = 0;
            int last = 0;

            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {

                    k++;
                    int perc = (int)(100.0 * ((double)k / (double)total));
                    if(perc != last)
                    {
                        last = perc;
                        Console.WriteLine(perc + "%");
                    }

                    double X = startX + j * stepX;
                    double Y = startY + i * stepY;
                    Complex Z = new Complex(X, Y);
                    mat[i, j] = Julia(Z, c);
                    
                }
            }

        }
        
         public int[,] Matrice
        {
            get
            {
                return this.mat;
            }
        }*/




    }
}
