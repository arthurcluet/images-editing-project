using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace TD2
{
    /// <summary>
    /// Type permettant d'effectuer des manipulations sur une image et l'exporter au format Bitmap
    /// </summary>
    public class Img
    {

        // Position des données dans le tableau de byte de sortie
        private static int _FILE_SIZE_OFFSET = 2;
        private static int _PIXEL_DATA_OFFSET = 10;
        private static int _HEADER_SIZE_OFFSET = 14;
        private static int _IMAGE_WIDTH_OFFSET = 18;
        private static int _IMAGE_HEIGHT_OFFSET = 22;
        private static int _IMAGE_SIZE_OFFSET = 34;
        private static int _PLANES_OFFSET = 26;
        private static int _BITSPERPIXEL_OFFSET = 28;

        string fileType;
        int pixelDataOffset;
        int headerSize;
        int planes;
        int bitsPerPixel;
        Pixel[,] pixels;

        /// <summary>
        /// Crée une instance de Img à partir du chemin d'un fichier .bmp
        /// Ce constructeur lit les informations dans le header et les place dans les champs de Img prévus à cet effet.
        /// (Ils sont bien lus et placés dans des variables -- bien que toujours les mêmes pour chaque image)
        /// </summary>
        /// <param name="filename">Chemin du fichier .BMP</param>
        public Img(string filename)
        {
            byte[] allBytes = File.ReadAllBytes(filename);

            // File Type
            this.fileType = "" + Convert.ToChar(allBytes[0]) + Convert.ToChar(allBytes[1]);

            this.pixelDataOffset = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 10, 4));
            this.headerSize = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 14, 4));
            int width = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 18, 4)); 
            int height = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 22, 4));
            this.planes = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 26, 2));
            this.bitsPerPixel = Utils.ToIntFromLittleEndian(Utils.Slice(allBytes, 28, 2));
            
            // Matrice de pixels
            this.pixels = new Pixel[height, width];

            int index = 54;
            int padding = (width * 3 % 4 == 0) ? 0 : 4 - width * 3 % 4;
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    // Calcul de l'index du pixel (i, j) dans le tableau d'octets
                    //int index = 54 + fullWidth * i + 3 * j;
                    //Console.WriteLine(index);
                    //this.pixels[height - 1 - i, j] = new Pixel(allBytes[index], allBytes[index + 1], allBytes[index + 2]);
                    this.pixels[height - 1 - i, j] = new Pixel(allBytes[index], allBytes[index + 1], allBytes[index + 2]);

                    index += 3;
                    if (j == width - 1) index += padding;
                }
            }

        }
        
        /// <summary>
        /// Ce constructeur crée une instance de Img à partir d'une matrice d'entiers.
        /// Les données nécessaires pour créer l'en-tête du Bitmap sont spécifiées au début de la méthode.
        /// Pour chaque entier, le constructeur associe une certaine couleur (calculée avec une formule inventée).
        /// Dans le cas où la matrice contient des 0 et 1 (pour les codes QR notamment), il convient de le spécifier grâce au second paramètre et l'image sera remplie de cases noires et blanches.
        /// </summary>
        /// <param name="matrix">Matrice dont chaque entier représente un pixel</param>
        /// <param name="nb">Booléen pour spécifier si l'on veut uniquement du noir et blanc</param>
        public Img(int[,] matrix, bool nb = false)
        {
            this.fileType = "BM";
            this.pixelDataOffset = 54;
            this.headerSize = 40;
            this.planes = 1;
            this.bitsPerPixel = 24;

            this.pixels = new Pixel[matrix.GetLength(0), matrix.GetLength(1)];

            if (nb)
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] % 2 == 0)
                        {
                            this.pixels[i, j] = new Pixel(255, 255, 255);
                        }
                        else
                        {
                            this.pixels[i, j] = new Pixel(0, 0, 0);
                        }
                    }
                }
            } else
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        byte r = (byte)(255 - matrix[i, j] % 255);
                        byte g = (byte)(255 - (matrix[i, j] * matrix[i, j]) % 255);
                        byte b = (byte)(255 - (Math.Exp(matrix[i, j])) % 255);
                        this.pixels[i, j] = (matrix[i, j] == -1) ? new Pixel(255, 255, 255) : new Pixel(r, g, b);
                    }
                }
            }
            
        }

        /// <summary>
        /// Alias pour obtenir la hauteur de l'image
        /// </summary>
        public int Height
        {
            get { return this.pixels.GetLength(0); }
        }
        /// <summary>
        /// Alias pour obtenir la largeur de l'image
        /// </summary>
        public int Width
        {
            get { return this.pixels.GetLength(1); }
        }

        /// <summary>
        /// Crée un objet de type Bitmap à partir d'une instance de Img.
        /// Une instance de Bitmap est créée avec les dimensions de l'image, puis chaque les pixels sont spécifiés un par un avec SetPixel().
        /// NB: La fonction n'est pas utilsée dans le projet (elle a été créé en pensant que nous ferions une version WPF du projet)
        /// </summary>
        /// <returns>Copie de l'image au format Bitmap</returns>
        public Bitmap toBitmap()
        {
            Bitmap bmp = new Bitmap(this.pixels.GetLength(0), this.pixels.GetLength(1));
            for(int i = 0; i < bmp.Height; i++)
            {
                for(int j = 0; j < bmp.Width; j++)
                {
                    bmp.SetPixel(j, i, Color.FromArgb(this.pixels[i,j].R, this.pixels[i, j].G, this.pixels[i, j].B));
                }
            }

            return bmp;
        }

        /// <summary>
        /// Crée une chaîne de caractères qui représente l'image dans le but de l'afficher dans la Console
        /// Chaque pixel est remplacé par un certain caractère en fonction de s'il est foncé ou clair.
        /// </summary>
        /// <returns>Chaîne de caractères qui représente l'image</returns>
        public string ToString()
        {
            string s = "";
            for(int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for(int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    s += this.pixels[i, j].toString() + " ";
                }
                s += "\n";
            }
            return s;
        }
        
        /// <summary>
        /// Applique un filtre Nuance de gris.
        /// La moyenne des valeurs R,G,B est calculée pour chaque pixel puis chacune des valeurs est remplacée par cette moyenne.
        /// </summary>
        /// <returns>Image modifiée</returns>
        public Img NuancesDeGris()
        {
            for(int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for(int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    int moyenne = (this.pixels[i, j].R + this.pixels[i, j].G + this.pixels[i, j].B) / 3;
                    if(moyenne >= 0 && moyenne < 256)
                    {
                        byte m = (byte)moyenne;
                        this.pixels[i, j].SetPixel(m, m, m);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Mets l'image en noir et blanc.
        /// Chaque pixel est remplacé par du noir (0,0,0) ou blanc (255,255,255) s'il est clair ou foncé.
        /// </summary>
        /// <returns>Image modifiée</returns>
        public Img NoirBlanc()
        {
            for (int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    int moyenne = (this.pixels[i, j].R + this.pixels[i, j].G + this.pixels[i, j].B) / 3;
                    if (moyenne < 128)
                    {
                        this.pixels[i, j].SetPixel(0, 0, 0);
                    } else
                    {
                        this.pixels[i, j].SetPixel(255, 255, 255);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Applique un effet miroir horizontal à l'image
        /// </summary>
        /// <returns>Image modifiée</returns>
        public Img FlipX()
        {
            for(int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for(int j = 0; j < this.pixels.GetLength(1) / 2; j++)
                {
                    Pixel A = this.pixels[i, j];
                    this.pixels[i, j] = this.pixels[i, this.pixels.GetLength(1) - 1 - j];
                    this.pixels[i, this.pixels.GetLength(1) - 1 - j] = A;
                }
            }
            return this;
        }
        
        /// <summary>
        /// Applique un effet miroir vertical à l'image
        /// </summary>
        /// <returns>Image modifée</returns>
        public Img FlipY()
        {
            for (int i = 0; i < this.pixels.GetLength(0) / 2; i++)
            {
                for (int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    Pixel A = this.pixels[i, j];
                    this.pixels[i, j] = this.pixels[this.pixels.GetLength(0) - 1 - i, j];
                    this.pixels[this.pixels.GetLength(0) - 1 - i, j] = A;
                }
            }
            return this;
        }
        
        /// <summary>
        /// Rotation de l'image dans le sens horaires (multiples de 90° uniquement)
        /// Fonction utilisée par la suite pour la rotation plus "précise".
        /// </summary>
        /// <param name="deg">Angle de rotation multiple de 90</param>
        /// <returns>Image modifiée</returns>
        private Img Rotate90(int deg)
        {
            //int times = (deg / 90) % 4;
            //while (times < 0) times += 4;
            while (deg < 0) deg += 360;
            while (deg >= 360) deg -= 360;

            for(int k = 0; k < deg / 90 && deg % 90 == 0; k++)
            {
                Pixel[,] rot = new Pixel[this.pixels.GetLength(1), this.pixels.GetLength(0)];

                for (int i = 0; i < this.pixels.GetLength(0); i++)
                {
                    for (int j = 0; j < this.pixels.GetLength(1); j++)
                    {
                        rot[j, this.pixels.GetLength(0) - 1 - i] = this.pixels[i, j];
                    }
                }

                this.pixels = rot;
            }

            return this;
        }

        /// <summary>
        /// Calcule l'argument du complexe Z = X + iY passé en paramètre.
        /// Cette fonction n'est plus utilisée car similaire à Math.Atan2();
        /// </summary>
        /// <param name="X">Partie réelle</param>
        /// <param name="Y">Partie imaginaire</param>
        /// <returns>Argument de X+iY</returns>
        private static double Argument(double X, double Y)
        {
            if (X > 0 && Y >= 0) return Math.Atan(Y / X);
            if (X > 0 && Y < 0) return Math.Atan(Y / X) + Math.PI * 2.0;
            if (X < 0) return Math.Atan(Y / X) + Math.PI;
            if (X == 0 && Y > 0) return Math.PI / 2.0;
            return 3.0 * Math.PI / 2.0;
        }

        /// <summary>
        /// Cette fonction effectue une rotation de l'image pour un angle entier fourni en paramètre.
        /// L'image tourne d'abord plusieurs fois de 90° grâce à une autre méthode puis une rotation entre 0 et 90° supplémentaire est faite ici.
        /// Les dimensions de la nouvelle image sont calculées puis pour chaque pixel de cette nouvelle image, la fonction retrouve les coordonnées du pixel correspondant dans l'image d'origine.
        /// En partant des coordonnées dans la nouvelle image et pas l'image d'origine, on évite qu'il y ai des pixels blancs auxquels aucune couleur n'est attribuée.
        /// Angle négatif accepté pour effectuer la rotation dans le sens anti-horaire.
        /// </summary>
        /// <param name="deg">Angle de rotation (négatif pour anti-horaire)</param>
        /// <returns>Image modifiée</returns>
        public Img Rotate(int deg)
        {
            while (deg < 0) deg += 360;
            while (deg >= 360) deg -= 360;

            int rotation = deg % 90;

            // On effectue d'abord des rotations de 90° avec une autre méthode plus simple
            this.Rotate90(deg - rotation);

            // On termine la rotation dans le cas où l'angle n'est pas un multiple de 90°
            if(rotation > 0)
            {
                // Angle de rotation en radians
                double rad = (double)rotation * Math.PI / 180.0;

                // Calcul de la nouvelle taille de l'image
                int height = (int)(Math.Sin(rad) * (double)this.pixels.GetLength(1) + Math.Cos(rad) * (double)this.pixels.GetLength(0));
                int width = (int)(Math.Cos(rad) * (double)this.pixels.GetLength(1) + Math.Sin(rad) * (double)this.pixels.GetLength(0));

                // Matrice de la nouvelle image
                Pixel[,] rot = new Pixel[height, width];

                // Pour chaque pixel de la NOUVELLE image
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {

                        // Calcul des coordonnées cartésiennes du point en question
                        double X = (double)j;
                        double Y = (double)(height - i) - (double)(Math.Sin(rad) * this.pixels.GetLength(1));

                        // Mise en coordonnées polaires + Ajout de l'angle de rotation "rad"
                        double r = Math.Sqrt(X * X + Y * Y);
                        double ang = Math.Atan2(Y, X) + rad;
                        
                        // Calcul des nouvelles coordonnées avec l'angle modifié
                        double x = r * Math.Cos(ang);
                        double y = r * Math.Sin(ang);

                        int I = (int)(this.pixels.GetLength(0) - y);
                        int J = (int)x;

                        if (I >= 0 && J >= 0 && I < this.pixels.GetLength(0) && J < this.pixels.GetLength(1))
                        {
                            rot[i, j] = this.pixels[I, J];
                        }
                        else
                        {
                            rot[i, j] = new Pixel(255, 255, 255);
                        }

                    }
                }

                pixels = rot;
            }

            return this;
        }

        /// <summary>
        /// Permet de rogner une image.
        /// Les coordonnées X et Y sont les coordonnées, dans l'image d'origine, du coin supérieur gauche de l'image rognée.
        /// </summary>
        /// <param name="x">Coordonnée horizontale du coin supérieur gauche de l'image rognée</param>
        /// <param name="y">Coordonnée verticale du coin supérieur gauche de l'image rognée</param>
        /// <param name="width">Largeur de l'image rognée</param>
        /// <param name="height">Hauteur de l'image rognée</param>
        /// <returns>Image modifiée</returns>
        public Img Crop(int x, int y, int width, int height)
        {
            if (y + height <= this.pixels.GetLength(0) && x + width <= this.pixels.GetLength(1) )
            {
                Pixel[,] c = new Pixel[height, width];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        c[i, j] = this.pixels[i + y, j + x];
                    }
                }
                this.pixels = c;
            }

            return this;
        }

        /// <summary>
        /// Agrandit ou réduit la taille de l'image en fonction du facteur passé en paramètre.
        /// Un nombre inférieur à 1 permet de réduire l'image.
        /// Ex : 0.5 pour réduire l'image de moitié || 2.0 pour doubler la taille
        /// </summary>
        /// <param name="facteur">Facteur d'agrandissement</param>
        /// <returns>Image modifiée</returns>
        public Img Agrandir(double facteur)
        {
            int height = (int)((double)this.pixels.GetLength(0) * facteur);
            int width = (int)((double)this.pixels.GetLength(1) * facteur);

            Pixel[,] mat = new Pixel[height, width];

            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    int I = (int)((double)i / (double)facteur);
                    int J = (int)((double)j / (double)facteur);

                    mat[i, j] = this.pixels[I, J];
                }
            }

            this.pixels = mat;

            return this;
        }

        /// <summary>
        /// Permet de modifier les coordonnées R,V,B pour un pixel de coordonnées (X,Y).
        /// </summary>
        /// <param name="x">Coordonnée horizontale du pixel</param>
        /// <param name="y">Coordonnée verticale du pixel</param>
        /// <param name="r">Rouge</param>
        /// <param name="g">Vert</param>
        /// <param name="b">Bleu</param>
        /// <returns>Image modifiée</returns>
        public Img SetPixel(int x, int y, byte r, byte g, byte b)
        {
            if(x >= 0 && y >= 0 && x < this.pixels.GetLength(1) && y < this.pixels.GetLength(0))
            {
                this.pixels[y, x].SetPixel(b, g, r);
            }
            return this;
        }
        /// <summary>
        /// Permet de modifier le pixel de coordonnées (X,Y) en le remplaçant par un pixel placé en paramètre.
        /// </summary>
        /// <param name="x">Coordonnée horizontale du pixel</param>
        /// <param name="y">Coordonnée verticale du pixel</param>
        /// <param name="P">Nouveau pixel</param>
        /// <returns>Image modifiée</returns>
        public Img SetPixel(int x, int y, Pixel P)
        {
            if (x >= 0 && y >= 0 && x < this.pixels.GetLength(1) && y < this.pixels.GetLength(0))
            {
                this.pixels[y, x] = P;
            }
            return this;
        }

        /// <summary>
        /// Effectue une convolution entre l'image et le noyau passé en paramètre.
        /// La convolution est effectuée pour chaque couleur.
        /// Les bords sont traités par EXTENSION et pas par enroullage.
        /// Pour plus d'informations :
        /// https://fr.wikipedia.org/wiki/Noyau_(traitement_d%27image)#Traitement_des_bords
        /// </summary>
        /// <param name="filtre">Noyau</param>
        /// <returns>Image modifiée</returns>
        public Img Convolution(double[,] filtre)
        {
            
            if(filtre != null && filtre.GetLength(0) == filtre.GetLength(1))
            {
                bool contrainteTaille0 = filtre.GetLength(0) >= 0 && filtre.GetLength(0) <= this.pixels.GetLength(0);
                bool contrainteTaille1 = filtre.GetLength(1) >= 0 && filtre.GetLength(1) <= this.pixels.GetLength(1);
                if(contrainteTaille0 && contrainteTaille1)
                {
                    // Début

                    Pixel[,] resultat = new Pixel[this.pixels.GetLength(0), this.pixels.GetLength(1)];

                    for(int i = 0; i < this.pixels.GetLength(0); i++)
                    {
                        for(int j = 0; j < this.pixels.GetLength(1); j++)
                        {
                            int md = filtre.GetLength(0) / 2;
                            int R = 0;
                            int G = 0;
                            int B = 0;

                            int a = 0;
                            for(int k = i - md; k <= i + md; k++)
                            {
                                int b = 0;
                                for(int l = j - md; l <= j + md; l++)
                                {
                                    // Traitement des bords par EXTENSION
                                    int K = k;
                                    int L = l;
                                    if (K < 0) K = 0;
                                    if (L < 0) L = 0;
                                    if (K >= this.pixels.GetLength(0)) K = this.pixels.GetLength(0) - 1;
                                    if (L >= this.pixels.GetLength(1)) L = this.pixels.GetLength(1) - 1;

                                    
                                    R += (int)(this.pixels[K, L].R * filtre[a, b]);
                                    G += (int)(this.pixels[K, L].G * filtre[a, b]);
                                    B += (int)(this.pixels[K, L].B * filtre[a, b]);

                                    b++;
                                }
                                a++;
                            }

                            // Si une des valeurs obtenues n'est pas dans l'intervalle [0, 256[
                            while (R < 0) R++;
                            while (R >= 256) R--;
                            while (G < 0) G++;
                            while (G >= 256) G--;
                            while (B < 0) B++;
                            while (B >= 256) B--;
                            
                            resultat[i, j] = new Pixel((byte) B, (byte)G, (byte)R);
                        }
                    }

                    this.pixels = resultat;
                }
            }

            return this;
        }

        /// <summary>
        /// Renvoie la moyenne des valeurs R,G,B d'un pixel.
        /// Méthode utilisée lors de la lecture de codes QR à partir d'un fichier Bitmap.
        /// </summary>
        /// <param name="P">Pixel</param>
        /// <returns>Moyenne des valeurs R,G,B</returns>
        private static byte PixelWeight(Pixel P)
        {
            int r = P.R;
            int g = P.G;
            int b = P.B;
            int moyenne = (r + g + b) / 3;
            while (moyenne > 255) moyenne--;

            return (byte)moyenne;
        }

        /// <summary>
        /// Cache l'image passée en paramètre dans l'image à partir de laquelle cette méthode est appelée.
        /// L'image cachée doit être de dimensions inférieures à l'autre.
        /// </summary>
        /// <param name="image2">Image à cacher</param>
        /// <returns>Image modifiée</returns>
        public Img Hide(Img image2)
        {
            if(this.pixels.GetLength(0) >= image2.Height && this.pixels.GetLength(1) >= image2.Width)
            {
                for(int i = 0; i < this.pixels.GetLength(0); i++)
                {
                    for(int j = 0; j < this.pixels.GetLength(1); j++)
                    {
                        if(i < image2.Height && j < image2.Width)
                        {
                            byte[] R1 = Utils.ToBinary(this.pixels[i, j].R, 8);
                            byte[] G1 = Utils.ToBinary(this.pixels[i, j].G, 8);
                            byte[] B1 = Utils.ToBinary(this.pixels[i, j].B, 8);
                            byte[] R2 = Utils.ToBinary(image2.pixels[i, j].R, 8);
                            byte[] G2 = Utils.ToBinary(image2.pixels[i, j].G, 8);
                            byte[] B2 = Utils.ToBinary(image2.pixels[i, j].B, 8);
                            Utils.Fill(R1, R2, 4);
                            Utils.Fill(G1, G2, 4);
                            Utils.Fill(B1, B2, 4);
                            this.pixels[i, j] = new Pixel(Utils.ToByte(B1), Utils.ToByte(G1), Utils.ToByte(R1));
                        } else
                        {
                            byte[] R1 = Utils.ToBinary(this.pixels[i, j].R, 8);
                            byte[] G1 = Utils.ToBinary(this.pixels[i, j].G, 8);
                            byte[] B1 = Utils.ToBinary(this.pixels[i, j].B, 8);
                            byte[] blank = new byte[4];
                            Utils.Fill(R1, blank, 4);
                            Utils.Fill(G1, blank, 4);
                            Utils.Fill(B1, blank, 4);
                            this.pixels[i, j] = new Pixel(Utils.ToByte(B1), Utils.ToByte(G1), Utils.ToByte(R1));
                        }
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Fais apparaître une image qui serait cachée dans une autre.
        /// </summary>
        /// <returns>Image modifiée</returns>
        public Img Expose()
        {
            for (int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    byte[] pR = Utils.Copy(Utils.ToBinary(this.pixels[i,j].R, 8), 4, 4);
                    byte[] pG = Utils.Copy(Utils.ToBinary(this.pixels[i, j].G, 8), 4, 4);
                    byte[] pB = Utils.Copy(Utils.ToBinary(this.pixels[i, j].B, 8), 4, 4);
                    byte[] R = new byte[8];
                    byte[] G = new byte[8];
                    byte[] B = new byte[8];
                    Utils.Fill(R, pR, 0);
                    Utils.Fill(G, pG, 0);
                    Utils.Fill(B, pB, 0);
                    this.pixels[i, j] = new Pixel(Utils.ToByte(B), Utils.ToByte(G), Utils.ToByte(R));

                }
            }

            return this;
        }

        /// <summary>
        /// A partir d'un fichier Bitmap contenant un QRCode, crée une matrice de 0 et de 1 contenant les informations du QRCode.
        /// Le QRCode peut être légèrement déformé mais rester lisible.
        /// La méthode cherche le premier pixel noir/foncé dans les coins supérieurs gauche et droit, et inférieur droit. C'est pourquoi seul le code QR doit être présent sur l'image. Il doit également apparaître en intégralité même s'il est déformé.
        /// Une déformation qui changerait la forme du QRCode en un rectangle de ratio différent de 1:1 est acceptée (exemples dans le rapport).
        /// Renvoie NULL en cas de problème de lecture.
        /// </summary>
        /// <returns>Matrice de 0 et 1 contenant les données du QR Code</returns>
        public int[,] ToQRCode()
        {
            // Etalonnage grâce au pattern en haut à gauche uniquement

            int i1 = -1;
            int j1 = -1;
            int i2 = -1;
            int j2 = -1;
            int i3 = -1;
            int j3 = -1;
            for(int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for(int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    if((i1 < 0 || j1 < 0) && PixelWeight(this.pixels[i,j]) < 10)
                    {
                        i1 = i;
                        j1 = j;
                    }
                    if(i1 >= 0 && j1 >= 0) break;
                }
                if (i1 >= 0 && j1 >= 0) break;
            }

            if (i1 < 0 || j1 < 0) return null; // Erreur de lecture

            int c = j1;
            while(j2 < 0)
            {
                c++;
                if (c < this.pixels.GetLength(1) && PixelWeight(this.pixels[i1, c]) >= 255) j2 = c;
            }
            j2--;
            i2 = i1;

            c = i1;
            while(i3 < 0)
            {
                c++;
                if (c < this.pixels.GetLength(0) && PixelWeight(this.pixels[c, j1]) >= 255) i3 = c;
            }
            i3--;
            j3 = j1;

            double dI = (double)(i3 - i1 + 1) / 7.0; // hauteur 1 module
            double dJ = (double)(j2 - j1 + 1) / 7.0; // largeur de 1 module

            // coordonnées d'origine
            double I0 = (double)i1 + dI / 2.0;
            double J0 = (double)j1 + dJ / 2.0;

            // Recherche du pixel noir en bas à gauche

            int i4 = -1;
            int j4 = -1;

            for(int i = this.pixels.GetLength(0) - 1; i >= 0; i--)
            {
                for(int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    if ((i4 < 0 || j4 < 0) && PixelWeight(this.pixels[i, j]) < 10)
                    {
                        i4 = i;
                        j4 = j;
                    }
                    if (i4 >= 0 && j4 >= 0) break;
                }
                if (i4 >= 0 && j4 >= 0) break;
            }

            if (i4 < 0 || j4 < 0) return null; // Erreur de lecture

            // En haut à droite

            int i5 = -1;
            int j5 = -1;

            for (int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for (int j = this.pixels.GetLength(1) - 1; j >= 0; j--)
                {
                    if ((i5 < 0 || j5 < 0) && PixelWeight(this.pixels[i, j]) < 10)
                    {
                        i5 = i;
                        j5 = j;
                    }
                    if (i5 >= 0 && j5 >= 0) break;
                }
                if (i5 >= 0 && j5 >= 0) break;
            }

            if (i5 < 0 || j5 < 0) return null; // Erreur de lecture

            if (dI == 0 || dJ == 0) return null; // Erreur de lecture (divisions par zéro plus loin)

            double DJ = (double)(j5 - j1 + 1);
            double DI = (double)(i4 - i1 + 1);

            double estimatedLength = (DI / dI + DJ / dJ) / 2;

            int version = 0;
            double diff = Math.Abs(estimatedLength - 17.0);

            for(int i = 1; i <= 40; i++)
            {
                double d = Math.Abs(estimatedLength - (17.0 + 4.0 * i));
                if (d < diff)
                {
                    diff = d;
                    version = i;
                }
            }

            int length = 17 + 4 * version;

            // On recalcule dI et dJ :
            dI = DI / (double)length;
            dJ = DJ / (double)length;
            ///////// A PARTIR DE LA, LIRE CHAQUE MODULE



            int[,] code = new int[length, length];

            for(int i = 0; i < length; i++)
            {
                for(int j = 0; j < length; j++)
                {
                    int I = (int)(I0 + i * dI);
                    int J = (int)(J0 + j * dJ);
                    if (I >= 0 && J >= 0 && I < this.pixels.GetLength(0) && J < this.pixels.GetLength(1) && PixelWeight(this.pixels[I, J]) < 10) code[i, j] = 1;
                }
            }

            return code;

        }

        /// <summary>
        /// Crée un histogramme avec la répartition des couleurs
        /// Une image très sombre aura les plus hautes valeurs sur la gauche pour les 3 parties de l'histogramme.
        /// </summary>
        /// <returns>Nouvelle instance de Img contenant l'histogramme</returns>
        public Img Histogramme()
        {
            int[] R = new int[256];
            int[] G = new int[256];
            int[] B = new int[256];

            for(int i = 0; i < this.Height; i++)
            {
                for(int j = 0; j < this.Width; j++)
                {
                    R[this.pixels[i, j].R]++;
                    G[this.pixels[i, j].G]++;
                    B[this.pixels[i, j].B]++;
                }
            }

            int Vmax = 0;
            for(int i = 0; i < 256; i++)
            {
                if (R[i] > Vmax) Vmax = R[i];
                if (G[i] > Vmax) Vmax = G[i];
                if (B[i] > Vmax) Vmax = B[i];
            }

            // Largeur de l'histogramme 3*256 = 768;
            int height = 300;
            int width = 768;

            int[,] mat = new int[height, width];
            Img image = new Img(mat, true);

            for(int i = 0; i < 256; i++)
            {
                int Rp = R[i] * height / (2*Vmax);
                int Gp = G[i] * height / (2*Vmax);
                int Bp = B[i] * height / (2*Vmax);

                for(int j = 0; j < Rp; j++)
                {
                    image.SetPixel(i, height - 1 - j, new Pixel(0, 0, 255));
                }
                for (int j = 0; j < Gp; j++)
                {
                    image.SetPixel(i + 256, height - 1 - j, new Pixel(0, 255, 0));
                }
                for (int j = 0; j < Bp; j++)
                {
                    image.SetPixel(i + 256*2, height - 1 - j, new Pixel(255, 0, 0));
                }
            }

            return image;
        }

        /// <summary>
        /// Création d'un fichier .BMP à partir de la matrice de pixels et des données d'en-têtes situées dans les champs de la classe Img.
        /// </summary>
        /// <param name="filename">Chemin où sauvegarder le fichier</param>
        /// <param name="start">Booléen pour spécifier s'il faut ouvrir le fichier avec Process.Start()</param>
        /// <returns>Instance de la classe Img pour l'image sauvegardée</returns>
        public Img Save(string filename, bool start = false)
        {
            // Copie des valeurs pour simplifier l'écriture après dans certains calculs
            int width = this.pixels.GetLength(1);
            int height = this.pixels.GetLength(0);

            // Calcul de la taille du fichier
            int padding = (width * 3 % 4 == 0) ? 0 : 4 - width * 3 % 4;
            int imageSize = (width * 3 + padding) * height;
            int fileSize = this.pixelDataOffset + imageSize;

            // Création du tableau de bytes
            byte[] bytes = new byte[fileSize];

            // File Type ("BM")
            bytes[0] = Convert.ToByte(this.fileType[0]);
            bytes[1] = Convert.ToByte(this.fileType[1]);

            Utils.FillBytesArray(bytes, _FILE_SIZE_OFFSET, fileSize);
            Utils.FillBytesArray(bytes, _PIXEL_DATA_OFFSET, this.pixelDataOffset);
            Utils.FillBytesArray(bytes, _HEADER_SIZE_OFFSET, this.headerSize);
            Utils.FillBytesArray(bytes, _IMAGE_WIDTH_OFFSET, this.pixels.GetLength(1));
            Utils.FillBytesArray(bytes, _IMAGE_HEIGHT_OFFSET, this.pixels.GetLength(0));
            Utils.FillBytesArray(bytes, _IMAGE_SIZE_OFFSET, imageSize);
            Utils.FillBytesArray(bytes, _PLANES_OFFSET, this.planes);
            Utils.FillBytesArray(bytes, _BITSPERPIXEL_OFFSET, this.bitsPerPixel);

            // Pixels
            int index = 54;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bytes[index] = pixels[height - 1 - i, j].B;
                    bytes[index + 1] = pixels[height - 1 - i, j].G;
                    bytes[index + 2] = pixels[height - 1 - i, j].R;

                    index += 3;
                    if (j == width - 1) index += padding;
                }
            }

            File.WriteAllBytes(filename, bytes);

            if (start)
            {
                Process.Start(Program.Path(filename));
            }

            return this;
        }

        
    }
}
