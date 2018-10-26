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

namespace MineMinesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            switch (b.Content)
            {
                case "Easy":
                    {
                        new GameWindow(10, 9, 9);
                        break;
                    }
                case "Medium":
                    {
                        new GameWindow(40, 16, 16);
                        break;
                    }
                case "Hard":
                    {
                        new GameWindow(99, 16, 30);
                        break;
                    }
            }
            Close();
        }
    }
}
