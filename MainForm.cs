using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Oyun4Islem
{
    public class MainForm : Form
    {
        private Game game = new Game();
        private CheatInfo cheat;

        // UI
        private Label lblBaslik;
        private Label lblOyuncu;
        private TextBox txtOyuncu;
        private Button btnBasla;
        private Button btnDevam;
        private Button btnSkorlar;

        // Oyun UI
        private Label lblSeviye;
        private Label lblSure;
        private ProgressBar prgSoru;
        private FlowLayoutPanel pnlSorular;
        private Button btnSonrakiBlok;
        private Timer timer;

        private int aktifIndex = 0; // 0..19
        private int kalanSure = 0;
        private List<TextBox> cevapKutular = new List<TextBox>();
        private List<Button> passButonlari = new List<Button>();

        public MainForm(CheatInfo cheat)
        {
            this.cheat = cheat;
            Text = "4 İşlem Oyunu";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            OlusturAnaMenu();
        }

        private void OlusturAnaMenu()
        {
            Controls.Clear();

            lblBaslik = new Label { Text = "4 İşlem Oyunu", Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lblOyuncu = new Label { Text = "Oyuncu Adı:", Font = new Font("Segoe UI", 12), AutoSize = true, Location = new Point(20, 80) };
            txtOyuncu = new TextBox { Text = game.OyuncuAdi, Font = new Font("Segoe UI", 12), Location = new Point(130, 78), Width = 200 };

            btnBasla = new Button { Text = "Yeni Oyun", Font = new Font("Segoe UI", 12), Location = new Point(20, 130), Width = 150, Size = new Size(130, 50) };
            btnDevam = new Button { Text = "Devam Et", Font = new Font("Segoe UI", 12), Location = new Point(180, 130), Width = 150, Size = new Size(130, 50) };
            btnSkorlar = new Button { Text = "Skorlar", Font = new Font("Segoe UI", 12), Location = new Point(340, 130), Width = 150 , Size = new Size(130, 50) };

            btnBasla.Click += (s, e) =>
            {
                game.OyuncuAdi = txtOyuncu.Text.Trim().Length == 0 ? "Oyuncu" : txtOyuncu.Text.Trim();

                // Kilitleri sıfırla ve seviye 1’i açık bırak (set accessor hatası olmadan)
                game.KilitAcikSeviyeler.Clear();
                game.KilitAcikSeviyeler.Add(1);

                game.ToplamSkor = 0;
                game.BaslatSeviye(1);
                BaslatOyunEkrani();
            };

            btnDevam.Click += (s, e) =>
            {
                game.YukleProgress();
                game.OyuncuAdi = txtOyuncu.Text.Trim().Length == 0 ? game.OyuncuAdi : txtOyuncu.Text.Trim();

                // Komut satırı hilesini uygula (kilit set hatası olmadan içeriği değiştiriyoruz)
                if (cheat.UseCheat) game.UygulaHile(cheat);

                game.BaslatSeviye(game.AktifSeviye);
                BaslatOyunEkrani();
            };

            btnSkorlar.Click += (s, e) => GosterSkorlar();

            Controls.AddRange(new Control[] { lblBaslik, lblOyuncu, txtOyuncu, btnBasla, btnDevam, btnSkorlar });
        }

        private void BaslatOyunEkrani()
        {
            Controls.Clear();
            aktifIndex = 0;
            cevapKutular.Clear();
            passButonlari.Clear();

            lblSeviye = new Label { Text = $"Seviye: {game.AktifSeviye}", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lblSure = new Label { Text = "Süre: ", Font = new Font("Segoe UI", 16), AutoSize = true, Location = new Point(250, 20) };
            prgSoru = new ProgressBar { Location = new Point(20, 60), Width = 800, Height = 20, Minimum = 0, Maximum = 20, Value = 0 };
            pnlSorular = new FlowLayoutPanel { Location = new Point(20, 100), Width = 820, Height = 450, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            btnSonrakiBlok = new Button { Text = "Sonraki Blok", Font = new Font("Segoe UI", 12), Location = new Point(20, 570), Width = 150, Size = new Size(180, 50) };

            Controls.AddRange(new Control[] { lblSeviye, lblSure, prgSoru, pnlSorular, btnSonrakiBlok });

            kalanSure = game.MevcutLevel.SureLimitSn;
            timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                kalanSure--;
                lblSure.Text = $"Süre: {kalanSure} sn";
                if (kalanSure <= 0)
                {
                    timer.Stop();
                    // kalan sorular yanlış say
                    foreach (var q in game.MevcutLevel.Sorular.Where(x => x.Durum == QuestionStatus.Unanswered || x.Durum == QuestionStatus.PassOnce))
                    {
                        q.Durum = QuestionStatus.Wrong;
                        game.MevcutLevel.SonucuGuncelle(q);
                    }
                    BitirSeviye();
                }
            };
            timer.Start();

            btnSonrakiBlok.Click += (s, e) => SonrakiBlok();

            // İlk blok
            DoldurBlok();
        }

        private void DoldurBlok()
        {
            pnlSorular.Controls.Clear();
            cevapKutular.Clear();
            passButonlari.Clear();

            int bas = aktifIndex;
            int son = Math.Min(aktifIndex + 5, game.MevcutLevel.Sorular.Count);
            for (int i = bas; i < son; i++)
            {
                var q = game.MevcutLevel.Sorular[i];
                var panel = new Panel { Width = 780, Height = 80, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(5) };

                var lbl = new Label { Text = $"S{q.Id}: {q.Metin()}", Font = new Font("Segoe UI", 14), AutoSize = true, Location = new Point(10, 10) };
                var txt = new TextBox { Font = new Font("Segoe UI", 14), Location = new Point(10, 40), Width = 120 };
                var btnCevapla = new Button { Text = "Cevapla", Font = new Font("Segoe UI", 12), Location = new Point(140, 38), Width = 100, Size = new Size(80, 35) };
                var btnPas = new Button { Text = "PAS", Font = new Font("Segoe UI", 12), Location = new Point(250, 38), Width = 80, Size = new Size(80, 35) };

                btnCevapla.Click += (s, e) =>
                {
                    if (q.Durum == QuestionStatus.PassTwice || q.Durum == QuestionStatus.Correct || q.Durum == QuestionStatus.Wrong)
                        return;

                    int cevap;
                    if (int.TryParse(txt.Text.Trim(), out cevap))
                    {
                        q.KullaniciCevap = cevap;
                        bool dogru = q.KontrolEt();
                        game.MevcutLevel.SonucuGuncelle(q);
                        btnCevapla.Enabled = false;
                        btnPas.Enabled = false;
                        txt.Enabled = false;
                        prgSoru.Value = Math.Min(prgSoru.Value + 1, 20);
                        lbl.Text = $"S{q.Id}: {q.Metin()}  [{(dogru ? "Doğru" : "Yanlış")}]";
                    }
                    else
                    {
                        MessageBox.Show("Lütfen sayı girin.", "Uyarı");
                    }
                };

                btnPas.Click += (s, e) =>
                {
                    if (q.Durum == QuestionStatus.Unanswered)
                    {
                        q.Durum = QuestionStatus.PassOnce;
                        game.MevcutLevel.SonucuGuncelle(q);
                        btnCevapla.Enabled = false;
                        btnPas.Enabled = false;
                        txt.Enabled = false;
                        prgSoru.Value = Math.Min(prgSoru.Value + 1, 20);
                        lbl.Text = $"S{q.Id}: {q.Metin()}  [PAS 1]";
                    }
                    else if (q.Durum == QuestionStatus.PassOnce)
                    {
                        q.Durum = QuestionStatus.PassTwice;
                        // ikinci PAS anında yanlış sayılır
                        game.MevcutLevel.SonucuGuncelle(q);
                        btnCevapla.Enabled = false;
                        btnPas.Enabled = false;
                        txt.Enabled = false;
                        lbl.Text = $"S{q.Id}: {q.Metin()}  [PAS 2 → Yanlış]";
                    }
                };

                panel.Controls.AddRange(new Control[] { lbl, txt, btnCevapla, btnPas });
                pnlSorular.Controls.Add(panel);

                cevapKutular.Add(txt);
                passButonlari.Add(btnPas);
            }
        }

        private void SonrakiBlok()
        {
            int sonraki = Math.Min(aktifIndex + 5, game.MevcutLevel.Sorular.Count);
            bool blokBitti = true;
            for (int i = aktifIndex; i < sonraki; i++)
            {
                var q = game.MevcutLevel.Sorular[i];
                if (q.Durum == QuestionStatus.Unanswered || q.Durum == QuestionStatus.PassOnce)
                {
                    blokBitti = false;
                    break;
                }
            }

            // Kullanıcı isterse ilerleyebilsin, zorlamıyoruz
            aktifIndex = sonraki;

            if (aktifIndex >= game.MevcutLevel.Sorular.Count)
            {
                // Seviye 20 soru bitti → PASS kuyruğunu göster
                if (game.MevcutLevel.PassKuyrugu.Count > 0)
                {
                    var yenidenListe = new List<Question>();
                    while (game.MevcutLevel.PassKuyrugu.Count > 0)
                        yenidenListe.Add(game.MevcutLevel.PassKuyrugu.Dequeue());

                    // ikinci tur soruları listeye ekle
                    game.MevcutLevel.Sorular.AddRange(yenidenListe);
                    DoldurBlok();
                    return;
                }
                else
                {
                    BitirSeviye();
                    return;
                }
            }

            DoldurBlok();
        }

        private void BitirSeviye()
        {
            if (timer != null) timer.Stop();

            int yildiz = game.MevcutLevel.Yildiz();
            int skor = game.HesaplaSeviyeSkoru(game.MevcutLevel, (kalanSure > 0 ? kalanSure : 0));
            game.ToplamSkor += skor;

            game.KaydetSkor(new ScoreEntry
            {
                Ad = game.OyuncuAdi,
                Seviye = game.AktifSeviye,
                Skor = skor,
                Yildiz = yildiz,
                Tarih = DateTime.Now
            });

            bool gecti = game.MevcutLevel.DogruSayisi >= 11;
            if (gecti && game.AktifSeviye < 5)
            {
                game.KilitAcikSeviyeler.Add(game.AktifSeviye + 1);
                game.AktifSeviye = game.AktifSeviye + 1;
            }
            game.KaydetProgress();

            var dlg = MessageBox.Show(
                "Seviye " + game.MevcutLevel.SeviyeNo + " bitti!\n" +
                "Doğru: " + game.MevcutLevel.DogruSayisi + ", Yanlış: " + game.MevcutLevel.YanlisSayisi + ", PAS: " + game.MevcutLevel.PasSayisi + "\n" +
                "Yıldız: " + yildiz + ", Skor: " + skor + "\n" +
                (gecti ? "Seviye geçti! Sonraki seviyeye geçilsin mi?" : "Seviye geçilemedi. Tekrar denensin mi?"),
                "Özet",
                MessageBoxButtons.YesNo);

            if (dlg == DialogResult.Yes)
            {
                game.BaslatSeviye(game.AktifSeviye);
                BaslatOyunEkrani();
            }
            else
            {
                OlusturAnaMenu();
            }
        }

        private void GosterSkorlar()
        {
            var list = game.YukleSkorlar();
            var form = new Form
            {
                Text = "Skorlar",
                Width = 600,
                Height = 400,
                StartPosition = FormStartPosition.CenterParent
            };
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                DataSource = list
            };
            form.Controls.Add(grid);
            form.ShowDialog(this);
        }
    }
}