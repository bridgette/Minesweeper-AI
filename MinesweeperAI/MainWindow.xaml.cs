using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MinesweeperAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UIAutomation ui { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ui = new UIAutomation("minesweeper");
        }

        private void solveButtonClick(object sender, RoutedEventArgs e)
        {
            ui.GetScreenshotModernApp();
        }


    }
}