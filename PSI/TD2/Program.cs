using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Diagnostics;

namespace TD2
{
    class Program
    {
        public static bool SUPPORT_CONSOLE_COLORS = true;

        /// <summary>
        /// Remplace les "/" d'un chemin par "\\"
        /// J'obtenais une erreur lors de l'utilisation de Process.Start(path) en utilisant des "/"
        /// </summary>
        /// <param name="path">Chemin</param>
        /// <returns>Chemin utilisable dans Process.Start()</returns>
        public static string Path(string path) => String.Join("\\", path.Split('/'));

        private static double PromptDouble(double defaultValue)
        {
            double A;
            try
            {
                A = Convert.ToDouble(Console.ReadLine());
            } catch
            {
                A = defaultValue;
            }

            return A;
        }

        private static int PromptInt(int defaultValue)
        {
            int A;
            try
            {
                A = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                A = defaultValue;
            }

            return A;
        }

        private static string PromptString(string defaultValue)
        {
            string saisie = Console.ReadLine();
            return (saisie.Length == 0) ? defaultValue : saisie;
        }

        private static void WriteWithColor(string S, ConsoleColor C)
        {
            if(SUPPORT_CONSOLE_COLORS) Console.ForegroundColor = C;
            Console.Write(S);
            if (SUPPORT_CONSOLE_COLORS) Console.ResetColor();
        }

        public static void Menu()
        {
            Console.WriteLine("[!] Si votre OS ne permet pas de faire des modifications de couleurs dans la console, il est possible de mettre le champ SUPPORT_CONSOLE_COLORS de la classe Program à false.\n");

            Console.WriteLine("[i] Durant toute l'execution du programme, laissez les champs vides pour utiliser la valeur par défaut spécifiée entre paranthèses\n");
            WriteWithColor("Fichier à utiliser pour l'édition d'images (./coco.bmp) >> ", ConsoleColor.Yellow);

            string path = Console.ReadLine();
            if (path.Length == 0) path = "./coco.bmp";

            Console.WriteLine("\n---------------------- MENU ----------------------\n");
            Console.WriteLine("1 - Nuances de gris");
            Console.WriteLine("2 - Noir et blanc");
            Console.WriteLine("3 - Effet Miroir Horizontal");
            Console.WriteLine("4 - Effet Miroir Vertical");
            Console.WriteLine("5 - Rotation");
            Console.WriteLine("6 - Agrandissement / Rétrécissement");
            Console.WriteLine("7 - Flou Gaussien");
            Console.WriteLine("8 - Détection des bords");
            Console.WriteLine("9 - Repoussage");
            Console.WriteLine("10 - Histogramme");
            Console.WriteLine("11 - Fractale de Julia");
            Console.WriteLine("12 - Stéganographie");
            Console.WriteLine("13 - Code QR");
            Console.WriteLine("14 - Lecture Code QR");

            string saisie = "";
            while(saisie != "q")
            {
                string outputPath;
                string chaine;
                WriteWithColor("\nEntrez un numéro ou 'q' pour quitter >> ", ConsoleColor.DarkCyan);
                saisie = Console.ReadLine();
                Console.WriteLine();
                try
                {
                    int a = Convert.ToInt32(saisie);
                    Img image = new Img(path);
                    switch (a)
                    {
                        case 1:
                            outputPath = "./output/Nuances de gris.bmp";
                            image.NuancesDeGris().Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 2:
                            outputPath = "./output/Noir et blanc.bmp";
                            image.NoirBlanc().Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 3:
                            outputPath = "./output/Miroir horizontal.bmp";
                            image.FlipX().Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 4:
                            outputPath = "./output/Miroir vertical.bmp";
                            image.FlipY().Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 5:
                            outputPath = "./output/Rotation.bmp";
                            WriteWithColor("Angle entier en degrés (-30) >> ", ConsoleColor.Yellow);
                            int angle = PromptInt(-30);

                            image.Rotate(angle).Save(outputPath);
                            Console.WriteLine("\nImage sauvegardée sous : " + outputPath);
                            break;
                        case 6:
                            outputPath = "./output/Agrandissement - Rétrécissement.bmp";
                            WriteWithColor("Facteur de redimensionnement (0.5) >> ", ConsoleColor.Yellow);
                            double fac = PromptDouble(0.5);

                            image.Agrandir(fac).Save(outputPath);
                            Console.WriteLine("\nImage sauvegardée sous : " + outputPath);
                            break;
                        case 7:
                            outputPath = "./output/Flou gaussien.bmp";
                            image.Convolution(Filtres.FlouGaussien5).Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 8:
                            outputPath = "./output/Détection des bords.bmp";
                            image.Convolution(Filtres.DetectionBords).Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 9:
                            outputPath = "./output/Repoussage.bmp";
                            image.Convolution(Filtres.Repoussage).Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 11:
                            outputPath = "./output/Fractale de Julia.bmp";
                            Console.WriteLine("La fractale est générée à partir d'un complexe Z = X + iY (exemples: 0,285 + 0,01 i / 0,3 + 0,5 i)\n");

                            WriteWithColor("X (0.285) >> ", ConsoleColor.Yellow);
                            double X = PromptDouble(0.285);
                            WriteWithColor("Y (0.01) >> ", ConsoleColor.Yellow);
                            double Y = PromptDouble(0.01);

                            int[,] fractale = Fractale.From(new Complex(X, Y), 2000, 2000, 1, 1, true);
                            image = new Img(fractale);
                            image.Save(outputPath);
                            Console.WriteLine("\nImage sauvegardée sous : " + outputPath);
                            break;
                        case 13:
                            outputPath = "./output/QR Code.bmp";

                            WriteWithColor("Chaîne à encoder - max. 4296 caractères (ESILV) >> ", ConsoleColor.Yellow);
                            chaine = PromptString("ESILV");
                            int[,] qrcode = new QRGenerator().Generate(chaine);
                            if (qrcode.GetLength(0) < 50) QRGenerator.DisplayQR(qrcode);

                            image = new Img(qrcode, true).Agrandir(20).Save(outputPath);
                            Console.WriteLine("\nImage sauvegardée sous : " + outputPath);
                            break;
                        case 10:
                            outputPath = "./output/Histogramme.bmp";
                            image.Histogramme().Save(outputPath);
                            Console.WriteLine("Image sauvegardée sous : " + outputPath);
                            break;
                        case 12:
                            Console.WriteLine("L'image principale doit être de dimensions supérieures ou égales à celles de l'image à cacher.\nAttention! Le traitement peut être assez long.");
                            WriteWithColor("Image principale (./pulv.bmp) >> ", ConsoleColor.Yellow);
                            string image1 = PromptString("./pulv.bmp");

                            WriteWithColor("Image à cacher (./coco.bmp) >> ", ConsoleColor.Yellow);
                            string image2 = PromptString("./coco.bmp");

                            image = new Img(image1).Hide(new Img(image2));
                            image.Save("./output/Image cachée.bmp");
                            Console.WriteLine("\nImage cachée sauvegardée sous : " + "./output/Image cachée.bmp");
                            image.Expose();
                            image.Save("./output/Image retrouvée.bmp");
                            Console.WriteLine("Image retrouvée sauvegardée sous : " + "./output/Image retrouvée.bmp");
                            break;
                        case 14:
                            Console.WriteLine("Le lecteur fonctionne lorsque ces règles sont respectées (exemples sur le rapport) :");
                            Console.WriteLine("-- L'image contient uniquement le code QR et d'éventuels bords blancs autour");
                            Console.WriteLine("-- Le code QR est présent dans son intégralité dans l'image");
                            Console.WriteLine("-- Le code QR utilise le mode alphanumérique, le niveau de correction L, le masque 0");
                            Console.WriteLine("-- Un code QR déformé en rectangle reste lisible par notre programme (qui calculera la hauteur et la largeur occupée par le code QR)");

                            WriteWithColor("\nCode QR à décoder (./codeqr.bmp) >> ", ConsoleColor.Yellow);
                            chaine = PromptString("./codeqr.bmp");

                            Console.WriteLine();
                            try
                            {
                                chaine = new QRGenerator().Read(chaine);
                                Console.WriteLine("Texte lu dans le code QR : " + chaine);
                            } catch
                            {
                                Console.WriteLine("Une erreur est survenue lors de la lecture.");
                            }
                            break;
                    }
                } catch
                {
                    Console.WriteLine("Une erreur est survenue (numéro ou fichier non valide?)");
                }
            }
        }

        static void Main(string[] args)
        {
            Menu();

            QRGenerator gen = new QRGenerator();
            int[,] qrcode = gen.Generate("HELLO");

            new Img(qrcode, true).Agrandir(20).Save("./output/qrcode.bmp", true);


            // EXEMPLES DE CODE UTILISANT LA CLASSE Img et QRGenerator

            //Img image = new Img("./pulv.bmp");

            //image.NuancesDeGris();
            //image.NoirBlanc();
            //image.FlipX();
            //image.FlipY();
            //image.Rotate(130);
            //image.Agrandir(2.0);
            //image.Agrandir(0.5);
            //image.Rotate(45);
            //image.Rotate(-45);
            //image.Convolution(Filtres.Repoussage);
            //image.Convolution(Filtres.DetectionBords);

            //Img imageACacher = new Img("./coco.bmp");
            //image.Cacher(imageACacher);
            //image.Expose();

            //Img histogramme = image.Histogramme();

            // Exportation
            //string output = "./output.bmp";
            //image.Save(output);

            //int[,] QRCode = new QRGenerator().GenerateRandom(4296);
            //new Img(QRCode, true).Agrandir(20).Save("./output.bmp");

            /* Les méthodes de la classe Img retournent l'instance de Img afin de pouvoir éxecuter plusieurs fonctions en une seule ligne.
             * Vous pouvez ainsi écrire sans soucis :
             * 
             * Img image = new Img("./pulv.bmp").Rotate(-45).NuancesDeGris().Save("./output.bmp");
             * 
             */

        }

    }
}
