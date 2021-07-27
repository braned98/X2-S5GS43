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

namespace PR24_2017_PZ2
{
    /// <summary>
    /// Interaction logic for DimensionSetting.xaml
    /// </summary>
    public partial class DimensionSetting : Window
    {
        public MainWindow mainWin;

        public DimensionSetting(MainWindow mainW)
        {
            InitializeComponent();
            this.mainWin = mainW;
        }
        

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                this.mainWin.canvas.Children.Clear();
                this.mainWin.size = Int32.Parse(sizeValue.Text);
                this.mainWin.subEnt.Clear();
                this.mainWin.nodeEnt.Clear();
                this.mainWin.swcEnt.Clear();
                this.mainWin.lineEnt.Clear();
                this.mainWin.lines.Clear();
                this.mainWin.drawnLines.Clear();
                this.mainWin.openSwcLine.Clear();
                this.mainWin.CallLoad();
                this.Close();
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool Validate()
        {
            try
            {
                int size = Int32.Parse(sizeValue.Text);
                if(size > 99)
                {
                    return true;
                }
            }
            catch
            {
                errorSize.Foreground = Brushes.Red;
                return false;
            }
            errorSize.Foreground = Brushes.Red;
            return false;
        }

    }
}
