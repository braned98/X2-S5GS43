using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for NodeColorWin.xaml
    /// </summary>
    public partial class NodeColorWin : Window
    {

        object UIElement;

        public NodeColorWin(object element)
        {
            InitializeComponent();
            this.UIElement = element;
        }

        private void Node_Loaded(object sender, RoutedEventArgs e)
        {
            NodeColor.ItemsSource = typeof(Colors).GetProperties();

            Line line = (Line)UIElement;

            this.LineID.Content = line.Uid.Split(':')[0];
        }

        private void YES_Click(object sender, MouseButtonEventArgs e)
        {

            if (this.UIElement.GetType() == typeof(Line))
            {

                Canvas canvas = this.Owner.FindName("canvas") as Canvas;

                Line line = (Line)this.UIElement;
                string id1 = line.Uid.Split(':')[1];
                string id2 = line.Uid.Split(':')[2];
                Ellipse circle1 = null;
                Ellipse circle2 = null;
                Color color = (Color)(NodeColor.SelectedItem as PropertyInfo).GetValue(1, null);

                foreach (UIElement el in canvas.Children)
                {
                    if (el.Uid == id1)
                    {
                        circle1 = (Ellipse)el;
                        (el as Ellipse).Fill = new SolidColorBrush(color);
                    }
                    if (el.Uid == id2)
                    {
                        circle2 = (Ellipse)el;
                        (el as Ellipse).Fill = new SolidColorBrush(color);
                    }
                }

            }
            this.Close();
        }

        private void NO_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
