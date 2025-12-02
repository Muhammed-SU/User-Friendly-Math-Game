using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Oyun4Islem
{
    public class ScoreEntry
    {
        public string Ad { get; set; }
        public int Seviye { get; set; }
        public int Skor { get; set; }
        public int Yildiz { get; set; }
        public DateTime Tarih { get; set; }
    }

    public class Game
    {
        public string OyuncuAdi { get; set; } = "Oyuncu";
        public int AktifSeviye { get; set; } = 1;
        public int ToplamSkor { get; set; } = 0;

        // set yerine sadece get var, dışarıdan Clear/Add ile değiştiriyoruz
        public HashSet<int> KilitAcikSeviyeler { get; private set; } = new HashSet<int> { 1 };

        public Random rng { get; } = new Random();
        public Level MevcutLevel { get; private set; }

        public void BaslatSeviye(int seviyeNo)
        {
            AktifSeviye = seviyeNo;
            int sure = 180 + (seviyeNo - 1) * 30;
            MevcutLevel = new Level(seviyeNo, sure, rng);
            MevcutLevel.SoruUret();
        }

        public void UygulaHile(CheatInfo cheat)
        {
            if (!cheat.UseCheat) return;

            if (cheat.UnlockAll)
            {
                KilitAcikSeviyeler.Clear();
                KilitAcikSeviyeler.UnionWith(new[] { 1, 2, 3, 4, 5 });
                AktifSeviye = cheat.StartLevel;
            }
            else
            {
                KilitAcikSeviyeler.Add(cheat.StartLevel);
                AktifSeviye = cheat.StartLevel;
            }

            KaydetProgress();
        }

        public void KaydetProgress()
        {
            try
            {
                File.WriteAllText("progress.txt",
                    $"OyuncuAdi={OyuncuAdi}\nSeviye={AktifSeviye}\nSkor={ToplamSkor}\nKilitAcik={string.Join(",", KilitAcikSeviyeler.OrderBy(x => x))}");
            }
            catch { }
        }

        public void YukleProgress()
        {
            try
            {
                if (!File.Exists("progress.txt")) return;
                var satirlar = File.ReadAllLines("progress.txt");

                foreach (var s in satirlar)
                {
                    var p = s.Split('=');
                    if (p.Length != 2) continue;

                    if (p[0] == "OyuncuAdi") OyuncuAdi = p[1];
                    else if (p[0] == "Seviye")
                    {
                        int tmp;
                        if (int.TryParse(p[1], out tmp))
                            AktifSeviye = tmp;
                    }
                    else if (p[0] == "Skor")
                    {
                        int tmp;
                        if (int.TryParse(p[1], out tmp))
                            ToplamSkor = tmp;
                    }
                    else if (p[0] == "KilitAcik")
                    {
                        KilitAcikSeviyeler.Clear();
                        foreach (var v in p[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            int tmp;
                            if (int.TryParse(v, out tmp))
                                KilitAcikSeviyeler.Add(tmp);
                        }
                        if (KilitAcikSeviyeler.Count == 0) KilitAcikSeviyeler.Add(1);
                    }
                }
            }
            catch { }
        }

        public void KaydetSkor(ScoreEntry entry)
        {
            try
            {
                string line = $"{entry.Ad};{entry.Seviye};{entry.Skor};{entry.Yildiz};{entry.Tarih:yyyy-MM-dd HH:mm}";
                File.AppendAllLines("leaderboard.txt", new[] { line });
            }
            catch { }
        }

        public List<ScoreEntry> YukleSkorlar()
        {
            var list = new List<ScoreEntry>();
            try
            {
                if (!File.Exists("leaderboard.txt")) return list;
                foreach (var line in File.ReadAllLines("leaderboard.txt"))
                {
                    var p = line.Split(';');
                    int lv, skor, yildiz;
                    DateTime tarih;

                    if (p.Length >= 5 &&
                        int.TryParse(p[1], out lv) &&
                        int.TryParse(p[2], out skor) &&
                        int.TryParse(p[3], out yildiz) &&
                        DateTime.TryParse(p[4], out tarih))
                    {
                        list.Add(new ScoreEntry
                        {
                            Ad = p[0],
                            Seviye = lv,
                            Skor = skor,
                            Yildiz = yildiz,
                            Tarih = tarih
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        public int HesaplaSeviyeSkoru(Level level, int kalanSure)
        {
            double toplam = 0;
            foreach (var q in level.Sorular)
            {
                if (q.Durum == QuestionStatus.Correct)
                {
                    double timeBonus = 1.0 + (double)kalanSure / (level.SureLimitSn + 1) * 0.5;
                    toplam += 10.0 * q.ZorlukAgirlik * timeBonus / 20.0;
                }
            }
            return (int)Math.Round(toplam * 20);
        }
    }
}