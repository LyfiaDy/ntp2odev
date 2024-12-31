using Obs.Data;
using Obs.Model;
using Obs.Helper;
using Obs.Properties;
using System;
using System.Windows.Forms;

namespace Obs.View
{
    public partial class SinifKayitFrm : Form
    {
        Sinif sinif = new Sinif();

        public SinifKayitFrm()
        {
            InitializeComponent();
        }

        // Kaydetme işlemi
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            // Boş alan kontrolü
            if (!FormHelper.AlanlarDoluMu(txtSinifAd.Text, txtKontenjan.Text)) return;

            using (var context = new OBSDBContext())
            {
                string sinifAd = txtSinifAd.Text;

                // Aynı isimde bir sınıfın olup olmadığını kontrol et
                var mevcutSinif = context.Siniflar.FirstOrDefault(s => s.SinifAd == sinifAd);
                if (mevcutSinif != null)
                {
                    MessageBox.Show($"'{sinifAd}' adlı sınıf zaten mevcut. Lütfen farklı bir sınıf adı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // İşlemi iptal et
                }

                // Yeni sınıf oluşturuluyor
                var yeniSinif = new Sinif
                {
                    SinifAd = txtSinifAd.Text,
                    Kontenjan = int.Parse(txtKontenjan.Text)
                    // SinifId atamıyoruz, çünkü bu otomatik artan bir alan
                };

                context.Siniflar.Add(yeniSinif);
                int etkilenenSatir = context.SaveChanges();

                if (etkilenenSatir > 0)
                {
                    MessageBox.Show("Sınıf başarıyla kaydedildi.");
                }
                else
                {
                    MessageBox.Show("Kayıt işlemi başarısız oldu.");
                }
            }

            // Giriş kutularını temizle
            txtSinifAd.Clear();
            txtKontenjan.Clear();
        }

        // Kayıt bulma işlemi
        private void btnKayit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSinifAd.Text))
            {
                MessageBox.Show("Lütfen aramak için bir sınıf adı girin.");
                return;
            }

            using (var context = new OBSDBContext())
            {
                string sinifAd = txtSinifAd.Text;
                sinif = context.Siniflar.FirstOrDefault(s => s.SinifAd == sinifAd);

                if (sinif != null)
                {
                    txtSinifAd.Text = sinif.SinifAd;
                    txtKontenjan.Text = sinif.Kontenjan.ToString();
                    MessageBox.Show("Sınıf bulundu!");
                }
                else
                {
                    MessageBox.Show("Sınıf bulunamadı.");
                }
            }
        }

        // Güncelleme işlemi
        private void btnGüncelle_Click(object sender, EventArgs e)
        {
            if (sinif == null)
            {
                MessageBox.Show("Lütfen önce bir sınıf arayın.");
                return;
            }

            // Boş alan kontrolü için yardımcı fonksiyonu çağırıyoruz
            if (!FormHelper.AlanlarDoluMu(txtSinifAd.Text, txtKontenjan.Text)) return;

            using (var context = new OBSDBContext())
            {
                sinif.SinifAd = txtSinifAd.Text;
                sinif.Kontenjan = int.Parse(txtKontenjan.Text);

                context.Siniflar.Update(sinif);
                int etkilenenSatir = context.SaveChanges();

                if (etkilenenSatir > 0)
                {
                    MessageBox.Show("Sınıf başarıyla güncellendi.");
                }
                else
                {
                    MessageBox.Show("Güncelleme işlemi başarısız oldu.");
                }
            }

            txtSinifAd.Clear();
            txtKontenjan.Clear();
        }

        // Silme işlemi
        private void btnSil_Click(object sender, EventArgs e)
        {
            if (sinif == null)
            {
                MessageBox.Show("Lütfen önce bir sınıf arayın.");
                return;
            }

            // Silme işlemi için onay alınır
            if (!FormHelper.SilmeOnayi(sinif.SinifAd)) return;

            using (var context = new OBSDBContext())
            {
                context.Siniflar.Remove(sinif);
                int etkilenenSatir = context.SaveChanges();

                if (etkilenenSatir > 0)
                {
                    MessageBox.Show("Sınıf başarıyla silindi.");
                    sinif = null; // Silinen nesneyi sıfırla
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız oldu.");
                }
            }

            txtSinifAd.Clear();
            txtKontenjan.Clear();
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            GirisFrm giris = new GirisFrm();
            giris.Show();

            // Mevcut formu gizle
            this.Hide();
        }
    }
}
