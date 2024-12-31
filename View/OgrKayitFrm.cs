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
        private OBSDBContext context = new OBSDBContext(); // Veritaban� context

        public OgrKayitFrm()
        {
            InitializeComponent();
            SiniflariYukle(); // S�n�f bilgilerini ComboBox'a y�kle
        }

        // S�n�f bilgilerini ComboBox'a y�kleme i�lemi
        private void SiniflariYukle()
        {
            var siniflar = context.Siniflar.ToList();
            cmbSinif.DataSource = siniflar;
            cmbSinif.DisplayMember = "SinifAd"; // G�r�nt�lenecek alan
            cmbSinif.ValueMember = "SinifId"; // Se�ildi�inde al�nacak ID
        }

        // Kaydetme i�lemi
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!FormHelper.AlanlarDoluMu(txtOgrAd.Text, txtOgrSoyad.Text, txtOgrNumara.Text)) return;

            string ogrNumara = txtOgrNumara.Text;

            // Ayn� numaraya sahip ��renci var m� kontrol�
            var mevcutOgrenci = context.Students.FirstOrDefault(o => o.Numara == ogrNumara);
            if (mevcutOgrenci != null)
            {
                MessageBox.Show($"'{ogrNumara}' numaral� ��renci zaten mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // ��lemi iptal et
            }

            // Yeni ��renci olu�turuluyor
            var yeniOgrenci = new Ogrenci
            {
                Ad = txtOgrAd.Text,
                Soyad = txtOgrSoyad.Text,
                Numara = ogrNumara,
                SinifId = (int)cmbSinif.SelectedValue // Se�ilen s�n�f�n ID'sini al�yoruz
            };

            context.Students.Add(yeniOgrenci);

            // Se�ilen s�n�f�n AktifKontenjan�n� art�r
            var secilenSinif = context.Siniflar.FirstOrDefault(s => s.SinifId == yeniOgrenci.SinifId);
            if (secilenSinif != null)
            {
                secilenSinif.AktifKontenjan += 1; // Kontenjan� art�r
            }

            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("��renci ba�ar�yla kaydedildi.");
            }
            else
            {
                MessageBox.Show("Kay�t i�lemi ba�ar�s�z oldu.");
            }

            // Alanlar� temizle
            txtOgrAd.Clear();
            txtOgrSoyad.Clear();
            txtOgrNumara.Clear();
            cmbSinif.SelectedIndex = -1;
        }

        // ��renci kayd� bulma i�lemi
        private void btnKayitBul_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOgrNumara.Text))
            {
                MessageBox.Show("L�tfen ��renci numaras� girin.");
                return;
            }

            string ogrNumara = txtOgrNumara.Text;
            ogrenci = context.Students.FirstOrDefault(o => o.Numara == ogrNumara);

            if (ogrenci != null)
            {
                txtOgrAd.Text = ogrenci.Ad;
                txtOgrSoyad.Text = ogrenci.Soyad;
                txtOgrNumara.Text = ogrenci.Numara;
                cmbSinif.SelectedValue = ogrenci.SinifId; // ��rencinin s�n�f�n� se�
                MessageBox.Show("��renci bulundu!");
            }
            else
            {
                MessageBox.Show("��renci bulunamad�.");
            }
        }

        // G�ncelleme i�lemi
        private void btnG�ncelle_Click(object sender, EventArgs e)
        {
            if (ogrenci == null)
            {
                MessageBox.Show("L�tfen �nce bir ��renci aray�n.");
                return;
            }

            if (!FormHelper.AlanlarDoluMu(txtOgrAd.Text, txtOgrSoyad.Text, txtOgrNumara.Text)) return;

            ogrenci.Ad = txtOgrAd.Text;
            ogrenci.Soyad = txtOgrSoyad.Text;
            ogrenci.Numara = txtOgrNumara.Text;
            ogrenci.SinifId = (int)cmbSinif.SelectedValue; // Se�ilen s�n�f�n ID'siyle g�ncelle

            context.Students.Update(ogrenci);
            int etkilenenSatir = context.SaveChanges();

            if (etkilenenSatir > 0)
            {
                MessageBox.Show("��renci ba�ar�yla g�ncellendi.");
            }
            else
            {
                MessageBox.Show("G�ncelleme i�lemi ba�ar�s�z oldu.");
            }

            // Alanlar� temizle
            txtOgrAd.Clear();
            txtOgrSoyad.Clear();
            txtOgrNumara.Clear();
            cmbSinif.SelectedIndex = -1;
        }

        // Silme i�lemi
        private void btnSil_Click(object sender, EventArgs e)
        {
            // E�er se�ili ��renci yoksa
            if (ogrenci != null)
            {
                // ��rencinin kay�tl� derslerini �ekiyoruz
                var ogrenciDersler = context.OgrenciDersler
                    .Include(od => od.Ders) // Ders bilgisini de y�kl�yoruz
                    .Where(od => od.OgrenciId == ogrenci.OgrenciId)
                    .ToList();

                if (ogrenciDersler.Any())
                {
                    // Ders bilgisi varsa, ders adlar�n� string listesine ekle
                    string dersListesi = string.Join(", ", ogrenciDersler.Select(od => od.Ders?.DersAd ?? "Bilinmeyen Ders"));

                    // Kullan�c�ya derslerin silinece�ini s�yleyen bir mesaj
                    var sonuc = MessageBox.Show($"{ogrenci.Ad} {ogrenci.Soyad} adl� ��rencinin kay�tl� oldu�u dersler: {dersListesi}. Bu dersler de silinecek. Devam etmek istiyor musunuz?",
                                                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (sonuc == DialogResult.Yes)
                    {
                        // �lk �nce ��rencinin ders kay�tlar�n� siliyoruz
                        context.OgrenciDersler.RemoveRange(ogrenciDersler);

                        // Sonra ��renciyi siliyoruz
                        context.Students.Remove(ogrenci);

                        // ��renci silindikten sonra aktif kontenjan� azalt�yoruz
                        var sinif = context.Siniflar.FirstOrDefault(s => s.SinifId == ogrenci.SinifId);
                        if (sinif != null)
                        {
                            sinif.AktifKontenjan--;
                        }

                        // De�i�iklikleri veritaban�na kaydet
                        context.SaveChanges();
                        MessageBox.Show("��renci ve ders kay�tlar� ba�ar�yla silindi.");
                    }
                }
                else
                {
                    // E�er ��renciye ait ders kayd� yoksa direkt ��renciyi silebiliriz
                    var sonuc = MessageBox.Show($"{ogrenci.Ad} {ogrenci.Soyad} adl� ��renciyi silmek istedi�inizden emin misiniz?",
                                                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (sonuc == DialogResult.Yes)
                    {
                        context.Students.Remove(ogrenci);

                        // ��renci silindikten sonra aktif kontenjan� azalt�yoruz
                        var sinif = context.Siniflar.FirstOrDefault(s => s.SinifId == ogrenci.SinifId);
                        if (sinif != null)
                        {
                            sinif.AktifKontenjan--;
                        }

                        context.SaveChanges();
                        MessageBox.Show("��renci ba�ar�yla silindi.");
                    }
                }
            }
            else
            {
                MessageBox.Show("L�tfen �nce silinecek bir ��renci se�in.", "��renci Se�ilmedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // ��lemi durdur
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
