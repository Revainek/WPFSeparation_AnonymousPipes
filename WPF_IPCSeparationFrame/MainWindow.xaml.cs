using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_IPCSeparationFrame.IPCSeparation_Host;

namespace WPF_IPCSeparationFrame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LongRunningProcess_Host _host = new LongRunningProcess_Host();
        public MainWindow()
        {
            InitializeComponent();

            ProcessingExample();
        }

        private void ProcessingExample()
        {
            ///Example is made based on strings, but any Serializable Type can fit in instead.
            List<string> Data = new List<string> { "string1", "string2", "string3", "string4", "stringN" };
            var result = _host.EnqueueTestListForShuttering(Data);
        }
    }
}