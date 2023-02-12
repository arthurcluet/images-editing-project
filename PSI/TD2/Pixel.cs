using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD2
{
    /// <summary>
    /// Type représentant un pixel grâce à trois champs Rouge, Vert et Bleu
    /// </summary>
    public class Pixel
    {
        byte b;
        byte g;
        byte r;

        /// <summary>
        /// Crée un pixel à partir de trois valeurs R,V,B
        /// </summary>
        /// <param name="b">Bleu</param>
        /// <param name="g">Vert</param>
        /// <param name="r">Rouge</param>
        public Pixel(byte b, byte g, byte r)
        {
            this.b = b;
            this.g = g;
            this.r = r;
        }

        /// <summary>
        /// Propriété BLEU accessible en lecture et écriture
        /// </summary>
        public byte B
        {
            get { return this.b; }
            set { this.b = value; }
        }

        /// <summary>
        /// Propriété VERT accessible en lecture et écriture
        /// </summary>
        public byte G
        {
            get { return this.g; }
            set { this.g = value; }
        }

        /// <summary>
        /// Propriété ROUGE accessible en lecture et écriture
        /// </summary>
        public byte R
        {
            get { return this.r; }
            set { this.r = value; }
        }

        /// <summary>
        /// Permet de modifier les valeurs R,V,B d'une instance de Pixel
        /// </summary>
        /// <param name="b">Bleu</param>
        /// <param name="g">Vert</param>
        /// <param name="r">Rouge</param>
        public void SetPixel(byte b, byte g, byte r)
        {
            this.b = b;
            this.g = g;
            this.r = r;
        }

        /// <summary>
        /// Pour afficher un pixel dans la console, retourne un caractère différent si le pixel est clair ou foncé
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            int moyenne = (this.b + this.g + this.r) / 3;
            return (moyenne < 128) ? "-" : "#";
        }
    }
}
