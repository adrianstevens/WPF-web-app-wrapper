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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AwesomeSlack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon notify = null;

        public MainWindow()
        {
            InitializeComponent();

    //        Uri iconUri = new Uri("pack://application:,,,/slack.ico", UriKind.RelativeOrAbsolute);
    //        this.Icon = BitmapFrame.Create(iconUri);

            notify = new NotifyIcon();
            notify.Icon = new Icon(SystemIcons.Exclamation, 40, 40);
            notify.Icon = new Icon("slack.ico");

            notify.Text = "AwesomeSlack";
            notify.Click += notify_Click;

            this.StateChanged += MainWindow_StateChanged;
        }

        void notify_Click(object sender, EventArgs e)
        {
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Normal;
            }
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Normal:
                    notify.Visible = false;
                    this.ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    notify.Visible = true;
                    this.ShowInTaskbar = false;
                    break;
                }
        }
    }
}
