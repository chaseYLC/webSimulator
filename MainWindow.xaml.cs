using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace webSimulator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TaskManager.Instance.LoadFile();
        }

        private void StartSimulator(object sender, RoutedEventArgs e)
        {
            int w = 800;
            int h = 600;

            int.TryParse(txtW.Text, out w);
            int.TryParse(txtH.Text, out h);

            WebContainer wnd = new WebContainer();
            wnd.SetInfo(txtURL.Text, w, h);
            wnd.Owner = this;
            wnd.Show();
        }
    }
}
