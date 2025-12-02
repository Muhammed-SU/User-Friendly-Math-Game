using System;

namespace Oyun4Islem
{
    public enum Operation { Toplama, Cikarma, Carpma, Bolme }
    public enum QuestionStatus { Unanswered, Correct, Wrong, PassOnce, PassTwice }

    public class Question
    {
        public int Id { get; set; }
        public int Sayi1 { get; set; }
        public int Sayi2 { get; set; }
        public Operation Islem { get; set; }
        public int DogruCevap { get; set; }
        public int? KullaniciCevap { get; set; }
        public QuestionStatus Durum { get; set; } = QuestionStatus.Unanswered;
        public double ZorlukAgirlik { get; set; }

        public string Metin()
        {
            string op;
            switch (Islem)
            {
                case Operation.Toplama: op = "+"; break;
                case Operation.Cikarma: op = "-"; break;
                case Operation.Carpma: op = "×"; break;
                case Operation.Bolme: op = "÷"; break;
                default: op = "?"; break;
            }
            return $"{Sayi1} {op} {Sayi2} = ?";
        }

        public bool KontrolEt()
        {
            if (!KullaniciCevap.HasValue) return false;
            bool dogru = KullaniciCevap.Value == DogruCevap;
            Durum = dogru ? QuestionStatus.Correct : QuestionStatus.Wrong;
            return dogru;
        }

        public static double Agirlik(Operation op, int maxBasamak)
        {
            double baseW = 1.0;
            switch (op)
            {
                case Operation.Carpma: baseW = 1.5; break;
                case Operation.Bolme: baseW = 2.0; break;
                default: baseW = 1.0; break;
            }
            return baseW + (maxBasamak - 1) * 0.3;
        }
    }
}