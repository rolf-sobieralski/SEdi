using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
/*
 * Name: Rolf Sobieralski
 * Datum: 11.03.2020
 * 
 * Beschreibung:
 * ein kleiner Text Editor mit selbst konfigurierbarem Syntax Highlighting
 * 
 * 
 */

namespace SEdi
{
    public partial class Form1 : Form
    {
        //Deklaration und Initialisierung der klassen verfügbaren Variablen
        List<ResourceElement> Highlights;
        bool changed = false;
        bool loading = false;
        String s_FileName = "";
        public Form1()
        {
            InitializeComponent();
            //evtl. vorhandene Highlighting Einstellungen laden
            ReadHighlights();
        }

        /*
         * Funktion zum Einlesen der bereits konfigurierten Highlighs
         * */
        void ReadHighlights()
        {
            //Datei mit Hihglights öffnen und, als Stream verfügbar machen und als Resourcen Liste einlesen
            Stream s = File.Open(Application.StartupPath + "\\Highlights.seh",FileMode.OpenOrCreate);
            BinaryFormatter b = new BinaryFormatter();
            Highlights = (List<ResourceElement>)b.Deserialize(s);
            s.Close();
        }
        /*
         * Funktion zum verarbeiten der Text änderung im RichTextBox Steuerelement
         */
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!changed)
            {
                //wenn changed false ist, wird changed geändert und an den Fenster Titel ein * angehängt um zu kennzeichnen, das die Datei seit dem letzen speichern 
                //oder öffnen bearbeitet wurde
                changed = true;
                this.Text += "*";
            }
            if (!loading)
            {
                //prüfung ob es sich bei der aktuellen änderung im Dokument um einen Datei Ladevorgang handelt. falls nicht werden die Highlights in der aktuellen Reihe geprüft
                checkHighlights(richTextBox1);
            }
            //aufruf der Funktion zum aktualisieren der Infos in der Statusleiste
            setall();
        }
        
        /*
         * Funktion zum einfärben der texte in Anführungszeichen
         */
        private void checkQuotes(RichTextBox RT)
        {
            //aktuelle Cursor Position im Dokument festhalten
            int curPos = RT.SelectionStart;
            if (RT.Lines.Length > 0)
            {
                //Anführungszeichen werden nur geprüft wenn auch Text in der RichTextBox vorhanden ist
                int rowIndex = RT.GetLineFromCharIndex(curPos);
                Color c = Color.Black;
                int CharsToLineStart = 0;//Variable zum Speichern der Anzahl zeichen, die zum Start der Reihe fehlen
                for(int i = 0; i < rowIndex; i++)
                {
                    //Zählschleife durch die Reihen bis zur Cursor Position. die Anzahl der zeichen in der Reihe wird in der Variablen festgehalten
                    CharsToLineStart += richTextBox1.Lines[i].Length;
                }
                //Variablen deklaration um die Position des ersten und zweiten Anführungszeichens in der Reihe fest zu halten
                int firstQuote = RT.Lines[rowIndex].IndexOf('"', 0);
                int secondQuote = RT.Lines[rowIndex].IndexOf('"', firstQuote + 1);
                foreach (ResourceElement r in Highlights)
                {
                    //Schleife um durch alle vorhandenen Text, Farb einträge zu laufen
                    if (r.getText() == "\"*\"")
                    {
                        //prüfung ob aktuelles Element für die Anführungszeichen ist. Farbe wird in der Variablen gespeichert
                        c = r.getColor();
                    }
                }
                while (RT.Lines[rowIndex].IndexOf('"', firstQuote + 1) != -1 && firstQuote != -1)
                {
                    //Schleife wird so lange durchlaufen bis keine öffnenden oder schließenden Anführungszeichen mehr vorhanden sind
                    if (firstQuote > -1)
                    {
                        //Prüfung eine gültige Position für das erste Anführungszeichen vorhanden ist
                        //schließendes Anführungszeichen suchen und position in der Variablen speichern
                        secondQuote = RT.Lines[rowIndex].IndexOf('"', firstQuote + 1);
                    }
                    else
                    {
                        //wenn kein erstes Anführungszeichen mehr da ist, wird variable für das zweite auch -1 gesetzt
                        secondQuote = -1;
                    }
                    if (secondQuote > -1)
                    {
                        //falls ein zweites Anführungszeichen vorhanden ist, wird der bereich zwischen dem esten und dem zweiten markiert und die Farbe auf 
                        //die oben gefunden wurde setzen. anschließend aktuelle Cursor Position auswählen und die Farbe wieder auf Schwarz ändern
                        RT.Select(CharsToLineStart + firstQuote + 2, (secondQuote - firstQuote) + 1);
                        RT.SelectionColor = c;
                        RT.Select(curPos, 0);
                        RT.SelectionColor = Color.Black;
                       firstQuote = RT.Lines[rowIndex].IndexOf('"', secondQuote + 1);
                    }
                }
            }
        }

        /*
         * Funktion um alle Anführungszeichen im Text in die Passende Highlight Farbe zu setzen.
         * Diese Funktion wird nur genutzt, wenn eine Komplette Textdatei geladen wird
         */
        private void checkAllQuotes(RichTextBox RT)
        {
            //aktuelle Cursor Position speichern
            int curPos = richTextBox1.SelectionStart;
            if (RT.Lines.Length > 0)
            {
                //nach Anführungszeichen wird nur gesucht, wenn das Steuerelement auch text beinhaltet
                Color c = Color.Black;
                foreach (ResourceElement r in Highlights)
                {
                    //Schleife um alle definierten Highlights zu durchlaufen
                    if (r.getText() == "\"*\"")
                    {
                        //falls das aktuelle Highlight den Anführungszeichen entspricht wird die zugewiesene Farbe in der Variable gespeichert
                        c = r.getColor();
                    }
                }
                //Deklaration und Initialisierung der Positionen für das öffnende und Schließende Anführungszeichen
                int firstQuote = RT.Text.IndexOf('"', 0);
                int secondQuote = RT.Text.IndexOf('"', firstQuote + 1);
                while (RT.Text.IndexOf('"', firstQuote + 1) != -1 && firstQuote != -1)
                {
                    //Schleife wird so lange durchlaufen bis entweder die Variable für das Öffnende oder das Schließende Anführungszeichen keinen gültigen Wert mehr hat
                    if (firstQuote > -1)
                    {
                        //zweites Anführungszeichen suchen und Position speichern, falls die Variable für das erste Anführungszeichen einen Gültigen wert hat
                        secondQuote = RT.Text.IndexOf('"', firstQuote + 1);
                    }
                    else
                    {
                        //falls keine gültige Position vorhanden ist, wird die Variable für das Zweite Anführungszeichen auch auf eine ungültige Position gesetzt
                        secondQuote = -1;
                    }
                    if (secondQuote > -1)
                    {
                        //wenn ein zweites Anführungszeichen vorhanden ist, wird der entsprechende Bereich markiert, die Farbe auf die vorher gefundene gesetzt,
                        //der Cursor wieder auf seine letzte Position gesetzt und die Schriftfarbe wieder auf Schwarz gesetzt
                        RT.Select(firstQuote, (secondQuote - firstQuote) + 1);
                        RT.SelectionColor = c;
                        RT.Select(curPos, 0);
                        RT.SelectionColor = Color.Black;
                        firstQuote = RT.Text.IndexOf('"', secondQuote + 1);
                    }
                }
            }
            //Steierelement wird wieder aktiviert, einfachste variante um zu vermeiden, dass nach dem Laden der Datei das Steuerelement ganz nach unten gescrollt ist
            richTextBox1.Enabled = true;
        }

        /*
         * Funktion um die aktuelle Reihe auf definierte highlights zu prüfen und text entsprechend zu färben
         */
        private void checkHighlights(RichTextBox RT)
        {
            //Variablen Deklaration und initialisierung
            //aktuelle Cursor Position und Index der aktuellen Reihe festhalten
            int cursorPosition = RT.SelectionStart;
            int lineIndex = RT.GetLineFromCharIndex(cursorPosition);
            if (RT.Text.Length > 0)
            {
                //Auf Highlights soll nur geprüft werden, wenn das Steuerelement auch einen Text beinhaltet
                foreach (ResourceElement r in Highlights)
                {
                    //für jedes Definierte Highlight wird der Text einmal überprüft
                    if (RT.Lines[lineIndex].Contains(" " + r.getText() + " ") | RT.Lines[lineIndex].Contains(r.getText() + " "))
                    {
                        //Färbung wird nur vorgenommen wenn im Text der entsprechende Wert des aktuellen Highlights in der akutellen Reihe vorhanden ist
                        //Counter initialisieren, startPos festlegen um eventuell doppeltes Vorkommen eines Highlights ab zu fangen und auch richtig zu färben
                        int counter = 0;
                        int startPos = lineIndex;
                        counter = startPos;
                        while (counter < startPos + RT.Lines[lineIndex].Length)
                        {
                            //Schleife wird so lange durchlaufen bis counter genau so groß ist wie StartPos
                            //startpos anfangswert des geschten Highlight textes geben
                            startPos = RT.Find(r.getText(), counter, startPos + RT.Lines[lineIndex].Length, RichTextBoxFinds.None);
                            if (startPos != -1)
                            {
                                //Wenn Highlight in der Reihe vorhanden ist wird es ausgewählt, in die richtige Farbe gebracht, die gespeicherte Cursor Position markiert, die
                                //Farbe wieder auf Schwarz geändert und der Counter um die position des aktuellen Highlights erhöht
                                RT.Select(startPos, r.getText().Length);
                                RT.SelectionColor = r.getColor();
                                RT.Select(cursorPosition, 0);
                                RT.SelectionColor = Color.Black;
                                counter = startPos + r.getText().Length;
                            }
                            else
                            {
                                //wenn kein Highlight mehr in der Reihe ist, wird der auf einen Wert gesetzt, der über der aktuellen Länge der Reihe ist um die Schleife zu verlassen
                                counter = startPos + RT.Lines[lineIndex].Length;
                            }
                        }
                    }
                }
            }
            //Funktion zum prüfen der Anführungszeichen wird aufgerufen
            checkQuotes(RT);
        }

        /*
         * Funktion um alle definierten Highlights im aktuellen Text zu suchen. diese Funktion wird nur verwendet, wenn eine Datei eingelesen wurde
         */
        private void checkAllHighlights(RichTextBox RT)
        {
            //deaktivieren des Steuerelements um das evtl auftretende Flackern und anschließende Scrollen zu verhindern
            richTextBox1.Enabled = false;
            int curPos = RT.SelectionStart; // aktuelle Cursor Position wird festgehalten

                foreach (ResourceElement r in Highlights)
                {
                //Schleife wird für jedes definierte Highlight durchlafen.
                    if (RT.Text.Contains(" " + r.getText() + " ") | RT.Text.Contains(r.getText() + " "))
                    {
                        //Färbung wird nur durchgeführt, wenn es auch entsprechende Elemente im text gibt

                        //Counter und Start Position für das suchen des Highlights Deklarieren und initialisieren
                        int counter = 0;
                        int startPos = 0;
                        while (counter < RT.Text.Length)
                        {
                            //schleife wird so lange durchlaufen, bis das ende des Dokuments erreicht ist
                            //Start Position des gesuchten Highlights festhalten
                            startPos = RT.Find(r.getText(), counter, RT.Text.Length, RichTextBoxFinds.None);
                            if (startPos != -1)
                            {
                                //wenn startPos einen gültigen Wert hat, wird das gesuchte Highlight markiert, entsprechend gefärbt, die gespeicherte Cursor Position markiert
                                //und die Farbe auf Schwarz gesetzt
                                RT.Select(startPos, r.getText().Length);
                                RT.SelectionColor = r.getColor();
                                RT.Select(curPos, 0);
                                RT.SelectionColor = Color.Black;
                                counter = startPos + r.getText().Length;
                            }
                            else
                            {
                                //falls keine gültige Start Position vorhanden ist wird der Counter auf die Länge des textes gesetzt
                                counter = RT.Text.Length;
                            }
                        }
                    }
                }
                //funktion zum färben aller Anführungszeichen aufrufen
            checkAllQuotes(RT);
        }

        /*
         * Funktion um die Informationen in der Fußleiste zu aktualisieren
         */
        private void setinfos()
        {
            //Variablen Deklaration und Initialisierung
            int i_RowCount = 0;
            int i_CurrentRow = 0;
            //abrufen und festhalten der gesammt sowie der aktuellenReihenzahl für das aktuelle Dokument
            i_RowCount = richTextBox1.Lines.Length;
            i_CurrentRow = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) + 1;

            //Informationen in die Fußleiste schreiben
            statusStrip1.Items[0].Text = "Zeile " + i_CurrentRow + " von " + i_RowCount;
            statusStrip1.Items[2].Text = "Zeichen " + (richTextBox1.SelectionStart + 1) + " von " + (richTextBox1.Text.Length + 1);
        }

        //Funktion um die aktuelle Textgröße in der Fußzeile zu aktualisieren
        private void setSize()
        {
            statusStrip1.Items[4].Text = "Größe: " + richTextBox1.Font.Size;
        }

        /*
         * Funktion um die vom einstellungen Fenster zurückgegebene Highlight Liste zu speichern
         */
        void SetHighlightList(List<ResourceElement> RL)
        {
            //zuweisung der übergebenen Liste zur Liste, die in dieser Klasse für das Highlighting genutzt wird und anschließender Aufruf der Funktion zum Speichern der Highlights
            //auf Festplatte
            Highlights = RL;
            saveHighlights(RL);
        }

        /*
         * Funkion um die Hihglights auf fesplatte zu speichern, damit sie beim nächsten Programmstart wieder eingelesen werden können
         */
        void saveHighlights(List<ResourceElement> R)
        {
            //Stream zur verarbeitung der Highlights erzeugen, falls die Datei nicht vorhanden ist, wird sie erstellt
            Stream s = File.Open(Application.StartupPath + "\\Highlights.seh", FileMode.OpenOrCreate);
            //Formatter zum serialisieren der Highlight liste erzeugen, daten in den angegebenen Stream serialisieren und Datei schließen
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, R);
            s.Close();
        }

        /*
         * Funktion zum Aufrufen des Highlight konfigurations Festers
         */
        private void textHighlightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Fenster wird erzeugt, rückgabe Funktion sowie die aktuelle Highlight Liste übergeben und das erzeugte Fenster angezeigt
            Highlighting h = new Highlighting(SetHighlightList,Highlights);
            h.Show();
        }

        /*
         * Funktion um zu prüfen, ob die aktuelle Datei schon vorhanden ist oder geändert wurde
         */
        private void CheckFileExistorChanged()
        {
            if (changed)
            {
                //es wird nur etwas geprüft, wenn die Datei beändert wurde
                if(MessageBox.Show("Datei wurde geändert, Speichern?","Datei geändert",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //prüfung ob der User den angezeigten Dialog mit ja geschlossen hat
                    if(s_FileName != "")
                    {
                        //wenn Dateiname einen gültigen Wert besitzt wird die angegebene Datei geöffnet, als Stream verfügbar gemacht, der Inhalt des Steuerelements
                        //hinein geschrieben und wieder geschlossen
                        Stream s = File.Open(s_FileName, FileMode.Open);
                        s.Write(System.Text.Encoding.UTF8.GetBytes(richTextBox1.Text), 0, richTextBox1.Text.Length);
                        s.Close();
                    }
                    else
                    {
                        if(saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            //falls kein gültiger Dateiname vorhanden ist und der User den Speichern Dialog mit ok geschlossen hat, wird eine Datei mit dem
                            //angegebenen Dateinamen als stream geöffnet oder erstellt, der Dateiname in die Global verfügbar Variable gespeichert
                            String FName = saveFileDialog1.FileName;
                            s_FileName = FName;
                            Stream s = File.Open(FName, FileMode.OpenOrCreate);
                            //Daten des Steuerelements werden in den Stream geschrieben und dieser geschlossen.
                            s.Write(System.Text.Encoding.UTF8.GetBytes(richTextBox1.Text), 0, richTextBox1.Text.Length);
                            s.Close();
                        }
                    }
                }
            }
        }
        /*
         * Funktion zum Öffnen einer Datei
         */
        private void öffnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //funktion aufrufen ob die vorhandene datei geändert wurde bzw bereits existiert
            CheckFileExistorChanged();
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //es wird nur eine Datei verarbeitet, wenn der User auch das öffnen bestätigt hat
                
                loading = true;//variable um beim text changed event die Funktion zu unterdrücken
                richTextBox1.Text = "";//Steuerelement leeren
                //Dateinamen aus dem Dialogfenster in variable speichern und Datei as Stream öffnen
                String FName = openFileDialog1.FileName;
                Stream s = File.Open(FName, FileMode.Open);
                //Byte Array mit passender größe erstellen, mit daten aus dem Stream füllen, in lesbaren Text umgewandelt in das Steuerelement schreiben und den Stream Schließen
                byte[] b = new byte[s.Length];
                s.Read(b, 0, b.Length);
                richTextBox1.Text = System.Text.Encoding.UTF8.GetString(b);
                s.Close();
                //Dateinamen in die Globale Variable schreiben, den Fenster Titel ändern, nach Highlights suchen und reset der variablen zum prüfen ob text geändert wurde und
                //das datei geladen wird
                s_FileName = FName;
                changeWindowTitle(s_FileName);
                checkAllHighlights(richTextBox1);
                changed = false;
                loading = false;
            }
        }

        /*
         * Funktion zum erstellen einer neuen Datei
         */
        private void neuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //funktion aufrufen ob die vorhanndene Datei geändert wurde bzw bereits existiert, Steuerelement leeren, Dateinamen leeren, geändert test variable reseten und Fenster
            //Titel anpassen
            CheckFileExistorChanged();
            richTextBox1.Text = "";
            s_FileName = "";
            changed = false;
            changeWindowTitle("Neues Dokument");
        }

        /*
         * Funktion zum speichern der aktuell offenen Datei
         */
        private void speichernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //prüfen ob datei geändert wurde bzw. bereits existiert, geändert test resetten und Fenster Titel anpassen
            CheckFileExistorChanged();
            changed = false;
            changeWindowTitle(s_FileName);
        }

        /*
         * Funktion zum ändern des Fenster Titels mithilfe der übergebenen Variablen
         */
        private void changeWindowTitle(String extention)
        {
            this.Text = "SEdi - " + extention;
        }

        /*
         * Funktion um aktuelle Datei unter bestimmtem Namen zu speichern
         */
        private void speichernUnterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Verarbeitet wird nur, wenn der User das dialogfeld mit ok geschlossen hat
                //Dateinamen aus dem Dialogfenster in Variable speichern, die datei öffnen, bzw falls nicht vorhanden erstellen, ein Byte Array mit passender größe erzeugen 
                //und Inhalt des Steuerelements in das Array schreiben
                String FName = saveFileDialog1.FileName;
                Stream s = File.Open(FName, FileMode.OpenOrCreate);
                byte[] b = new byte[richTextBox1.Text.Length];
                b = System.Text.Encoding.UTF8.GetBytes(richTextBox1.Text);
                //Array in den Stream schreiben, den Stream schließen, Dateinamen global bekannt machen und den Fenster Titel entsprechend ändern
                s.Write(b, 0, b.Length);
                s.Close();
                s_FileName = FName;
                changeWindowTitle(s_FileName);
            }
        }
        /*
         * Funktion zum initialisieren der Daten in der Fußzeile, damit nach Programmstart auch eine korrekte Anzeige vorhanden ist
         */
        private void Form1_Load(object sender, EventArgs e)
        {
            setall();
        }

        //Sammel Funktion um alle Anzeigen auf einmal zu aktualisieren
        private void setall()
        {
            setinfos();
            setSize();
        }

        //Funktion um bei Mausklick die korrekte Position im Text in der Fußzeile an zu zeigen
        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            setall();
        }

        //Funktion um die Tab Teste im Steuerelement zu unterdrücken und anstelle eine Anzahl von Leerzeichen ein zu fügen
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 9)
            {
                //nur etwas verarbeiten, wenn die Tab Taste gedrückt wurde
                //eingabe unterdrücken, dem System erklren das es bearbeitet wurde und selbst an der stelle des Cursor die Leerzeichen einfügen
                e.SuppressKeyPress = true;
                e.Handled = true;
                richTextBox1.SelectedText =  "    ";
            }
            //funktion aufrufen um Fußzeile zu aktualisieren
            setall();
        }

        //Funktion um die Tab Teste im Steuerelement zu unterdrücken, hier wird nur unterdrückt, da das System sowohl beim key down als auch beim Key up prüft, 
        //ob ein Tab eingefügt wurde
        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 9)
            {
                //
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            setall();
        }
        //funktion zum ändern der Text Eigenschaften
        private void textgrößeÄndernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(fontDialog1.ShowDialog() == DialogResult.OK)
            {
                //nur etwas unternehmen, wenn der User den Font Dialog mit ok schließt
                //Schriftgröße, Schriftart, schriftstyle(fett, unterstrichen, usw) aus dem Dialog holen, einen neuen Font erstellen und diesen dem Steuerelement zuweisen
                float size = fontDialog1.Font.Size;
                FontFamily ff = fontDialog1.Font.FontFamily;
                FontStyle fs = fontDialog1.Font.Style;
                Font f = new Font(ff, size,fs);
                richTextBox1.Font = f;
            }
        }
    }
}
