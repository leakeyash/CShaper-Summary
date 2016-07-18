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

namespace Gobang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _interval = 20;
        private int _lineNum = 15;
        private int _margin = 5;
        private Dictionary<int,int> _chesses=new Dictionary<int, int>(); 
        int[,] _chessArray=new int[15,15];
        private int _chessCount = 0;
        public MainWindow()
        {
            InitializeComponent();
            DrawBord();
           
        }

        public void DrawBord()
        {
            int interval = _interval;
            int rowNum = _lineNum, columnNum = _lineNum;
            int width= interval * (columnNum-1), height = interval*(rowNum-1);
            int margin = _margin;
           
            for (int i = 0; i < rowNum; i++)
            {
                Line rowLine = new Line();
                rowLine.X1 = margin + 0;
                rowLine.X2 = margin + width;
                rowLine.Y1 = margin + i*interval;
                rowLine.Y2 = rowLine.Y1;
                rowLine.Stroke = Brushes.Black;
                this.ChessBoard.Children.Add(rowLine);
            }

            for (int i = 0; i < columnNum; i++)
            {
                Line Line = new Line();
                Line.X1 = margin + i * interval;
                Line.X2 = margin + i * interval;
                Line.Y1 = margin ;
                Line.Y2 = margin+height;
                Line.Stroke = Brushes.Black;
                this.ChessBoard.Children.Add(Line);
            }

        }

        private void ChessBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = Mouse.GetPosition(ChessBoard);
            double x = p.X - _margin;
            double y = p.Y - _margin;
            int xNum = (int)(x/_interval);
            int yNum= (int)(y / _interval);
            double xD = x%_interval;
            double yd = y%_interval;
            if (xD > (_interval/2)) xNum++;
            if (yd > (_interval / 2)) yNum++;

            int actualLeft = _margin + xNum*_interval-_interval/2;
            int actualTop = _margin + yNum * _interval-_interval/2;
            //if (_chesses.ContainsKey(actualLeft) && _chesses[actualLeft] == actualTop) return;

            //_chesses.Add(actualLeft,actualTop);
            if (_chessArray[xNum,yNum] != 0) return;

            _chessCount++;

            if (_chessCount%2==0)
            _chessArray[xNum,yNum] = 2;
            _chessArray[xNum, yNum] = 1;

            if (CheckWinner(xNum, yNum, _chessArray[xNum, yNum]))
                MessageBox.Show("Win");

            Ellipse ep=new Ellipse();
            ep.Height = ep.Width = _interval;
            //ep.Fill = e.ClickCount/2 == 0 ? Brushes.Black : Brushes.White;
            ep.Fill= _chessCount % 2!=0? Brushes.Black : Brushes.White;
            ChessBoard.Children.Add(ep);
            Canvas.SetLeft(ep,actualLeft);
            Canvas.SetTop(ep,actualTop);

        }

        public bool CheckWinner(int x,int y,int origin)
        {
            int left = x, right = x;
            while (left - 1 >= 0)
            {               
                if (_chessArray[left - 1, y] == origin)
                    left = left - 1;
                else
                {
                    break;
                }
            }
            if (right - left + 1 >= 5) return true;
            while (right + 1 < 15)
            {
                if (_chessArray[right+1, y] == origin)
                    right = right + 1;
                else
                {
                    break;
                }
            }
            if (right - left + 1 >= 5) return true;

            int top = y, bottom = y;
            while (top - 1 >= 0)
            {
                if (_chessArray[x,top - 1] == origin)
                    top = top - 1;
                else
                {
                    break;
                }
            }
            if (bottom - top + 1 >= 5) return true;
            while (bottom + 1 < 15)
            {
                if (_chessArray[x,bottom + 1] == origin)
                    bottom = bottom + 1;
                else
                {
                    break;
                }
            }
            if (bottom - top + 1 >= 5) return true;

            return false;
        }
    }
}
