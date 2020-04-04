using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EatingMonster
{
    public partial class LoseForm : Form
    {
        public LoseForm()
        {
            InitializeComponent();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            LoseForm.ActiveForm.Hide();
            FrmMain frm = new FrmMain();
            frm.ShowDialog();
            frm.Dispose();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
