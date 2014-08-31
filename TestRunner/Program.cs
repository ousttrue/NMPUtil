using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestRunner
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            NUnit.Gui.AppEntry.Main(new string[] { 
                System.IO.Path.GetDirectoryName(
                System.Windows.Forms.Application.ExecutablePath)+"\\Test.dll",     
                "/run" }
                );
            //Application.Run(new Form1());
        }
    }
}
