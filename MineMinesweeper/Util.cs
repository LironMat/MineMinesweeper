using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MineMinesweeper
{
    public static class Util
    {
        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Point))
                    return false;
                Point p = (Point)obj;
                return p.X == X && p.Y == Y;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public static Point GetLocationInGrid(this UIElement element)
        {
            return new Point(Grid.GetColumn(element), Grid.GetRow(element));
        }
    }
}
