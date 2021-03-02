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
