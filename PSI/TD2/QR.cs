using ReedSolomon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD2
{
    class QR
    {
        // Encodage des données

        private static byte[] ALPHANUMERIC_MODE = new byte[] { 0, 0, 1, 0 };
        private static string ALPHANUMERIC_CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

        private static int[] L_CAPACITIES = new int[] { 0, 25, 47, 77, 114, 154, 195, 224, 279, 335, 395, 468, 535, 619, 667, 758, 854, 938, 1046, 1153, 1249 };
        private static int[] L_DATA_CODEWORDS_NUMBER = new int[] { 0, 19, 34, 55, 80, 108, 136, 156, 194, 232, 274, 324, 370, 428, 461, 523, 589, 647, 721, 795, 861 };
        private static int[] L_EC_CODEWORDS_NUMBER = new int[] { 0, 7, 10, 15, 20, 26, 18 , 20, 24, 30};

        private static int[,] FINDER_PATTERN = new int[,] { { 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1 } };
        private static int[,] ALIGNEMENT_PATTERN = new int[,] { { 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1 }, { 1, 0, 1, 0, 1 }, { 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1 } };


        private static byte[] PAD_BYTES_1 = new byte[] { 1,1,1,0,1,1,0,0 };
        private static byte[] PAD_BYTES_2 = new byte[] { 0,0,0,1,0,0,0,1 };

        private static void Fill(byte[] array, byte[] content, int index)
        {
            if(index >= 0)
                for (int i = 0; i < content.Length; i++)
                    if (i + index < array.Length) array[i + index] = content[i];
        }

        private static void Fill(int[,] matrix, int[,] content, int x, int y)
        {
            if (x >= 0 && y >= 0)
                for (int i = 0; i < content.GetLength(0); i++)
                    for (int j = 0; j < content.GetLength(1); j++)
                        if (x + i < matrix.GetLength(0) && y + j < matrix.GetLength(1)) matrix[i + x, j + y] = content[i, j];
        }

        public static string ToString(byte[] b)
        {
            string s = "";
            for(int i = 0; i < b.Length; i++)
            {
                s += b[i] + " ";
            }
            return s;
        }

        private static int Max(int binaryLength)
        {
            int S = 0;
            for(int i = 0; i < binaryLength; i++)
            {
                S += (int)Math.Pow(2, i);
            }
            return S;
        }

        public static byte[] ToBinary(int a, int length = -1)
        {
            if (length > 0) length++;
            int pMax = length - 1;
            while (a > Max(pMax) || pMax < 0) pMax++;
            byte[] tab = new byte[pMax];
            for(int i = pMax - 1; i >= 0; i--)
            {
                if (a >= Math.Pow(2, i))
                {
                    tab[pMax -1 - i] = 1;
                    a -= (int)Math.Pow(2, i);
                }

            }
            return tab;
        }

        public static byte ToByte(byte[] a)
        {
            int b = 0;
            int c = 0;
            for(int i = a.Length - 1; i >= 0; i--)
            {
                b += (int)a[i] * (int)Math.Pow(2, c);
                c++;
            }

            return (byte)b;
        }

        public static byte[] DataEncode(string saisie, int codewords_number)
        {
            // Curseur pour se déplacer dans la chaîne
            int cursor = 0;

            byte[] data = new byte[8 * codewords_number];

            // Mode alphanumérique : 0010
            Fill(data, ALPHANUMERIC_MODE, cursor);
            cursor += 4;

            // Nombre de caractères, sur 9 bits :
            Fill(data, ToBinary(saisie.Length, 9), cursor);
            cursor += 9;

            // Encodage de la chaîne de caractères
            int[] paires = new int[saisie.Length/2 + ((saisie.Length % 2 == 0) ? 0 : 1)];
            for(int i = 0; i < saisie.Length; i+=2)
            {
                int s;
                int l;
                if(i+1 < saisie.Length)
                {
                    // 2 LETTRES
                    s = 45 * ALPHANUMERIC_CHARS.IndexOf(saisie[i]) + ALPHANUMERIC_CHARS.IndexOf(saisie[i+1]);
                    l = 11;
                } else
                {
                    // 1 LETTRE SEULEMENT
                    s = ALPHANUMERIC_CHARS.IndexOf(saisie[i]);
                    l = 6;
                }
                Fill(data, ToBinary(s, l), cursor);
                cursor += l;
            }

            // Ajout de zéros à la fin (maximum 4)
            if (codewords_number * 8 - cursor <= 4) cursor = codewords_number * 8;
            else cursor += 4;

            // Ajout de zéros pour obtenir un multiple de 8
            while (cursor % 8 != 0) cursor++;

            // Ajout de bytes pour compléter la partie données
            int c = 0;
            while(cursor < codewords_number * 8)
            {
                Fill(data, ((c % 2 == 0)?PAD_BYTES_1:PAD_BYTES_2), cursor);
                c++;
                cursor += 8;
            }

            // Conversion pour Reed Solomon
            byte[] words = new byte[codewords_number];
            for(int i = 0; i < codewords_number; i++)
            {
                // Copie dans un tableau temporaire
                byte[] word = new byte[8];
                for(int j = 0; j < word.Length; j++)
                {
                    word[j] = data[i*8 + j];
                }
                words[i] = ToByte(word);
            }

            return words;

            //

        
        }



        public static int[,] Generate(string input)
        {
            int[][] ALIGNEMENT_PATTERN_LOCATIONS = new int[10][];
            ALIGNEMENT_PATTERN_LOCATIONS[0] = new int[0];
            ALIGNEMENT_PATTERN_LOCATIONS[1] = new int[0];
            ALIGNEMENT_PATTERN_LOCATIONS[2] = new int[] { 6, 18 };
            ALIGNEMENT_PATTERN_LOCATIONS[3] = new int[] { 6, 22 };
            ALIGNEMENT_PATTERN_LOCATIONS[4] = new int[] { 6, 26 };
            ALIGNEMENT_PATTERN_LOCATIONS[5] = new int[] { 6, 30 };
            ALIGNEMENT_PATTERN_LOCATIONS[6] = new int[] { 6, 34 };
            ALIGNEMENT_PATTERN_LOCATIONS[7] = new int[] { 6, 22, 38 };
            ALIGNEMENT_PATTERN_LOCATIONS[8] = new int[] { 6, 24, 42 };
            ALIGNEMENT_PATTERN_LOCATIONS[9] = new int[] { 6, 26, 46 };


            // Choix de la version en fonction de la capacité
            if (input.Length > 1249) return null;
            int version = 0;
            while (input.Length > L_CAPACITIES[version]) version++;

            Console.WriteLine(version);

            // Longueur de mots pour la partie Données
            int codewords_number = L_DATA_CODEWORDS_NUMBER[version];

            // Données encodées
            byte[] data_codewords = DataEncode(input, codewords_number);
            // Correction d'erreurs
            byte[] EC = ReedSolomonAlgorithm.Encode(data_codewords, L_EC_CODEWORDS_NUMBER[version], ErrorCorrectionCodeType.QRCode);

            // Création de la matrice
            int length = 17 + version * 4;
            int[,] code = new int[length, length];

            // Remplissage de "-1"s en valeur par défaut
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                    code[i, j] = -1;

            // Placement des Finder Patterns
            // TOP LEFT
            Fill(code, FINDER_PATTERN, 0, 0);
            // TOP RIGHT
            Fill(code, FINDER_PATTERN, (((version - 1) * 4) + 21) - 7, 0);
            // BOTTOM LEFT
            Fill(code, FINDER_PATTERN, 0, (((version - 1) * 4) + 21) - 7);

            // Placement des lignes blanches autour
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    bool TL = (i < 8 && j < 8);
                    bool TR = (i < 8 && j >= ((((version - 1) * 4) + 21) - 8));
                    bool BL = (i >= ((((version - 1) * 4) + 21) - 8) && j < 8);
                    if (code[i, j] == -1 && (TL || TR || BL)) code[i, j] = 0;
                }
            }

            // Alignement Patterns
            for(int i = 0; i < ALIGNEMENT_PATTERN_LOCATIONS[version].Length; i++)
            {
                for (int j = 0; j < ALIGNEMENT_PATTERN_LOCATIONS[version].Length; j++)
                {
                    // Vérification avant placement
                    int x = ALIGNEMENT_PATTERN_LOCATIONS[version][i];
                    int y = ALIGNEMENT_PATTERN_LOCATIONS[version][j];
                    bool allowed = true;
                    for(int k = x - ALIGNEMENT_PATTERN.GetLength(0)/2; k <= x + ALIGNEMENT_PATTERN.GetLength(0)/2; k++)
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
                        Fill(code, ALIGNEMENT_PATTERN, x - ALIGNEMENT_PATTERN.GetLength(0) / 2, y - ALIGNEMENT_PATTERN.GetLength(1) / 2);
                    }
                }
            }

            // Timing Pattern

            int c = 0;
            for(int i = 0; i < length; i++)
            {
                if (code[6, i] == -1) code[6, i] = (c % 2 == 0) ? 1 : 0;
                if (code[i, 6] == -1) code[i, 6] = (c % 2 == 0) ? 1 : 0;
                c++;
            }

            // Dark Module
            code[4 * version + 9, 8] = 1;

            // Reserved Areas
            // Format Information Area
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    bool TL = (i < 9 && j < 9);
                    bool TR = (i < 9 && j >= ((((version - 1) * 4) + 21) - 8));
                    bool BL = (i >= ((((version - 1) * 4) + 21) - 8) && j < 9);
                    if (code[i, j] == -1 && (TL || TR || BL)) code[i, j] = -2;
                }
            }
            // Version Information Area
            if(version >= 7)
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        bool TR = (i < 6 && j >= ((((version - 1) * 4) + 21) - 11));
                        bool BL = (i >= ((((version - 1) * 4) + 21) - 11) && j < 6);
                        if (code[i, j] == -1 && (TR || BL)) code[i, j] = -2;
                    }
                }
            }

            // On convertit les données (données + correction) en bits
            byte[] dataBits = new byte[(data_codewords.Length + EC.Length) * 8];
            for(int i = 0; i < data_codewords.Length; i++)
            {
                byte[] binary = ToBinary(data_codewords[i], 8);
                for (int j = 0; j < binary.Length; j++)
                    dataBits[i * 8 + j] = binary[j];
            }
            for (int i = 0; i < EC.Length; i++)
            {
                byte[] binary = ToBinary(EC[i], 8);
                for (int j = 0; j < binary.Length; j++)
                    dataBits[(data_codewords.Length + i) * 8 + j] = binary[j];
            }

            Console.WriteLine(data_codewords.Length + "words");
            Console.WriteLine(EC.Length + "words");

            // On les place
            // Toutes les deux colonnes
            int column = 0;
            int cursor = 0;
            int lastLine = length - 1;
            for(int i = length - 1; i >= 0; i -= 2)
            {
                //Console.WriteLine(i);
                if (i == 6) i--;
                if(column % 2 == 0)
                {
                    // Cas montant
                    // On remplit les colonnes i et i-1
                    for(int j = lastLine; j >= 0; j--)
                    {
                        if (code[j, i] == -1 && cursor < dataBits.Length)
                        {
                            code[j, i] = ((i+j) % 2 == 0)? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                            cursor++;
                            lastLine = j;
                        }
                        if (code[j, i - 1] == -1 && cursor < dataBits.Length)
                        {
                            code[j, i - 1] = ((i + j - 1) % 2 == 0) ? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                            cursor++;
                            lastLine = j;
                        }

                    }
                    
                } else
                {
                    // Cas descendant

                    for (int j = lastLine; j < length; j++)
                    {
                        if (code[j, i] == -1 && cursor < dataBits.Length)
                        {
                            code[j, i] = ((i + j) % 2 == 0) ? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                            cursor++;
                            lastLine = j;
                        }
                        if (code[j, i - 1] == -1 && cursor < dataBits.Length)
                        {
                            code[j, i - 1] = ((i + j - 1) % 2 == 0) ? (dataBits[cursor] == 1 ? 0 : 1) : (dataBits[cursor]);
                            cursor++;
                            lastLine = j;
                        }
                    }

                }
                column++;
            }

            //Console.WriteLine(cursor);
            //Console.WriteLine(dataBits.Length);
            //Console.WriteLine(ToString(dataBits));

            // Ajout du format
            
             byte[] format = new byte[] { 1,1,1,0,1,1,1,1,1,0,0,0,1,0,0 };
            int cursor1 = 0;
            int cursor2 = format.Length - 1;
            for(int i = 0; i < length; i++)
            {
                if(code[8, i] == -2)
                {
                    code[8, i] = format[cursor1];
                    if(i != 8) cursor1++;
                }
                if(code[i, 8] == -2)
                {
                    code[i, 8] = format[cursor2];
                    cursor2--;
                }
                if (i == 8) cursor2--;
            }
             

            return code;
        }

    }
}
