using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CANV2ProtocolDemoClient
{
    static class Program
    {
        // Version of the CRI Demo Client
        // Also update the lifecycle info in sbomify.json!
        public static readonly int SWVersionMajor = 1;
        public static readonly int SWVersionMinor = 2;
        public static readonly int SWVersionPatch = 0;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
