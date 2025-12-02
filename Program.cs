using System;
using System.Windows.Forms;

namespace Oyun4Islem
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Hile: open x
            CheatInfo cheat = CheatInfo.None();
            if (args.Length == 2 && args[0].ToLower() == "open")
            {
                string x = args[1].ToLower();
                if (x == "all") cheat = CheatInfo.All();
                else
                {
                    if (int.TryParse(x, out int lv) && lv >= 2 && lv <= 5)
                        cheat = CheatInfo.Level(lv);
                }
            }

            Application.Run(new MainForm(cheat));
        }
    }

    public class CheatInfo
    {
        public bool UseCheat { get; private set; }
        public bool UnlockAll { get; private set; }
        public int StartLevel { get; private set; }

        private CheatInfo() { }
        public static CheatInfo None() => new CheatInfo { UseCheat = false };
        public static CheatInfo All() => new CheatInfo { UseCheat = true, UnlockAll = true, StartLevel = 5 };
        public static CheatInfo Level(int lv) => new CheatInfo { UseCheat = true, UnlockAll = false, StartLevel = lv };
    }
}