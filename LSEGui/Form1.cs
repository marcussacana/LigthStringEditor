using LigthStringEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSEGui {
    public partial class LSEGUI : Form {
        public LSEGUI() {
            InitializeComponent();
            MessageBox.Show("THIS IS A FUCKING DEMOSTRATION PROJECT, WRITE YOUR GUI.");
        }

        DatTL Editor;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog filed = new OpenFileDialog();
            filed.Filter = "All Dat Files|*.dat";

            if (filed.ShowDialog() == DialogResult.OK) {
                byte[] File = System.IO.File.ReadAllBytes(filed.FileName);
                Editor = new DatTL(File);

                listBox1.Items.Clear();
                foreach (string str in Editor.Import())
                    listBox1.Items.Add(str);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                Text = string.Format("ID: {0}/{1}",i, listBox1.Items.Count);
                textBox1.Text = listBox1.Items[i].ToString();
            }
            catch { }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
                if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                    try {
                        listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                    }
                    catch {

                    }
                }
            }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog filed = new SaveFileDialog();
            filed.Filter = "All Dat Files|*.dat";

            if (filed.ShowDialog() == DialogResult.OK) {
                string[] Strs = new string[listBox1.Items.Count];
                for (int i = 0; i < Strs.Length; i++) {
                    Strs[i] = listBox1.Items[i].ToString();
                }

                byte[] Script = Editor.Export(Strs);
                System.IO.File.WriteAllBytes(filed.FileName, Script);

                MessageBox.Show("File Saved.");
            }
        }
    }
}
