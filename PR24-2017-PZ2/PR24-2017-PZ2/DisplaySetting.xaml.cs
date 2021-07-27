using Microsoft.Win32;
using PR24_2017_PZ2.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PR24_2017_PZ2
{
    /// <summary>
    /// Interaction logic for DisplaySetting.xaml
    /// </summary>
    public partial class DisplaySetting : Window
    {
        public MainWindow mainWin;
        string NodeCol = "";
        string SwcCol = "";
        string SubCol = "";
        BitmapImage NodeP = null;
        BitmapImage SwcP = null;
        BitmapImage SubP = null;

        List<string> colors = new List<string>();

        Canvas canvas;

        public DisplaySetting(MainWindow mainW)
        {
            InitializeComponent();
            this.mainWin = mainW;
            this.canvas = mainWin.canvas;
        }

        private void DisplaySettingLoaded(object sender, RoutedEventArgs e)
        {
            NodeColor.ItemsSource = typeof(Colors).GetProperties();
            SwcColor.ItemsSource = typeof(Colors).GetProperties();
            SubColor.ItemsSource = typeof(Colors).GetProperties();

            

            foreach (var prop in typeof(Colors).GetProperties())
            {
                colors.Add(prop.Name);
            }




            foreach (UIElement v in canvas.Children)
            {
                long id = long.Parse(v.Uid);
                if (this.mainWin.subEnt.ContainsKey(id))
                {
                    if ((v as Ellipse).Fill.GetType() != typeof(ImageBrush))
                    {
                        SubCol = GetColorName(new BrushConverter().ConvertFromString((v as Ellipse).Fill.ToString()) as SolidColorBrush);
                        break;
                    }
                }   
            }

            SubColor.SelectedIndex = colors.IndexOf(SubCol);

            foreach (UIElement v in canvas.Children)
            {
                long id = long.Parse(v.Uid);
                if (this.mainWin.swcEnt.ContainsKey(id))
                {
                    if ((v as Ellipse).Fill.GetType() != typeof(ImageBrush))
                    {
                        SwcCol = GetColorName(new BrushConverter().ConvertFromString((v as Ellipse).Fill.ToString()) as SolidColorBrush);
                        break;
                    }
                }
            }

            SwcColor.SelectedIndex = colors.IndexOf(SwcCol);

            foreach (UIElement v in canvas.Children)
            {
                long id = long.Parse(v.Uid);
                if (this.mainWin.nodeEnt.ContainsKey(id))
                {
                    if ((v as Ellipse).Fill.GetType() != typeof(ImageBrush))
                    {
                        NodeCol = GetColorName(new BrushConverter().ConvertFromString((v as Ellipse).Fill.ToString()) as SolidColorBrush);
                        break;
                    }
                }
            }

            NodeColor.SelectedIndex = colors.IndexOf(NodeCol);

        }

        private void OpenFile_Click1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                NodeP = new BitmapImage(new Uri(openFileDialog.FileName));
                nodePic.Source = NodeP;
            }
        }

        private void OpenFile_Click2(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                SwcP = new BitmapImage(new Uri(openFileDialog.FileName));
                swcPic.Source = SwcP;
            }
        }

        private void OpenFile_Click3(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                SubP = new BitmapImage(new Uri(openFileDialog.FileName));
                subPic.Source = SubP;
            }
        }

        private string GetColorName(SolidColorBrush brush)
        {
            var results = typeof(Colors).GetProperties().Where(
             p => (System.Windows.Media.Color)p.GetValue(null, null) == brush.Color).Select(p => p.Name);
            return results.Count() > 0 ? results.First() : String.Empty;
        }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            if(NodeP != null)
            {
                foreach(UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.nodeEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        (el as Ellipse).Fill = new ImageBrush(NodeP);
                    }
                }
            }
            if (SwcP != null)
            {
                foreach (UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.swcEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        (el as Ellipse).Fill = new ImageBrush(SwcP);
                    }
                }
            }
            if (SubP != null)
            {
                foreach (UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.subEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        (el as Ellipse).Fill = new ImageBrush(SubP);
                    }
                }
            }
            if(NodeP == null && NodeColor.SelectedIndex != -1)
            {
                foreach (UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.nodeEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        System.Windows.Media.Color col = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colors[NodeColor.SelectedIndex]);
                        (el as Ellipse).Fill = new SolidColorBrush(col);
                    }
                }
            }
            if (SwcP == null && SwcColor.SelectedIndex != -1)
            {
                foreach (UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.swcEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        System.Windows.Media.Color col = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colors[SwcColor.SelectedIndex]);
                        (el as Ellipse).Fill = new SolidColorBrush(col);
                    }
                }
            }
            if (SubP == null && SubColor.SelectedIndex != -1)
            {
                foreach (UIElement el in mainWin.canvas.Children)
                {
                    if (mainWin.subEnt.ContainsKey(long.Parse(el.Uid)))
                    {
                        System.Windows.Media.Color col = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colors[SubColor.SelectedIndex]);
                        (el as Ellipse).Fill = new SolidColorBrush(col);
                    }
                }
            }
            this.Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
