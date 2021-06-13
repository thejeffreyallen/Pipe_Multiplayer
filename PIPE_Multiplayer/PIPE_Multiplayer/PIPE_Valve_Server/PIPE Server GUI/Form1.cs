using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PIPE_Valve_Online_Server;


namespace PIPE_Server_GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

       public void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Hello");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

      
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
