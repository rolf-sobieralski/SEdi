using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace SEdi
{
    /*
     * Klasse für die Erstellung speicherung und aktualisierung der Highlights
     */
    public partial class Highlighting : Form
    {
        Action<List<ResourceElement>> ReturnFunc;
        List<ResourceElement> list;

        /*
         * Funktion zum Initialisieren einer neuen Highlighting Instanz mit übergabe einer aktuellen Highlight liste und der Passenden Rückgabe Funktion aus der Aufrufenden klasse
         */
        public Highlighting(Action<List<ResourceElement>> ReturnList, List<ResourceElement> RL)
        {
            InitializeComponent();
            //Speichern der Übergebenen Funktion zum zurückgeben einer ggf. aktualisierten Highlight Liste an die Aufrufende KLasse
            ReturnFunc = ReturnList;
            if (RL != null)
            {
                //wenn eine Highlight Liste übergeben wurde, wird diese der klasse bekannt gemacht und die Funktion zum befüllen des Fensters aufgerufen
                list = RL;
                InsertHighlights();
            }
            else
            {
                //Falls keine Liste übergeben wurde, wird eine neue Liste erzeugt
                list = new List<ResourceElement>();
            }
        }

        /*
         * Funktion um das Fenster mit den Werten aus der übergebenen Highlight Liste zu befüllen
         */
        void InsertHighlights()
        {
            if(list.Count > 0)
            {
                //verarbeitung erfolgt nur, wenn die Liste auch Einträge hat
                dataGridView1.Rows.Clear();//evtl. vorhandene Einträge aus der Liste entfernen
                for(int i = 0; i < list.Count; i++)
                {
                    // Zählschleife um jeden Eintrag aus der Liste in das Steuerelement ein zu fügen

                    //neue Reihe und neue Zellen für die Reihe erzeugen
                    DataGridViewRow r = new DataGridViewRow();
                    DataGridViewTextBoxCell ct = new DataGridViewTextBoxCell();
                    DataGridViewTextBoxCell cc = new DataGridViewTextBoxCell();
                    //Inhalt der Zellen mit den Werten aus dem aktuellen Listen Element befüllen und die Hintergrundfarbe der Farb Zelle auf den entsprechenden Wert setzen
                    ct.Value = list[i].getText();
                    cc.Value = list[i].getColor().ToArgb().ToString();
                    cc.Style.BackColor = list[i].getColor();
                    //Zellen zur Reihe hinzufügen und die Reihe an das Steuerelement anhängen
                    r.Cells.Add(ct);
                    r.Cells.Add(cc);
                    dataGridView1.Rows.Add(r);
                }
            }
        }

        /*
         * Funktion zum erstellen der eigendlichen Highlight Liste
         */
        void CreateList()
        {
            if(dataGridView1.Rows.Count > 1)
            {
                //Verarbeitung erfolgt nur, wenn das Steuerelement auch Einträge hat

                //Liste vorsichtshalber löschen
                list.Clear();
                foreach(DataGridViewRow r in dataGridView1.Rows)
                {
                    //Schleife die jede Reihe aus dem Steuerelement durchläuft
                    if (r.Cells[0].FormattedValue.ToString() != "")
                    {
                        //prüfung ob die aktuelle Reihe noch Gültige Einträge hat

                        //neues ResourceElement für die Highlight Liste erzeugen, die Werte aus dem Steuerelement dort eintragen und das Element der Liste hinzufügen
                        ResourceElement re = new ResourceElement(r.Cells[0].FormattedValue.ToString(), r.Cells[1].Style.BackColor);
                        list.Add(re);
                    }
                }
            }
        }

        /*
         * Funktion zum Behandeln des Speicherknopf Klicks
         */
        private void but_save_Click(object sender, EventArgs e)
        {
            //funktion zum erstellen der Highlight liste aufrufen, Liste an die Rückgabefunktion übergeben und aktuelles Fenster schließen
            CreateList();
            ReturnFunc(list);
            this.Close();
        }

        /*
         * Funktion um die Farbe im Steuerelement Feld ein zu tragen
         */
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                //verarbeitung erfolgt nur, wenn eine Zelle in der zweiten spalte geklickt wurde
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    //falls der User das Fenster mit ok schließt, wird die Farbe aus dem Dialog abgerufen, der Wert der Gewählten Zell auf den Farbwert gesetzt und die Hintergrund
                    //Farbe entsprechend angepasst
                    Color c = colorDialog1.Color;
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = c.ToArgb().ToString();
                    dataGridView1.Rows[e.RowIndex].Cells[1].Style.BackColor = c;
                }
            }
        }

        /*
         * Funktion um externe Formatierungs Dateien zu laden
         */
        private void but_load_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "*.seh";//Filter setzen um nur Highlight Dateien an zu zeigen
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Verarbeitung erfolgt nur, wenn der User den Dialog mit ok geschlossen hat

                //Datei als Stream bereitstellen, einen neuen Formatter erzeugen, den übergebenen Stream de serialisieren, in der Highlight Liste speichern, den Stream schließen
                //und die funktion zum darstellen der Highlights aufrufen
                Stream s = File.Open(openFileDialog1.FileName, FileMode.Open);
                BinaryFormatter b = new BinaryFormatter();
                list = (List<ResourceElement>)b.Deserialize(s);
                s.Close();
                InsertHighlights();
            }
        }
        /*
         * Funktion um die aktuellen Highlights zu exportieren
         */
        private void but_export_Click(object sender, EventArgs e)
        {
            //Dateinamen vorgeben und Funktion zum erzeugen der Highlight liste aus den Eitnrägen im Steuerelement aufrufen
            saveFileDialog1.FileName = "Highlights.seh";
            CreateList();
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Verarbeitung erfolgt nur, wenn der User den Dialog mit ok geschlossen hat

                //neuen Stream erzeugen
                Stream FileStream;
                if((FileStream = saveFileDialog1.OpenFile()) != null)
                {
                    //prüfung ob die im Dialog angegebene Datei erstellt und geöffnet werden konnte

                    //neuen Formater erstellen, die aktuelle Highlight Liste serialisiert in den Stream schreiben und den Stream schließen
                    BinaryFormatter b = new BinaryFormatter();
                    b.Serialize(FileStream, list);
                    FileStream.Close();
                }
            }
        }
        /*
         * Funktion zum verarbeiten des Klick auf abbrechen
         */
        private void but_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
