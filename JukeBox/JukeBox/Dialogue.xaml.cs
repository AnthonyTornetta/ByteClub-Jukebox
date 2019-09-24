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
using System.Windows.Shapes;

namespace JukeBox
{
    /// <summary>
    /// Interaction logic for Dialogue.xaml
    /// </summary>
    public partial class Dialogue : Window
    {
        private string response = null;

        public Dialogue()
        {
            InitializeComponent();
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            response = TxtResponse.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static string Show()
        {
            Dialogue d = new Dialogue();
            if(d.ShowDialog() == true)
                return d.response;
            return null;
        }
    }
}
