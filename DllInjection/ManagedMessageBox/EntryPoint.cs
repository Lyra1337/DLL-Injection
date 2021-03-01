using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ManagedMessageBox
{
    public class EntryPoint
    {
        [DllExport("DllMain")]
        public static void DllMain()
        {
            MessageBox.Show("Moin");
        }
    }
}
