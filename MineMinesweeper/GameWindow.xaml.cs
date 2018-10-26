using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public enum Hint { NOTHING, FLAG, QUESTION };
        bool gameOn = true;
        Random rnd = new Random();
        int[,] mineMat;
        bool[,] revealedMat;
        Button[,] buttonMat;
        Hint[,] hintMat;
        int matHeight, matWidth, tileSize = 50, mines;
        bool firstClick = true;
        public GameWindow(int mines, int height, int width)
        {
            InitializeComponent();
            this.mines = mines;
            matHeight = height;
            matWidth = width;
            mineMat = new int[matHeight, matWidth];
            revealedMat = new bool[matHeight, matWidth];
            buttonMat = new Button[matHeight, matWidth];
            hintMat = new Hint[matHeight, matWidth];
            SetWindow();
            Show();
        }

        private void SetMat(int firstX, int firstY)
        {
            for (int i = 0; i < this.mines; i++)
            {
                int rndX = rnd.Next(matWidth);
                int rndY = rnd.Next(matHeight);
                if (mineMat[rndY, rndX] != -1 && !(rndX == firstX && rndY == firstY))
                {
                    mineMat[rndY, rndX] = -1;
                    DoAround(rndX, rndY, (aroundY, aroundX) =>
                    {
                        if (mineMat[aroundY, aroundX] != -1)
                        {
                            mineMat[aroundY, aroundX]++;
                        }
                    }
                    );
                }
                else
                {
                    i--;
                }
            }
        }

        private void DoAround(int x, int y, Action<int, int> action)
        {
            for (int aroundY = y - 1; aroundY < y + 2; aroundY++)
            {
                for (int aroundX = x - 1; aroundX < x + 2; aroundX++)
                {
                    if (aroundX >= 0 && aroundY >= 0 && aroundX < matWidth && aroundY < matHeight)
                    {
                        if (!(aroundX == x && aroundY == y))
                        {
                            action(aroundY, aroundX);
                        }
                    }
                }
            }
        }

        private IEnumerable<T> ReturnAround<T>(int x, int y, Func<int, int, T> func)
        {
            for (int aroundY = y - 1; aroundY < y + 2; aroundY++)
            {
                for (int aroundX = x - 1; aroundX < x + 2; aroundX++)
                {
                    if (aroundX >= 0 && aroundY >= 0 && aroundX < matWidth && aroundY < matHeight)
                    {
                        if (!(aroundX == x && aroundY == y))
                        {
                            yield return func(aroundY, aroundX);
                        }
                    }
                }
            }
        }

        private void SetWindow()
        {
            Width = matWidth * tileSize;
            Height = matHeight * tileSize;
            for (int x = 0; x < matWidth; x++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int y = 0; y < matHeight; y++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                for (int x = 0; x < matWidth; x++)
                {
                    Button b = buttonMat[y, x] = new Button();
                    b.Click += TileClick;
                    b.MouseRightButtonDown += FlagClick;
                    b.MouseDoubleClick += B_MouseDoubleClick;
                    grid.Children.Add(b);
                    Grid.SetRow(b, y);
                    Grid.SetColumn(b, x);
                }
            }
        }

        private void B_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;
            int y = Grid.GetRow(b);
            int x = Grid.GetColumn(b);
            if (revealedMat[y, x])
            {
                int flagCount = ReturnAround(x, y, (aroundY, aroundX) =>
                {
                    return hintMat[aroundY, aroundX] == Hint.FLAG;
                }).Count(f => f);

                if (flagCount == mineMat[y, x])
                {
                    DoAround(x, y, (aroundY, aroundX) =>
                    {
                        RevealTile(buttonMat[aroundY, aroundX]);
                    });
                }
            }
        }

        private void FlagClick(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;
            Hint hint = Hint.NOTHING;
            switch (b.Content)
            {
                case null:
                    {
                        b.Content = "F";
                        hint = Hint.FLAG;
                        break;
                    }
                case "F":
                    {
                        b.Content = "?";
                        hint = Hint.QUESTION;
                        break;
                    }
                case "?":
                    {
                        b.Content = null;
                        hint = Hint.NOTHING;
                        break;
                    }
            }
            int y = Grid.GetRow(b);
            int x = Grid.GetColumn(b);
            hintMat[y, x] = hint;
        }

        private void TileClick(object sender, RoutedEventArgs e)
        {
            RevealTile((Button)sender);
        }

        private void RevealTile(Button b)
        {
            if (gameOn)
            {
                int y = Grid.GetRow(b);
                int x = Grid.GetColumn(b);
                if (firstClick)
                {
                    firstClick = false;
                    SetMat(x, y);
                    OutMat();
                }
                if (!revealedMat[y, x] && hintMat[y, x] == Hint.NOTHING)
                {
                    revealedMat[y, x] = true;
                    SetContent(b, mineMat[y, x]);
                    switch (mineMat[y, x])
                    {
                        case -1:
                            {
                                EndGame("You Lost!");
                                break;
                            }
                        case 0:
                            {
                                DoAround(x, y, (aroundY, aroundX) =>
                                {
                                    RevealTile(buttonMat[aroundY, aroundX]);
                                });
                                break;
                            }

                    }
                    if (revealedMat.Cast<bool>().ToList().Count(f => !f) == this.mines)
                    {
                        EndGame("You Won!");
                    }
                }
            }
        }

        private void EndGame(string message)
        {
            if (gameOn)
            {
                gameOn = false;
                MessageBox.Show(message);
                new MainWindow().Show();
                Close();
            }
        }

        private void SetContent(Button b, int val)
        {
            b.Background = Brushes.White;
            switch (val)
            {
                case -1:
                    {
                        b.Foreground = Brushes.Black;
                        b.Content = "*";
                        break;
                    }
                case 0:
                    {
                        b.Foreground = Brushes.Black;
                        b.Content = "";
                        break;
                    }
                case 1:
                    {
                        b.Foreground = Brushes.Blue;
                        b.Content = "1";
                        break;
                    }
                case 2:
                    {
                        b.Foreground = Brushes.Green;
                        b.Content = "2";
                        break;
                    }
                case 3:
                    {
                        b.Foreground = Brushes.Red;
                        b.Content = "3";
                        break;
                    }
                case 4:
                    {
                        b.Foreground = Brushes.DarkBlue;
                        b.Content = "4";
                        break;
                    }
                case 5:
                    {
                        b.Foreground = Brushes.DarkRed;
                        b.Content = "5";
                        break;
                    }
                case 6:
                    {
                        b.Foreground = Brushes.Cyan;
                        b.Content = "6";
                        break;
                    }
                case 7:
                    {
                        b.Foreground = Brushes.Black;
                        b.Content = "7";
                        break;
                    }
                case 8:
                    {
                        b.Foreground = Brushes.Gray;
                        b.Content = "8";
                        break;
                    }
            }
        }

        private void OutMat()
        {
            using (StreamWriter fs = new StreamWriter(new FileStream("xd.txt", FileMode.Append)))
            {
                for (int y = 0; y < mineMat.GetLength(0); y++)
                {
                    for (int x = 0; x < mineMat.GetLength(1); x++)
                    {
                        fs.Write(mineMat[y, x].ToString().PadLeft(2) + " ");
                    }
                    fs.WriteLine();
                }
                fs.WriteLine();
            }
        }
    }
}
