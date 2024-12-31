using Obs.Data;
using Obs.Helper;
using Obs.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Obs.View
{
    public partial class DersKayitFrm : Form
    {
        private Ders ders = new Ders(); // Ders nesnesi

        public DersKayitFrm()
        {
            InitializeComponent();
        }

        // Ders kaydını ders koduna göre arama işlemi
        private void btnKayitBul_Click(object sender, EventArgs e)
        {
            using (var context = new OBSDBContext())
            {
                string dersKod = txtDersKod.Text; // Kullanıcıdan Ders Kodunu alıyoruz

                if (string.IsNullOrEmpty(dersKod))
                {
                    MessageBox.Show("Lütfen ders kodunu girin.");
                    return;
                }

                // Veritabanında ders kodu ile arama yapıyoruz
                var bulunanDers = context.Dersler.FirstOrDefault(d => d.DersKod == dersKod);

                if (bulunanDers != null)
                {
                    ders = bulunanDers;  // Ders nesnesine veriyi atıyoruz
                    txtDersKod.Text = ders.DersKod;
                    txtDersAd.Text = ders.DersAd;
                }
                else
                {
                    MessageBox.Show($"'{dersKod}' ders koduna sahip ders bulunamadı.");
                }
            }
        }

        // Yeni ders kaydetme işlemi
        private void btnKayit_Click(object sender, EventArgs e)
        {
            // Boş alan kontrolü
            if (!FormHelper.AlanlarDoluMu(txtDersAd.Text, txtDersKod.Text)) return;

            using (var context = new OBSDBContext())
            {
                string dersKod = txtDersKod.Text;

                // Aynı ders koduyla bir kayıt var mı kontrolü
                var mevcutDers = context.Dersler.FirstOrDefault(d => d.DersKod == dersKod);
                if (mevcutDers != null)
                {
                    MessageBox.Show($"'{dersKod}' koduna sahip ders zaten mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // İşlem iptal edilir
                }

                // Yeni ders nesnesi oluşturuluyor
                var yeniDers = new Ders
                {
                    DersKod = dersKod,
                    DersAd = txtDersAd.Text
                };

                context.Dersler.Add(yeniDers);
                int etkilenenSatir = context.SaveChanges();

                if (etkilenenSatir > 0)
                {
                    MessageBox.Show("Ders başarıyla kaydedildi.");
                }
                else
                {
                    MessageBox.Show("Kayıt işlemi başarısız oldu.");
                }
            }

            // Alanları temizle
            txtDersKod.Clear();
            txtDersAd.Clear();
        }

        // Ders güncelleme işlemi
        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            // Ders bulunmalı ve boş alan kontrolü yapılmalı
            if (ders == null)
            {
                MessageBox.Show("Lütfen önce bir ders arayın.");
                return;
            }

            if (!FormHelper.AlanlarDoluMu(txtDersAd.Text, txtDersKod.Text)) return;

            using (var context = new OBSDBContext())
            {
                var mevcutDers = context.Dersler.FirstOrDefault(d => d.DersKod == ders.DersKod);
                if (mevcutDers != null)
                {
                    // Aynı ders koduna sahip başka bir ders var mı kontrolü (ID haricinde)
                    var ayniKoddaDers = context.Dersler.FirstOrDefault(d => d.DersKod == txtDersKod.Text && d.DersId != ders.DersId);
                    if (ayniKoddaDers != null)
                    {
                        MessageBox.Show($"'{txtDersKod.Text}' koduna sahip ders zaten mevcut. Lütfen farklı bir ders kodu girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Ders bilgilerini güncelle
                    mevcutDers.DersKod = txtDersKod.Text;
                    mevcutDers.DersAd = txtDersAd.Text;

                    context.Dersler.Update(mevcutDers);
                    int etkilenenSatir = context.SaveChanges();

                    if (etkilenenSatir > 0)
                    {
                        MessageBox.Show("Ders başarıyla güncellendi.");
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme işlemi başarısız oldu.");
                    }
                }
                else
                {
                    MessageBox.Show("Ders bulunamadı.");
                }
            }

            // Alanları temizle
            txtDersKod.Clear();
            txtDersAd.Clear();
        }

        // Ders silme işlemi
        private void btnSil_Click(object sender, EventArgs e)
        {
            // Ders bulunmalı
            if (ders == null)
            {
                MessageBox.Show("Lütfen önce bir ders arayın.");
                return;
            }

            // Silme onayı al (DersAd'ına göre)
            if (!FormHelper.SilmeOnayi(ders)) return;

            using (var context = new OBSDBContext())
            {
                var silinecekDers = context.Dersler.FirstOrDefault(d => d.DersKod == ders.DersKod);
                if (silinecekDers != null)
                {
                    context.Dersler.Remove(silinecekDers);
                    int etkilenenSatir = context.SaveChanges();

                    if (etkilenenSatir > 0)
                    {
                        MessageBox.Show("Ders başarıyla silindi.");
                        ders = null; // Silinen nesneyi sıfırla
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.");
                    }
                }
                else
                {
                    MessageBox.Show("Ders bulunamadı.");
                }
            }

            // Alanları temizle
            txtDersKod.Clear();
            txtDersAd.Clear();
        }

        private void anaSayfa_Click(object sender, EventArgs e)
        {
            GirisFrm giris = new GirisFrm();
            giris.ShowDialog();
            this.Hide();
        }

        private void anaSayfa_Click_1(object sender, EventArgs e)
        {
            GirisFrm frm = new GirisFrm();
            frm.Show();
            this.Close();
        }

        // Diğer butonlar için gerekli işlemler...
    }
}
