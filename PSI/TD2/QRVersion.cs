using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD2
{
    /// <summary>
    /// Type permettant de stocker les informations à propos d'une version de Code QR
    /// </summary>
    public class QRVersion
    {
        int version;

        int capacity;

        int data;
        int ec;

        int group1_length;
        int group1_blockLength;
        int group2_length;
        int group2_blockLength;

        List<int> alignment_patterns;

        /// <summary>
        /// Constructeur de QR Version
        /// Remplit les informations à partir d'une chaîne de caractère (une ligne du fichier ./versions_info/table.txt)
        /// </summary>
        /// <param name="v">Version du code QR</param>
        /// <param name="input">Chaîne décrivant la version</param>
        public QRVersion(int v, string input)
        {
            this.version = v;

            string[] table_data = input.Split(' ');
            if (table_data.Length == 8)
            {
                this.capacity = Convert.ToInt32(table_data[0]);
                this.data = Convert.ToInt32(table_data[1]);
                this.ec = Convert.ToInt32(table_data[2]);
                this.group1_length = Convert.ToInt32(table_data[3]);
                this.group1_blockLength = Convert.ToInt32(table_data[4]);
                this.group2_length = Convert.ToInt32(table_data[5]);
                this.group2_blockLength = Convert.ToInt32(table_data[6]);

                string[] positions = table_data[7].Split(',');
                this.alignment_patterns = new List<int>();
                for (int i = 0; i < positions.Length; i++)
                {
                    if(positions.Length > 1)
                    {
                        this.alignment_patterns.Add(Convert.ToInt32(positions[i]));
                    }
                }
            }
        }

        /// <summary>
        /// Permet d'obtenir des informations sur la version sous forme de chaîne de caractères
        /// Méthode utilisée à des fins de tests
        /// </summary>
        /// <returns>Chaîne décrivant la version</returns>
        public string toString()
        {
            return "Version " + version + " - Capacité : " + this.capacity;
        }

        /// <summary>
        /// N° de version
        /// </summary>
        public int Version {
            get { return this.version; }
        }

        /// <summary>
        /// Capacité (en mode alphanumérique) de cette version
        /// </summary>
        public int Capacity
        {
            get { return this.capacity; }
        }

        /// <summary>
        /// Nombre de mots de données requis pour cette version
        /// </summary>
        public int Data_Length
        {
            get { return this.data;  }
        }

        /// <summary>
        /// Nombre de mots de corrections par bloc
        /// </summary>
        public int EC_Length
        {
            get { return this.ec; }
        }

        /// <summary>
        /// Nombre de blocs du groupe 1
        /// </summary>
        public int Group1_Length
        {
            get { return this.group1_length; }
        }

        /// <summary>
        /// Nombre de mots dans un bloc du groupe 1
        /// </summary>
        public int Group1_BlockLength
        {
            get { return this.group1_blockLength; }
        }

        /// <summary>
        /// Nombre de blocs du groupe 2
        /// </summary>
        public int Group2_Length
        {
            get { return this.group2_length; }
        }

        /// <summary>
        /// Nombre de mots dans un bloc du groupe 2
        /// </summary>
        public int Group2_BlockLength
        {
            get { return this.group2_blockLength; }
        }

        /// <summary>
        /// Coordonnées permettant d'obtenir la position des Alignment Patterns
        /// </summary>
        public List<int> PatternPositions
        {
            get { return this.alignment_patterns;  }
        }
    }
}
