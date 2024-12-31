using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Obs.View
{
    public partial class GirisFrm : Form
    {
        public GirisFrm()
        {
            InitializeComponent();
        }

        private void btnOgrKayit_Click(object sender, EventArgs e)
        {
            // Yeni formu göster
            OgrKayitFrm ogrKayitFrm = new OgrKayitFrm();
            ogrKayitFrm.Show();

            // Mevcut formu gizle
            this.Hide();
        }

        private void btnDersKayit_Click(object sender, EventArgs e)
        {
            DersKayitFrm dersKayitFrm = new DersKayitFrm();
            dersKayitFrm.Show();

            // Mevcut formu gizle
            this.Hide();
        }

        private void btnSinifKayit_Click(object sender, EventArgs e)
        {
            SinifKayitFrm sinifKayitFrm = new SinifKayitFrm();
            sinifKayitFrm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OgrenciDersFrm ogrDers = new OgrenciDersFrm();
            ogrDers.Show();
            this.Hide();
        }
    }
}
