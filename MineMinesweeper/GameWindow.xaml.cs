using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using Point = MineMinesweeper.Util.Point;
namespace MineMinesweeper
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window, INotifyPropertyChanged
    {
        public enum Hint { NOTHING, FLAG, QUESTION };
        bool gameOn = true;
        Random rnd = new Random();
        int[,] mineMat;
        bool[,] revealedMat;
        Button[,] buttonMat;
        Hint[,] hintMat;
        int matHeight, matWidth, tileSize = 50, mines, seconds;
        int showMines;
        DispatcherTimer timer;
        public int ShownMines
        {
            get { return showMines; }

            set
            {
                showMines = value;
                OnPropertyChanged("ShownMines");
            }
        }

        bool firstClick = true;
        public GameWindow(int mines, int height, int width)
        {
            InitializeComponent();
            this.mines = ShownMines = mines;
            Mines.DataContext = this;
            matHeight = height;
            matWidth = width;
            mineMat = new int[matHeight, matWidth];
            revealedMat = new bool[matHeight, matWidth];
            buttonMat = new Button[matHeight, matWidth];
            hintMat = new Hint[matHeight, matWidth];
            SetWindow();
            Show();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Time.Text = (++seconds).ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetMat(Point firstP)
        {
            for (int i = 0; i < this.mines; i++)
            {
                Point rndP = new Point(rnd.Next(matWidth), rnd.Next(matHeight));
                if (!HasMine(rndP) && !firstP.Equals(rndP))
                {
                    mineMat[rndP.Y, rndP.X] = -1;
                    DoAround(rndP, (aroundP) =>
                    {
                        if (!HasMine(aroundP))
                        {
                            mineMat[aroundP.Y, aroundP.X]++;
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

        private bool HasMine(Point p)
        {
            return mineMat[p.Y, p.X] == -1;
        }

        private void DoAround(Point p, Action<Point> action)
        {
            Point aroundPoint = new Point(0, 0);
            for (aroundPoint.Y = p.Y - 1; aroundPoint.Y < p.Y + 2; aroundPoint.Y++)
            {
                for (aroundPoint.X = p.X - 1; aroundPoint.X < p.X + 2; aroundPoint.X++)
                {
                    if (InMat(aroundPoint))
                    {
                        if (!p.Equals(aroundPoint))
                        {
                            action(aroundPoint);
                        }
                    }
                }
            }
        }

        private IEnumerable<T> ReturnAround<T>(Point p, Func<Point, T> func)
        {
            Point aroundPoint = new Point(0, 0);
            for (aroundPoint.Y = p.Y - 1; aroundPoint.Y < p.Y + 2; aroundPoint.Y++)
            {
                for (aroundPoint.X = p.X - 1; aroundPoint.X < p.X + 2; aroundPoint.X++)
                {
                    if (InMat(aroundPoint))
                    {
                        if (!p.Equals(aroundPoint))
                        {
                            yield return func(aroundPoint);
                        }
                    }
                }
            }
        }
        private bool InMat(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < matWidth && p.Y < matHeight;
        }



        private void SetWindow()
        {
            Width = matWidth * tileSize;
            Height = matHeight * tileSize + 100;
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
            Point p = b.GetLocationInGrid();
            if (revealedMat[p.Y, p.X])
            {
                int flagCount = ReturnAround(p, (aroundP) =>
                {
                    return hintMat[aroundP.Y, aroundP.X] == Hint.FLAG;
                }).Count(f => f);

                if (flagCount == mineMat[p.Y, p.X])
                {
                    DoAround(p, (aroundP) =>
                    {
                        RevealTile(buttonMat[aroundP.Y, aroundP.X]);
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
                        ShownMines--;
                        b.Content = "F";
                        hint = Hint.FLAG;
                        break;
                    }
                case "F":
                    {
                        ShownMines++;
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
                Point p = b.GetLocationInGrid();
                if (firstClick)
                {
                    firstClick = false;
                    SetMat(p);
                    //OutMat();
                }
                if (!revealedMat[p.Y, p.X] && hintMat[p.Y, p.X] == Hint.NOTHING)
                {
                    revealedMat[p.Y, p.X] = true;
                    SetContent(p);
                    switch (mineMat[p.Y, p.X])
                    {
                        case -1:
                            {
                                ShowAllMines();
                                EndGame("You Lost!");
                                break;
                            }
                        case 0:
                            {
                                DoAround(p, (aroundP) =>
                                {
                                    RevealTile(buttonMat[aroundP.Y, aroundP.X]);
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

        private void ShowAllMines()
        {
            Point p = new Point(0, 0);
            for (p.Y = 0; p.Y < matHeight; p.Y++)
            {
                for (p.X = 0; p.X < matWidth; p.X++)
                {
                    if (HasMine(p) && hintMat[p.Y, p.X] != Hint.FLAG)
                    {
                        SetContent(p);
                    }
                    if (!HasMine(p) && hintMat[p.Y, p.X] == Hint.FLAG)
                    {
                        SetContent(p);
                        buttonMat[p.Y, p.X].Background = Brushes.Red;
                    }
                }
            }
        }

        private void EndGame(string message)
        {
            if (gameOn)
            {
                timer.Stop();
                gameOn = false;
                MessageBox.Show(message);
                new MainWindow().Show();
                Close();
            }
        }

        private void SetContent(Point p)
        {
            Button b = buttonMat[p.Y, p.X];
            int val = mineMat[p.Y, p.X];
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
