using ReedSolomon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TD2
{
    /// <summary>
    /// Classe permettant de générer des QR Codes.
    /// Nécessite d'être instanciée car des données nécessaires à la fabrication du code QR sont stockées dans un fichier
    /// </summary>
    public class QRGenerator
    {

        // Bits pour spécifier le mode
        private static byte[] ALPHANUMERIC_MODE = new byte[] { 0, 0, 1, 0 };
        // Caractères possible en alphanumérique
        private static string ALPHANUMERIC_CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";
        // Polynome pour générer la chaîne de bits indiquant la version du code QR (pour les versions 7 et +)
        private static byte[] GENERATOR_POLYNOMIAL = new byte[] { 1,1,1,1,1,0,0,1,0,0,1,0,1,0,0 };

        // Finder Pattern à mettre dans les coins TL, TR, BL
        private static int[,] FINDER_PATTERN = new int[,] { { 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1 } };
        
        // Pattern d'alignement à placer à des positions spécifiques pour chaque version
        private static int[,] ALIGNEMENT_PATTERN = new int[,] { { 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1 }, { 1, 0, 1, 0, 1 }, { 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1 } };

        // Octets pour compléter la partie données et atteindre la longueur voulue
        private static byte[] PAD_BYTES_1 = new byte[] { 1, 1, 1, 0, 1, 1, 0, 0 };
        private static byte[] PAD_BYTES_2 = new byte[] { 0, 0, 0, 1, 0, 0, 0, 1 };

        // Tableau qui contiendra les informations nécessaires pour générer chaque version
        // (longueur de chaîne max, nombre de blocs par groupe, nombre d'octets par bloc, ...)
        QRVersion[] versions;

        /// <summary>
        /// Permet d'obtenir les informations de versions à partir d'une instance de QRGenerator
        /// </summary>
        public QRVersion[] Versions
        {
            get { return versions;  }
        }

        /// <summary>
        /// Constructeur de QRGenerator
        /// La classe doit être instanciée pour pouvoir lire et créer des codes QR.
        /// En effet, un fichier est lu dans le constructeur et les informations contenues dedans sont nécessaires au fonctionnement des méthodes.
        /// </summary>
        public QRGenerator()
        {
            this.versions = new QRVersion[41];
            int compteur = 0;

            StreamReader sr = new StreamReader("../../../TD2/bin/Debug/versions_info/table.txt");
            string s = sr.ReadLine();
            while (s != null) {
                this.versions[compteur] = new QRVersion(compteur, s);
                compteur++;
                s = sr.ReadLine();
            }
            sr.Close();
        }

        

        /// <summary>
        /// Permet de générer un QRCode contenant une chaîne de caractères aléatoire
        /// </summary>
        /// <param name="length">Longueur de la chaîne attendue / Longueur aléatoire si non spécifié</param>
        /// <returns>Chaîne générée aléatoirement</returns>
        public int[,] GenerateRandom(int length = -1)
        {
            if (length < 0) length = new Random().Next(1, 4297);
            string input = Utils.RandomString(length);
            return this.Generate(input);
        }

        /// <summary>
        /// Encode une chaîne de caractères en vue de générer un code QR.
        /// Le nombres de bytes attendu est à préciser en paramètres avec la version de code QR.
        /// Le nombre de bits sur lequel indiquer la longueur de la chaîne varie en fonction de la version et elle doit donc être spécifiée.
        /// </summary>
        /// <param name="saisie">Chaîne à encoder</param>
        /// <param name="codewords_number">Nombre de bytes attendu</param>
        /// <param name="version">Version de code QR</param>
        /// <returns>Suite d'octets contenant les données encodées</returns>
        public static byte[] DataEncode(string saisie, int codewords_number, int version)
        {
            // Curseur pour se déplacer dans la chaîne
            int cursor = 0;

            byte[] data = new byte[8 * codewords_number];

            // Mode alphanumérique : 0010
            Utils.Fill(data, ALPHANUMERIC_MODE, cursor);
            cursor += 4;

            // Nombre de caractères, sur X bits
            int charCountBits = 9;
            if (version >= 10 && version < 27) charCountBits = 11;
            if (version >= 27 && version < 41) charCountBits = 13;
            Utils.Fill(data, Utils.ToBinary(saisie.Length, charCountBits), cursor);
            cursor += charCountBits;

            // Encodage de la chaîne de caractères
            int[] paires = new int[saisie.Length / 2 + ((saisie.Length % 2 == 0) ? 0 : 1)];
            for (int i = 0; i < saisie.Length; i += 2)
            {
                int s;
                int l;
                if (i + 1 < saisie.Length)
                {
                    // 2 LETTRES
                    s = 45 * ALPHANUMERIC_CHARS.IndexOf(saisie[i]) + ALPHANUMERIC_CHARS.IndexOf(saisie[i + 1]);
                    l = 11;
                }
                else
                {
                    // 1 LETTRE SEULEMENT
                    s = ALPHANUMERIC_CHARS.IndexOf(saisie[i]);
                    l = 6;
                }
                Utils.Fill(data, Utils.ToBinary(s, l), cursor);
                cursor += l;
            }

            // Ajout de zéros à la fin (maximum 4)
            if (codewords_number * 8 - cursor <= 4) cursor = codewords_number * 8;
            else cursor += 4;

            // Ajout de zéros pour obtenir un multiple de 8
            while (cursor % 8 != 0) cursor++;

            // Ajout de bytes pour compléter la partie données
            int c = 0;
            while (cursor < codewords_number * 8)
            {
                Utils.Fill(data, ((c % 2 == 0) ? PAD_BYTES_1 : PAD_BYTES_2), cursor);
                c++;
                cursor += 8;
            }

            // Conversion pour Reed Solomon
            byte[] words = new byte[codewords_number];
            for (int i = 0; i < codewords_number; i++)
            {
                // Copie dans un tableau temporaire
                byte[] word = new byte[8];
                for (int j = 0; j < word.Length; j++)
                {
                    word[j] = data[i * 8 + j];
                }
                words[i] = Utils.ToByte(word);
            }

            return words;

            //


        }

        /// <summary>
        /// A partir d'une chaîne de caractères, génère un code QR sous forme de matrice de 0 et de 1.
        /// Les 0 représentent les pixels blancs et les 1 représentent les pixels noirs.
        /// La matrice retournée peut-être passée à un constructeur de Img pour générer une image Bitmap.
        /// 
        /// Lire les commentaires au sein de la méthode pour en savoir plus sur le fonctionnement.
        /// </summary>
        /// <param name="input">Chaîne à encoder dans le code QR</param>
        /// <returns>Code QR</returns>
        public int[,] Generate(string input)
        {
            input = input.ToUpper();
            // Choix de la version en fonction de la longueur de la chaîne
            // La longueur de la chaîne est comparée à la capacité de chaque version
            if (input.Length > 4296) return null;
            int v = 0;
            while (input.Length > versions[v].Capacity) v++;

            // Encodage des données (le résultat est obtenu sans correction à cette étape)
            byte[] data_codewords = DataEncode(input, versions[v].Data_Length, v);

            /* Pour les QR Codes de version 5 et +,
             * Il est nécessaire de séparer les données en deux groupes puis en "blocs" au sein des deux groupes.
             * Le nombre de blocs et le nombre d'octets à mettre dans chaque bloc a été trouvé dans ce tableau :
             * https://www.thonky.com/qr-code-tutorial/error-correction-table
             * Nous avons stocké les données du tableau dans un fichier .txt qui est lu lorsque QRGenerator est instancié.
             * 
             * Ici les données sont donc séparées en différents blocs
             * Les blocs du groupe 1 sont d'abord remplis, puis ceux du groupe 2. On procède bloc par bloc au sein de chaque groupe.
             * 
             * Explication pour le nom des variables :
             * - Group1_Length est le nombre de blocs au sein du groupe 1
             * - Group1_BlockLength est le nombre d'octets dans un bloc du groupe 1
             * - Pareil pour le deuxième groupe
             * 
             * Voici la page qui permet de comprendre l'organisation des données dans un code QR :
             * https://www.thonky.com/qr-code-tutorial/structure-final-message
            */
            int compteur = 0;
            byte[][] group1 = new byte[versions[v].Group1_Length][];
            for(int i = 0; i < group1.Length; i++)
            {
                group1[i] = new byte[versions[v].Group1_BlockLength];
                for(int j = 0; j < group1[i].Length; j++)
                {
                    group1[i][j] = data_codewords[compteur];
                    compteur++;
                }
            }
            byte[][] group2 = new byte[versions[v].Group2_Length][];
            for (int i = 0; i < group2.Length; i++)
            {
                group2[i] = new byte[versions[v].Group2_BlockLength];
                for (int j = 0; j < group2[i].Length; j++)
                {
                    group2[i][j] = data_codewords[compteur];
                    compteur++;
                }
            }

            /* Pour la correction d'erreur, elle doit aussi se faire de façon organisée en groupes et blocs
             * On calcule les mots de correction pour chaque bloc.
             * Nous avons utilisé une organisation similaire aux variables group1 et group2
             */
            byte[][] group1_ec = new byte[versions[v].Group1_Length][];
            for (int i = 0; i < group1.Length; i++)
            {
                group1_ec[i] = ReedSolomonAlgorithm.Encode(group1[i], versions[v].EC_Length, ErrorCorrectionCodeType.QRCode);
            }
            byte[][] group2_ec = new byte[versions[v].Group2_Length][];
            for (int i = 0; i < group2.Length; i++)
            {
                group2_ec[i] = ReedSolomonAlgorithm.Encode(group2[i], versions[v].EC_Length, ErrorCorrectionCodeType.QRCode);
            }

            // Calcul de la largeur/hauteur de la matrice
            int length = 17 + v * 4;
            // Création de la matrice résultat
            int[,] code = new int[length, length];

            // Remplissage de "-1"s en valeur par défaut
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                    code[i, j] = -1;

            // Placement des Finder Patterns
            // TOP LEFT
            Utils.Fill(code, FINDER_PATTERN, 0, 0);
            // TOP RIGHT
            Utils.Fill(code, FINDER_PATTERN, (((v - 1) * 4) + 21) - 7, 0);
            // BOTTOM LEFT
            Utils.Fill(code, FINDER_PATTERN, 0, (((v - 1) * 4) + 21) - 7);

            // Placement des lignes blanches autour des Finder Pattern
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    bool TL = (i < 8 && j < 8);
                    bool TR = (i < 8 && j >= ((((v - 1) * 4) + 21) - 8));
                    bool BL = (i >= ((((v - 1) * 4) + 21) - 8) && j < 8);
                    if (code[i, j] == -1 && (TL || TR || BL)) code[i, j] = 0;
                }
            }

            /* Alignement Patterns
             * La position des Alignment Patterns est expliquée ici :
             * https://www.thonky.com/qr-code-tutorial/module-placement-matrix
             * Avant de placer un pattern, on vérifie qu'il y a la place et que ça ne déborde pas sur les modules déjà remplis (d'où la variable 'allowed')
             * Enfin le pattern est placé
             * 
             * Les positions sont stockées dans le même fichier que le nombre de blocs dans chaque groupe, la capacité, ect...
             */
            for (int i = 0; i < versions[v].PatternPositions.Count; i++)
            {
                for (int j = 0; j < versions[v].PatternPositions.Count; j++)
                {
                    // Vérification avant placement
                    int x = versions[v].PatternPositions[i];
                    int y = versions[v].PatternPositions[j];
                    bool allowed = true;
                    for (int k = x - ALIGNEMENT_PATTERN.GetLength(0) / 2; k <= x + ALIGNEMENT_PATTERN.GetLength(0) / 2; k++)
                    {
                        for (int l = y - ALIGNEMENT_PATTERN.GetLength(1) / 2; l <= y + ALIGNEMENT_PATTERN.GetLength(1) / 2; l++)
                        {
                            if (code[k, l] != -1)
                            {
                                allowed = false;
                                break;
                            }
                        }
                        if (!allowed) break;
                    }

                    if (allowed)
                    {
                        // on place le pattern
                        Utils.Fill(code, ALIGNEMENT_PATTERN, x - ALIGNEMENT_PATTERN.GetLength(0) / 2, y - ALIGNEMENT_PATTERN.GetLength(1) / 2);
                    }
                }
            }

            // Timing Pattern
            // Ce sont les deux lignes avec alternance de noir et blanc
            int c = 0;
            for (int i = 0; i < length; i++)
            {
                if (code[6, i] == -1) code[6, i] = (c % 2 == 0) ? 1 : 0;
                if (code[i, 6] == -1) code[i, 6] = (c % 2 == 0) ? 1 : 0;
                c++;
            }

            // Dark Module
            code[4 * v + 9, 8] = 1;

            // Reserved Areas
            // Format Information Area --> Les zones réservées au placement du "format" (masque + niveau de correction) sont remplies de -1 pour les réserver
     
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    bool TL = (i < 9 && j < 9);
                    bool TR = (i < 9 && j >= ((((v - 1) * 4) + 21) - 8));
                    bool BL = (i >= ((((v - 1) * 4) + 21) - 8) && j < 9);
                    if (code[i, j] == -1 && (TL || TR || BL)) code[i, j] = -2;
                }
            }
            // Version Information Area
            // Pour les QR Code de version 7+, des cases sont réservées pour indiquer la version.
            // Plus d'informations seront disponibles plus loin dans le code, lorsque ces cases seront remplies
            // Ici elles sont justes remplies de -2 pour les réserver
            if (v >= 7)
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        bool TR = (i < 6 && j >= ((((v - 1) * 4) + 21) - 11));
                        bool BL = (i >= ((((v - 1) * 4) + 21) - 11) && j < 6);
                        if (code[i, j] == -1 && (TR || BL)) code[i, j] = -2;
                    }
                }
            }

            /* Ajout du format (type de masque utilisé + niveau de correction)
             * Le masque et le niveau de correction étant toujours les mêmes, nous avons préféré coder la chaîne en dur
             * Cependant, si on voulait supporter d'autres masques et niveaux de corrections, le calcul des bits de cette chaîne
             * ressemble beaucoup au calcul pour les informations de version (effectué plus bas)
             */
            byte[] format = new byte[] { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            int cursor1 = 0;
            int cursor2 = format.Length - 1;
            for (int i = 0; i < length; i++)
            {
                if (code[8, i] == -2)
                {
                    code[8, i] = format[cursor1];
                    if (i != 8) cursor1++;
                }
                if (code[i, 8] == -2)
                {
                    code[i, 8] = format[cursor2];
                    cursor2--;
                }
                if (i == 8) cursor2--;
            }


            /* On convertit les données (données + correction) en bits pour pouvoir les placer
             * Avoir séparé les données en groupes et blocs prend tout son sens ici :
             * En effet, les blocs sont à placer dans un certaines ordres.
             * 
             * A l'origine, nous avons rempli les blocs d'octets 1 à 1 dans le groupe 1, puis 1 à 1 dans le groupe 2.
             * Ici l'ordre change, nous devons placer le premier octet du premier bloc, puis le premier du second bloc, ect... puis le premier octet du premier bloc du groupe2, le second, ect...
             * C'est compliqué d'expliquer le placement sans illustration. C'est pourquoi je me permets de vous rediriger ici :
             * https://www.thonky.com/qr-code-tutorial/structure-final-message
             * Le tableau au paragraphe "Interleave the Data Codewords" permet de bien comprendre l'organisation des "mots". Il faut le lire colonne par colonne alors qu'il a été rempli ligne par ligne.
             */

            compteur = 0;
            byte[] dataBits = new byte[8 * (versions[v].Data_Length + versions[v].EC_Length * (versions[v].Group1_Length + versions[v].Group2_Length))];
            for(int i = 0; i < Math.Max(versions[v].Group1_BlockLength, versions[v].Group2_BlockLength); i++)
            {
                for(int j = 0; j < group1.Length; j++)
                {
                    // On prend le i-ième de chaque bloc group1[j]
                    if(i < group1[j].Length)
                    {
                        Utils.Fill(dataBits, Utils.ToBinary(group1[j][i], 8), compteur);
                        compteur += 8;
                    }
                }
                for (int j = 0; j < group2.Length; j++)
                {
                    // On prend le i-ième de chaque bloc group2[j]
                    if (i < group2[j].Length)
                    {
                        Utils.Fill(dataBits, Utils.ToBinary(group2[j][i], 8), compteur);
                        compteur += 8;
                    }
                }
            }

            // Même fonctionnement pour les mots de correction
            for (int i = 0; i < versions[v].EC_Length; i++)
            {
                for (int j = 0; j < group1_ec.Length; j++)
                {
                    // On prend le i-ième de chaque bloc group1[j]
                    Utils.Fill(dataBits, Utils.ToBinary(group1_ec[j][i], 8), compteur);
                    compteur += 8;
                    
                }
                for (int j = 0; j < group2_ec.Length; j++)
                {
                    // On prend le i-ième de chaque bloc group2[j]
                    Utils.Fill(dataBits, Utils.ToBinary(group2_ec[j][i], 8), compteur);
                    compteur += 8;
                }
            }

            /* L'étape suivante est le placement des données dans chaque module du QR Code
             * Nous procédons toutes les deux colonnes
             * 
             * Comme la matrice est initialement remplie de -1, puis que les cases réservées sont remplies de 0, 1, ou -2,
             * Il est assez aisé de savoir si l'on se trouve sur un module déjà pris ou non
             * 
             * Nous procédons donc toutes les deux colonnes, puis ligne par ligne (en remontant ou descendant une fois sur deux),
             * En nous souciant simplement de vérifier que la case à remplir contient un "-1"
             * 
             * Nous tenons compte du masque 0 avec la vérification ( I + J ) % 2 == 0
             * Et du timing pattern vertical en vérifiant si i == 6 au début
             */
            int column = 0;
            int cursor = 0;
            for (int i = length - 1; i >= 0; i -= 2)
            {
                if (i == 6) i--;

                for (int j = 0; j < length; j++)
                {
                    // On calcule J en fonction du cas montant ou descendant
                    int J = (column % 2 == 0) ? length - 1 - j : j;
                    // Puis on remplit la ligne J aux colonnes i et i-1
                    if (code[J, i] == -1 && cursor < dataBits.Length)
                    {
                        code[J, i] = ((i + J) % 2 == 0) ? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                        cursor++;
                    }
                    
                    if (code[J, i - 1] == -1 && cursor < dataBits.Length)
                    {
                        code[J, i - 1] = ((i + J - 1) % 2 == 0) ? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                        cursor++;
                    }
                }

                column++;
            }

            /* Calcul des infos de version pour les QR Code de taille 7 et +
             * Lorsque j'écris ces commentaires j'ai oublié comment nous avons généré les bits qui donnent l'information de la verison,
             * Mais voilà la page qui nous a permis d'écrire ce code :
             * https://www.thonky.com/qr-code-tutorial/format-version-information
             * Il faut faire certaines opérations sur des chaînes de bits dont l'opération "XOR" (OU EXCLUSIF) que nous avons placé dans la classe Utils
             * 
             * NB: Les opérations qui suivent sont assez similaires au calcul du "Format String" qui précise masque & niveau de correction.
             */
            if (v >= 7)
            {
                byte[] binaryVersion = Utils.ToBinary(v);

                byte[] initialValue = new byte[binaryVersion.Length + 12];
                Utils.Fill(initialValue, binaryVersion, 0);

                while (initialValue.Length > 12)
                {
                    // Polynome
                    byte[] paddedGeneratorPolynomial = new byte[initialValue.Length];
                    Utils.Fill(paddedGeneratorPolynomial, GENERATOR_POLYNOMIAL, 0);

                    initialValue = Utils.XOR(initialValue, paddedGeneratorPolynomial);
                }

                byte[] finalResult = new byte[18];
                Utils.Fill(finalResult, initialValue, 18 - initialValue.Length);
                Utils.Fill(finalResult, Utils.ToBinary(v, 6), 0);

                // placement sur les cases -2 qu'il reste
                // Version Information Area
                int cur = finalResult.Length - 1;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                     {
                        bool TR = (i < 6 && j >= ((((v - 1) * 4) + 21) - 11)) && code[i, j] == -2;
                        if(TR)
                        {
                            if(cur >= 0)
                            {
                                code[i, j] = finalResult[cur];
                                code[j, i] = finalResult[cur];
                                cur--;
                            }
                        }
                    }
                }
            }

        // En fonction de la version, il peut rester des modules non remplis à la fin (jusqu'à 7)
        // Il suffit de les remplir en blanc
        // Infos ici au "Step 4" :
        // https://www.thonky.com/qr-code-tutorial/structure-final-message

            for (int i = 0; i < length; i++)
            {
                for(int j = 0; j < length; j++)
                {
                    if (code[i, j] != 0 && code[i, j] != 1) code[i, j] = 0;
                }
            }

            return code;
        }

        /// <summary>
        /// Affiche un code QR dans la Console en modifiant la couleur de fond et en affichant des espaces
        /// </summary>
        /// <param name="matrix">Code QR à afficher</param>
        public static void DisplayQR(int[,] matrix)
        {
            for(int i = -1; i <= matrix.GetLength(0); i++)
            {
                for(int j = -1; j <= matrix.GetLength(1); j++)
                {
                    if(i < 0 || j < 0 || j >= matrix.GetLength(1) || i >= matrix.GetLength(0))
                    {
                        if (Program.SUPPORT_CONSOLE_COLORS) Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("  ");
                    } else if(matrix[i,j] == 0)
                    {
                        if (Program.SUPPORT_CONSOLE_COLORS) Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("  ");
                    } else
                    {
                        if (Program.SUPPORT_CONSOLE_COLORS) Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }

            if (Program.SUPPORT_CONSOLE_COLORS) Console.ResetColor();
        }

        /// <summary>
        /// Lit un code QR à partir d'un fichier dont le chemin est passé en paramètre
        /// </summary>
        /// <param name="filename">Chemin du fichier</param>
        /// <returns>Contenu du code QR</returns>
        public string Read(string filename)
        {
            Img image = new Img(filename);
            int[,] code = image.ToQRCode();
            return this.Read(code);
        }

        /// <summary>
        /// Lit le contenu du code QR passé en paramètre (sous forme de matrice de 0 et 1)
        /// On procède en faisant l'inverse de la méthode de création de code QR
        /// On supprime les informations dans les zones réservées
        /// Lorsque tout est supprimé, on lit les données en partant du début.
        /// 
        /// Les données sont réorganisées dans le bon ordre avec le même système de groupes que pour la création de code QR.
        /// 
        ///  Nous n'utilisons (malheureusement) pas la correction présente dans le code QR pour corriger d'éventuelles erreurs de lecture (par la classe Img qui convertir le Bmp en matrice)
        /// </summary>
        /// <param name="code0">Matrice représentant un code QR</param>
        /// <returns>Contenu du code QR</returns>
        public string Read(int[,] code0)
        {
            // Copie dans une nouvelle matrice modifiable
            int length = code0.GetLength(0);
            int[,] code = new int[length, length];
            for(int i = 0; i < length; i++)
            {
                for(int j = 0; j < length; j++)
                {
                    code[i, j] = code0[i, j];
                }
            }

            // Recherche
            int v = (length - 17) / 4;

            // Suppression des 3 Finder Pattern + Zone réservées au masque
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    code[i, j] = -1;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    code[i, length - 1 - j] = -1;
                    code[length - 1 - j, i] = -1;
                }
            }

            // Suppression des Alignement Patterns
            for (int i = 0; i < versions[v].PatternPositions.Count; i++)
            {
                for (int j = 0; j < versions[v].PatternPositions.Count; j++)
                {
                    // Vérification avant placement
                    int x = versions[v].PatternPositions[i];
                    int y = versions[v].PatternPositions[j];
                    bool allowed = true;
                    for (int k = x - ALIGNEMENT_PATTERN.GetLength(0) / 2; k <= x + ALIGNEMENT_PATTERN.GetLength(0) / 2; k++)
                    {
                        for (int l = y - ALIGNEMENT_PATTERN.GetLength(1) / 2; l <= y + ALIGNEMENT_PATTERN.GetLength(1) / 2; l++)
                        {
                            if (code[k, l] == -1)
                            {
                                allowed = false;
                                break;
                            }
                        }
                        if (!allowed) break;
                    }

                    if (allowed)
                    {
                        // on supprime le pattern
                        for (int k = x - ALIGNEMENT_PATTERN.GetLength(0) / 2; k <= x + ALIGNEMENT_PATTERN.GetLength(0) / 2; k++)
                        {
                            for (int l = y - ALIGNEMENT_PATTERN.GetLength(1) / 2; l <= y + ALIGNEMENT_PATTERN.GetLength(1) / 2; l++)
                            {
                                code[k, l] = -1;
                            }
                            if (!allowed) break;
                        }
                    }
                }
            }

            // Timing Pattern

            int c = 0;
            for (int i = 0; i < length; i++)
            {
                if (code[6, i] != -1) code[6, i] = -1;
                if (code[i, 6] != -1) code[i, 6] = -1;
                c++;
            }

            // Dark Module
            code[4 * v + 9, 8] = -1;

            // Version area
            if(v >= 7)
            {
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        code[i, length - 1 - j] = -1;
                        code[length - 1 - j, i] = -1;
                    }
                }
            }




            // Lecture

            /* AMELIORATION POSSIBLE
             * La lecture se faire toutes les 2 colonnes comme pour l'écriture
             * Cependant nous avons conscience que le code peut être amélioré en terme de lisibilité et peut être simplifié aussi au niveau du fonctionnement
             * 
             * Nous l'avons fait pour la méthode d'écriture dans le code QR mais pas ici par manque de temps.
             * 
             * Ca reste tout de même parfaitement fonctionnel
             */

            byte[] dataBits = new byte[8 * (versions[v].Data_Length + versions[v].EC_Length * (versions[v].Group1_Length + versions[v].Group2_Length))];

            int column = 0;
            int cursor = 0;
            for (int i = length - 1; i >= 0; i -= 2)
            {
                if (i == 6) i--;
                if (column % 2 == 0)
                {
                    // Cas montant
                    // On lit les colonnes i et i-1
                    for (int j = length - 1; j >= 0; j--)
                    {
                        if (code[j, i] != -1 && cursor < dataBits.Length)
                        {
                            dataBits[cursor] = ((i+j)%2 == 0)? ( (code[j,i] == 0) ? (byte)1: (byte)0 ) : (byte)code[j,i] ;
                            cursor++;
                        }
                        if (code[j, i - 1] != -1 && cursor < dataBits.Length)
                        {
                            dataBits[cursor] = ((i + j - 1) % 2 == 0) ? ((code[j, i - 1] == 0) ? (byte)1 : (byte)0) : (byte)code[j, i - 1];
                            cursor++;
                        }

                    }

                }
                else
                {
                    // Cas descendant

                    for (int j = 0; j < length; j++)
                    {
                        if (code[j, i] != -1 && cursor < dataBits.Length)
                        {
                            dataBits[cursor] = ((i + j) % 2 == 0) ? ((code[j, i] == 0) ? (byte)1 : (byte)0) : (byte)code[j, i];
                            cursor++;
                        }
                        if (code[j, i - 1] != -1 && cursor < dataBits.Length)
                        {
                            dataBits[cursor] = ((i + j - 1) % 2 == 0) ? ((code[j, i - 1] == 0) ? (byte)1 : (byte)0) : (byte)code[j, i - 1];
                            cursor++;
                        }
                    }

                }
                column++;
            }

            byte[] dataBytes = new byte[(versions[v].Data_Length + versions[v].EC_Length * (versions[v].Group1_Length + versions[v].Group2_Length))];
            for(int i = 0; i < dataBits.Length; i += 8) {
                byte[] oct = new byte[8];
                for(int j = 0; j < 8; j++)
                {
                    oct[j] = dataBits[i + j];
                }
                dataBytes[i / 8] = Utils.ToByte(oct);
            }

            int compteur = 0; 

            byte[][] group1 = new byte[versions[v].Group1_Length][];
            byte[][] group2 = new byte[versions[v].Group2_Length][];
            for (int i = 0; i < group1.Length; i++)
                group1[i] = new byte[versions[v].Group1_BlockLength];
            for (int i = 0; i < group2.Length; i++)
                group2[i] = new byte[versions[v].Group2_BlockLength];

            for(int i = 0; i < Math.Max(versions[v].Group1_BlockLength, versions[v].Group2_BlockLength); i++)
            {
                for(int j = 0; j < group1.Length; j++)
                {
                    if(i < group1[j].Length)
                    {
                        group1[j][i] = dataBytes[compteur];
                        compteur++;
                    }
                }
                for (int j = 0; j < group2.Length; j++)
                {
                    if (i < group2[j].Length)
                    {
                        group2[j][i] = dataBytes[compteur];
                        compteur++;
                    }
                }
            }

            compteur = 0;
            byte[] orderedBytes = new byte[dataBytes.Length];
            for(int i = 0; i < group1.Length; i++)
            {
                for(int j = 0; j < group1[i].Length; j++)
                {
                    orderedBytes[compteur] = group1[i][j];
                    compteur++;
                }
            }
            for (int i = 0; i < group2.Length; i++)
            {
                for (int j = 0; j < group2[i].Length; j++)
                {
                    orderedBytes[compteur] = group2[i][j];
                    compteur++;
                }
            }

            byte[] orderedBits = new byte[orderedBytes.Length * 8];
            for(int i = 0; i < orderedBytes.Length; i++)
            {
                byte[] octet = Utils.ToBinary(orderedBytes[i], 8);
                Utils.Fill(orderedBits, octet, i * 8);
            }

            // Maintenant qu'on a la suite de 0 et 1 dans le bon ordre on peut décoder

            // 4 premiers bits = mode alphanumérique // On lit pas cette fois ci
            cursor = 4;

            // bits suivant = longueur de la chaine
            int charCountBits = 9;
            if (v >= 10 && v < 27) charCountBits = 11;
            if (v >= 27 && v < 41) charCountBits = 13;
            byte[] charLengthBits = Utils.Copy(orderedBits, cursor, charCountBits);
            cursor += charCountBits;
            int charLength = Utils.ToInt(charLengthBits);

            // Récupération des caractères

            string resultat = "";

            for(int i = 0; i < charLength; i+=2)
            {
                if(i != charLength - 1)
                {
                    int numberRepresentation = Utils.ToInt(Utils.Copy(orderedBits, cursor, 11));
                    int N1 = numberRepresentation / 45;
                    int N2 = numberRepresentation % 45;
                    resultat += ALPHANUMERIC_CHARS[N1];
                    resultat += ALPHANUMERIC_CHARS[N2];
                    cursor += 11;
                } else
                {
                    int numberRepresentation = Utils.ToInt(Utils.Copy(orderedBits, cursor, 6));
                    resultat += ALPHANUMERIC_CHARS[numberRepresentation];
                    cursor += 6;
                }

            }

            return resultat;

        }
    }
}
