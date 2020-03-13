using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
/*
 * Klasse zum Bereitstellen eines Grundgerüstes zur Speicherung von texten und zugehöriger Farbwerte die als Highlights gekennzeichnet werden sollen
 */
namespace SEdi
{
    [Serializable]
    public class ResourceElement
    {
        //Variablen Deklaration
        String Text;
        Color col;

        /*
         * Funktion zum erzeugen einer Instanz mit übergebenen Werten für Text und Farbe
         */
        public ResourceElement(String text, Color color)
        {
            //übergebene Werte in die innerhalb dieser Instanz ferfügbaren Variablen schreiben
            Text = text;
            col = color;
        }

        /*
         * Funktion zum erzeugen einer Instanz ohne übergebene Werte
         */
        public ResourceElement()
        {
            //wenn eine Instanz erzeugt wird, ohne Werte zu übergeben, dann wird ein leerer String und die Farbe Schwarz in die Instanz Variablen geschrieben
            Text = "";
            col = Color.Black;
        }

        /*
         * Funktion um die Farbe dieser Instanz zurück zu geben
         */
        public Color getColor()
        {
            return col;
        }
        /*
         * Funktion um den Text dieser Instanz zurück zu geben
         */

        public String getText()
        {
            return Text;
        }
    }
}
