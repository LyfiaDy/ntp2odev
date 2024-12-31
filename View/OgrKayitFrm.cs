using Microsoft.EntityFrameworkCore;
using Obs.Data;
using Obs.Helper;
using Obs.Model;
using Obs.View;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Obs
{
    public partial class OgrKayitFrm : Form
    {
        private Ogrenci ogrenci = new Ogrenci(); // Ogrenci nesnesi
        private OBSDBContext context = new OBSDBContext(); // Veritabaný context

        public OgrKayitFrm()
        {
            InitializeComponent();
            SiniflariYukle(); // Sýnýf bilgilerini ComboBox'a yükle
        }

        // Sýnýf bilgilerini ComboBox'a yükleme iþlemi
        private void SiniflariYukle()
        {
            var siniflar = context.Siniflar.ToList();
            cmbSinif.DataSource = siniflar;
            cmbSinif.DisplayMember = "SinifAd"; // Görüntülenecek alan
            cmbSinif.ValueMember = "SinifId"; // Seçildiðinde alýnacak ID
        }

        // Kaydetme iþlemi
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!FormHelper.AlanlarDoluMu(txtOgrAd.Text, txtOgrSoyad.Text, txtOgrNumara.Text)) return;

            string ogrNumara = txtOgrNumara.Text;

            // Ayný numaraya sahip öðrenci var mý kontrolü
            var mevcutOgrenci = context.Students.FirstOrDefault(o => o.Numara == ogrNumara);
            if (mevcutOgrenci != null)
            {
                MessageBox.Show($"'{ogrNumara}' numaralý öðrenci zaten mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Ýþlemi iptal et
            }

            // Yeni öðrenci oluþturuluyor
            var yeniOgrenci = new Ogrenci
            {
                Ad = txtOgrAd.Text,
                Soyad = txtOgrSoyad.Text,
                Numara = ogrNumara,
                SinifId = (int)cmbSinif.SelectedValue // Seçilen sýnýfýn ID'sini alýyoruz
            };

            context.Students.Add(yeniOgrenci);

            // Seçilen sýnýfýn AktifKontenjanýný artýr
            var secilenSinif = context.Siniflar.FirstOrDefault(s => s.SinifId == yeniOgrenci.SinifId);
            if (secilenSinif != null)
            {
                secilenSinif.AktifKontenjan += 1; // Kontenjaný artýr
            }

            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("Öðrenci baþarýyla kaydedildi.");
            }
            else
            {
                MessageBox.Show("Kayýt iþlemi baþarýsýz oldu.");
            }

            // Alanlarý temizle
            txtOgrAd.Clear();
            txtOgrSoyad.Clear();
            txtOgrNumara.Clear();
            cmbSinif.SelectedIndex = -1;
        }

        // Öðrenci kaydý bulma iþlemi
        private void btnKayitBul_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOgrNumara.Text))
            {
                MessageBox.Show("Lütfen öðrenci numarasý girin.");
                return;
            }

            string ogrNumara = txtOgrNumara.Text;
            ogrenci = context.Students.FirstOrDefault(o => o.Numara == ogrNumara);

            if (ogrenci != null)
            {
                txtOgrAd.Text = ogrenci.Ad;
                txtOgrSoyad.Text = ogrenci.Soyad;
                txtOgrNumara.Text = ogrenci.Numara;
                cmbSinif.SelectedValue = ogrenci.SinifId; // Öðrencinin sýnýfýný seç
                MessageBox.Show("Öðrenci bulundu!");
            }
            else
            {
                MessageBox.Show("Öðrenci bulunamadý.");
            }
        }

        // Güncelleme iþlemi
        private void btnGüncelle_Click(object sender, EventArgs e)
        {
            if (ogrenci == null)
            {
                MessageBox.Show("Lütfen önce bir öðrenci arayýn.");
                return;
            }

            if (!FormHelper.AlanlarDoluMu(txtOgrAd.Text, txtOgrSoyad.Text, txtOgrNumara.Text)) return;

            ogrenci.Ad = txtOgrAd.Text;
            ogrenci.Soyad = txtOgrSoyad.Text;
            ogrenci.Numara = txtOgrNumara.Text;
            ogrenci.SinifId = (int)cmbSinif.SelectedValue; // Seçilen sýnýfýn ID'siyle güncelle

            context.Students.Update(ogrenci);
            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("Öðrenci baþarýyla güncellendi.");
            }
            else
            {
                MessageBox.Show("Güncelleme iþlemi baþarýsýz oldu.");
            }

            // Alanlarý temizle
            txtOgrAd.Clear();
            txtOgrSoyad.Clear();
            txtOgrNumara.Clear();
            cmbSinif.SelectedIndex = -1;
        }

        // Silme iþlemi
        private void btnSil_Click(object sender, EventArgs e)
        {
            // Eðer seçili öðrenci yoksa
            if (ogrenci != null)
            {
                // Öðrencinin kayýtlý derslerini çekiyoruz
                var ogrenciDersler = context.OgrenciDersler
                    .Include(od => od.Ders) // Ders bilgisini de yüklüyoruz
                    .Where(od => od.OgrenciId == ogrenci.OgrenciId)
                    .ToList();

                if (ogrenciDersler.Any())
                {
                    // Ders bilgisi varsa, ders adlarýný string listesine ekle
                    string dersListesi = string.Join(", ", ogrenciDersler.Select(od => od.Ders?.DersAd ?? "Bilinmeyen Ders"));

                    // Kullanýcýya derslerin silineceðini söyleyen bir mesaj
                    var sonuc = MessageBox.Show($"{ogrenci.Ad} {ogrenci.Soyad} adlý öðrencinin kayýtlý olduðu dersler: {dersListesi}. Bu dersler de silinecek. Devam etmek istiyor musunuz?",
                                                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (sonuc == DialogResult.Yes)
                    {
                        // Ýlk önce öðrencinin ders kayýtlarýný siliyoruz
                        context.OgrenciDersler.RemoveRange(ogrenciDersler);

                        // Sonra öðrenciyi siliyoruz
                        context.Students.Remove(ogrenci);

                        // Öðrenci silindikten sonra aktif kontenjaný azaltýyoruz
                        var sinif = context.Siniflar.FirstOrDefault(s => s.SinifId == ogrenci.SinifId);
                        if (sinif != null)
                        {
                            sinif.AktifKontenjan--;
                        }

                        // Deðiþiklikleri veritabanýna kaydet
                        context.SaveChanges();
                        MessageBox.Show("Öðrenci ve ders kayýtlarý baþarýyla silindi.");
                    }
                }
                else
                {
                    // Eðer öðrenciye ait ders kaydý yoksa direkt öðrenciyi silebiliriz
                    var sonuc = MessageBox.Show($"{ogrenci.Ad} {ogrenci.Soyad} adlý öðrenciyi silmek istediðinizden emin misiniz?",
                                                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (sonuc == DialogResult.Yes)
                    {
                        context.Students.Remove(ogrenci);

                        // Öðrenci silindikten sonra aktif kontenjaný azaltýyoruz
                        var sinif = context.Siniflar.FirstOrDefault(s => s.SinifId == ogrenci.SinifId);
                        if (sinif != null)
                        {
                            sinif.AktifKontenjan--;
                        }

                        context.SaveChanges();
                        MessageBox.Show("Öðrenci baþarýyla silindi.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen önce silinecek bir öðrenci seçin.", "Öðrenci Seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Ýþlemi durdur
            }

            
        }



        private void btnGeri_Click(object sender, EventArgs e)
        {
            GirisFrm giris = new GirisFrm();
            giris.Show();
            this.Hide();
        }
    }
}
