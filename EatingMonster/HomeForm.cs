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
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();

            this.Height = 32 * 22;
            this.Width = 32 * 21;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            HomeForm.ActiveForm.Hide();

            FrmMain frm = new FrmMain();
            frm.ShowDialog();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HomeForm.ActiveForm.Hide();

            infoFrm frm = new infoFrm();
            frm.ShowDialog();
        }
    }
}
