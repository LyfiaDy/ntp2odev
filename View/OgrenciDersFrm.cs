using Obs.Data;
using Obs.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Obs.View
{
    public partial class OgrenciDersFrm : Form
    {
        private Ogrenci ogrenci; // Seçilen öğrenci
        private OBSDBContext context = new OBSDBContext(); // Veritabanı context

        public OgrenciDersFrm()
        {
            InitializeComponent();
        }

        // Geri butonu (önceki sayfaya dönmek için)
        private void btnGeri_Click(object sender, EventArgs e)
        {
            GirisFrm frm = new GirisFrm();
            frm.Show();
            this.Close(); // Mevcut formu kapat
        }

        // Öğrenci arama işlemi
        private void btnOgrAra_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumara.Text))
            {
                MessageBox.Show("Lütfen öğrenci numarasını girin.");
                return;
            }

            string ogrNumara = txtNumara.Text;
            ogrenci = context.Students.FirstOrDefault(o => o.Numara == ogrNumara);

            if (ogrenci != null)
            {
                lblAd.Text = ogrenci.Ad;
                lblSoyad.Text = ogrenci.Soyad;
                MessageBox.Show("Öğrenci bulundu!");

                // Tüm dersleri listele
                DersleriYukle();
            }
            else
            {
                MessageBox.Show("Öğrenci bulunamadı.");
            }
        }

        // Ders arama işlemi
        private void btnDersAra_Click(object sender, EventArgs e)
        {
            DersleriYukle();
        }

        // Tüm dersleri veya arama kriterine göre dersleri listeleme işlemi
        private void DersleriYukle()
        {
            string dersAdi = txtDersAd.Text.Trim();
            string dersKodu = txtDersKodu.Text.Trim();

            // Ders adında veya ders kodunda arama yap
            var dersler = context.Dersler.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dersAdi))
            {
                dersler = dersler.Where(d => d.DersAd.Contains(dersAdi)); // % işareti etkisiyle arama
            }

            if (!string.IsNullOrWhiteSpace(dersKodu))
            {
                dersler = dersler.Where(d => d.DersKod.Contains(dersKodu)); // % işareti etkisiyle arama
            }

            // Sonuçları data gride yükle
            dgDersListesi.DataSource = dersler.ToList();
        }

        // Ders kaydetme işlemi
        private void btnDersKaydet_Click(object sender, EventArgs e)
        {
            if (ogrenci == null)
            {
                MessageBox.Show("Lütfen önce bir öğrenci seçin.");
                return;
            }

            // DataGridView'deki seçili dersleri al
            List<int> secilenDersler = new List<int>();

            foreach (DataGridViewRow row in dgDersListesi.SelectedRows)
            {
                int dersId = (int)row.Cells["DersId"].Value;
                secilenDersler.Add(dersId);
            }

            if (secilenDersler.Count == 0)
            {
                MessageBox.Show("Lütfen kaydetmek istediğiniz dersleri seçin.");
                return;
            }

            // Seçilen dersleri OgrenciDers modeline ekle
            foreach (var dersId in secilenDersler)
            {
                // Öğrencinin bu dersi daha önce alıp almadığını kontrol et
                var mevcutKayit = context.OgrenciDersler.FirstOrDefault(od => od.OgrenciId == ogrenci.OgrenciId && od.DersId == dersId);
                if (mevcutKayit != null) continue; // Eğer kayıt varsa atla

                OgrenciDers yeniKayit = new OgrenciDers
                {
                    OgrenciId = ogrenci.OgrenciId,
                    DersId = dersId
                };

                context.OgrenciDersler.Add(yeniKayit);
            }

            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("Dersler başarıyla kaydedildi.");
            }
            else
            {
                MessageBox.Show("Kaydetme işlemi başarısız oldu.");
            }
        }

        // Ders silme işlemi
        private void btnDersSil_Click(object sender, EventArgs e)
        {
            if (ogrenci == null)
            {
                MessageBox.Show("Lütfen önce bir öğrenci seçin.");
                return;
            }

            // DataGridView'deki seçili dersleri al
            List<int> secilenDersler = new List<int>();

            foreach (DataGridViewRow row in dgDersListesi.SelectedRows)
            {
                int dersId = (int)row.Cells["DersId"].Value;
                secilenDersler.Add(dersId);
            }

            if (secilenDersler.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz dersleri seçin.");
                return;
            }

            // Seçilen dersleri OgrenciDers modelinden sil
            foreach (var dersId in secilenDersler)
            {
                var mevcutKayit = context.OgrenciDersler.FirstOrDefault(od => od.OgrenciId == ogrenci.OgrenciId && od.DersId == dersId);
                if (mevcutKayit != null)
                {
                    context.OgrenciDersler.Remove(mevcutKayit);
                }
            }

            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("Dersler başarıyla silindi.");
            }
            else
            {
                MessageBox.Show("Silme işlemi başarısız oldu.");
            }
        }
    }
}
