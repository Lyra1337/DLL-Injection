using System.Windows.Forms;

namespace ManagedMessageBox
{
    public class EntryPoint
    {
        [DllExport("DllMain", System.Runtime.InteropServices.CallingConvention.Winapi)]
        public static void DllMain()
        {
            MessageBox.Show("DllMain");
        }

        public static void Main()
        {
            MessageBox.Show("Main");
        }

        [DllExport("Initialize")]
        public static void Initialize(int i)
        {
            MessageBox.Show("Initialize");
        }
    }
}
