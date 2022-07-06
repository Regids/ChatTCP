using ClienteTCP;
using ServerTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiMenu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnServidor_Click(object sender, EventArgs e)
        {
            frmServidor server = new frmServidor();
            server.Visible = true;
        }

        private void btnCliente_Click(object sender, EventArgs e)
        {
            frmCliente cliente =  new frmCliente();
            cliente.Visible = true;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
