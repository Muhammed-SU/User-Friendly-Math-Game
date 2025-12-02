using System;
using System.Collections.Generic;

namespace Oyun4Islem
{
    public class Level
    {
        public int SeviyeNo { get; private set; }
        public List<Question> Sorular { get; private set; } = new List<Question>();
        public Queue<Question> PassKuyrugu { get; private set; } = new Queue<Question>();
        public int DogruSayisi { get; private set; }
        public int YanlisSayisi { get; private set; }
        public int PasSayisi { get; private set; }
        public int SureLimitSn { get; private set; }
        public List<Operation> IzinliIslemler { get; private set; } = new List<Operation> { Operation.Toplama, Operation.Cikarma, Operation.Carpma, Operation.Bolme };
        private Random rng;

        public Level(int seviyeNo, int sureLimitSn, Random rng)
        {
            SeviyeNo = seviyeNo;
            SureLimitSn = sureLimitSn;
            this.rng = rng;
        }

        public void SoruUret()
        {
            Sorular.Clear();
            PassKuyrugu.Clear();
            int id = 1;
            int minBasamak = 1;
            int maxBasamak = 1;

            if (SeviyeNo == 2) { maxBasamak = 2; }
            else if (SeviyeNo == 3) { minBasamak = 2; maxBasamak = 2; }
            else if (SeviyeNo >= 4) { minBasamak = 2; maxBasamak = 3; }

            for (int i = 0; i < 20; i++)
            {
                Operation op = IzinliIslemler[rng.Next(IzinliIslemler.Count)];
                int a = RastgeleSayi(minBasamak, maxBasamak);
                int b = RastgeleSayi(minBasamak, maxBasamak);
                int cevap = 0;

                if (op == Operation.Bolme)
                {
                    // Tam sayı sonuç olsun
                    b = Math.Max(1, RastgeleSayi(minBasamak, maxBasamak));
                    int k = rng.Next(1, 9);
                    a = b * k;
                    cevap = k;
                }
                else if (op == Operation.Cikarma)
                {
                    if (a < b) { int t = a; a = b; b = t; }
                    cevap = a - b;
                }
                else if (op == Operation.Toplama)
                {
                    cevap = a + b;
                }
                else if (op == Operation.Carpma)
                {
                    cevap = a * b;
                }

                var q = new Question
                {
                    Id = id++,
                    Sayi1 = a,
                    Sayi2 = b,
                    Islem = op,
                    DogruCevap = cevap,
                    ZorlukAgirlik = Question.Agirlik(op, maxBasamak)
                };
                Sorular.Add(q);
            }
        }

        private int RastgeleSayi(int minBasamak, int maxBasamak)
        {
            int basamak = rng.Next(minBasamak, maxBasamak + 1); // [min, max]
            int min = (int)Math.Pow(10, basamak - 1);
            int max = (int)Math.Pow(10, basamak) - 1;
            if (basamak == 1) min = 1; // 0’dan kaçın
            return rng.Next(min, max + 1);
        }

        public void SonucuGuncelle(Question q)
        {
            switch (q.Durum)
            {
                case QuestionStatus.Correct: DogruSayisi++; break;
                case QuestionStatus.Wrong: YanlisSayisi++; break;
                case QuestionStatus.PassOnce:
                    PasSayisi++;
                    PassKuyrugu.Enqueue(q);
                    break;
                case QuestionStatus.PassTwice:
                    YanlisSayisi++; // ikinci pas yanlış kabul
                    break;
            }
        }

        public int Yildiz()
        {
            if (DogruSayisi >= 19) return 3;
            if (DogruSayisi >= 16) return 2;
            if (DogruSayisi >= 11) return 1;
            return 0;
        }
    }
}