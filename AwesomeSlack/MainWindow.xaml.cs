using Awesomium.Core;
using Awesomium.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

            webControl.ShowCreatedWebView += webControl_ShowCreatedWebView;
            webControl.DocumentReady += OnDocumentReady;
            webControl.ConsoleMessage += OnConsoleMessage;

            notify = new NotifyIcon();
            notify.Icon = new Icon(SystemIcons.Exclamation, 40, 40);
            notify.Icon = new Icon("slack.ico");

            notify.Text = "AwesomeSlack";
            notify.Click += notify_Click;

            this.StateChanged += MainWindow_StateChanged;
        }

        
        Uri CleanUrl(Uri url)
        {
            string path = url.ToString();

            string slacklink = "https://slack-redir.com/link?url=";

            if (path.Contains(slacklink))
            {
                path = path.Remove(0, slacklink.Length);

                path = System.Net.WebUtility.UrlDecode(path);

                url = new Uri(path, true);
            }

            return url;

        }
        
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            // Destroy the WebControl and its underlying view.
            webControl.Dispose();
        }

        private void GetScreenshot( string fileName )
        {
            WebViewPresenter surface = webControl.Surface as WebViewPresenter;

            if ( surface == null )
                return;

            BitmapSource bitmap = surface.Image as BitmapSource;

            if ( bitmap == null )
                return;

            // For the sample, we use a PNG encoder. WPF provides
            // other too, such as a JpegBitmapEncoder.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add( BitmapFrame.Create( bitmap ) );

            // Open/Create the file to save the image to.
            using ( FileStream fs = File.Open( fileName, FileMode.OpenOrCreate ) )
                encoder.Save( fs );
        }

        private void PrintPageHTML()
        {
            ISynchronizeInvoke sync = (ISynchronizeInvoke)webControl;
            string html = sync.InvokeRequired ?
                sync.Invoke( (Func<String>)ExecuteJavascriptOnView, null ) as String :
                ExecuteJavascriptOnView();

            if ( !String.IsNullOrEmpty( html ) )
            {
                Debug.Print( html );
            }
        }

        // This method will be called on the WebControl's thread (the UI thread).
        private string ExecuteJavascriptOnView()
        {
            // This method is called from within DocumentReady so it already
            // executes in an asynchronous Javascript Execution Context (JEC).
            var global = Global.Current;

            if ( !global )
                return String.Empty;

            // We demonstrate the use of dynamic.
            var document = global.document;

            if ( !document )
                return String.Empty;

            // JSObject supports the DLR. You can dynamically call methods,
            // access arrays or lists and get or set properties.
            return document.getElementsByTagName( "html" )[ 0 ].outerHTML;
        }

        public Uri Source
        {
            get { return (Uri)GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source",
            typeof( Uri ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( null ) );


        // This will be set to the created child view that the WebControl will wrap,
        // when ShowCreatedWebView is the result of 'window.open'. The WebControl, 
        // is bound to this property.
        public IntPtr NativeView
        {
            get { return (IntPtr)GetValue( NativeViewProperty ); }
            private set { this.SetValue( MainWindow.NativeViewPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey NativeViewPropertyKey =
            DependencyProperty.RegisterReadOnly( "NativeView",
            typeof( IntPtr ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( IntPtr.Zero ) );

        /// <summary>
        /// Identifies the <see cref="NativeView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NativeViewProperty =
            NativeViewPropertyKey.DependencyProperty;

        // The visibility of the address bar and status bar, depends
        // on the value of this property. We set this to false when
        // the window wraps a WebControl that is the result of JavaScript
        // 'window.open'.
        public bool IsRegularWindow
        {
            get { return (bool)GetValue( IsRegularWindowProperty ); }
            private set { this.SetValue( MainWindow.IsRegularWindowPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsRegularWindowPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsRegularWindow",
            typeof( bool ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( true ) );

        /// <summary>
        /// Identifies the <see cref="IsRegularWindow"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsRegularWindowProperty =
            IsRegularWindowPropertyKey.DependencyProperty;

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
       
        private void OnDocumentReady( object sender, DocumentReadyEventArgs e )
        {
            // When ReadyState is Ready, you can execute JavaScript against
            // the DOM but all resources are not yet loaded. Wait for Loaded.
            if ( e.ReadyState == DocumentReadyState.Ready )
                return;

            // Get a screenshot of the view.
            GetScreenshot( "wpf_screenshot_before.png" );

            // Print the page's HTML source.
            PrintPageHTML();
        }

        private void onImageLoaded( object sender, JavascriptMethodEventArgs e )
        {
            Debug.Print( String.Format( "IMG with id: '{0}' completed loading: {1}", e.Arguments[ 0 ], e.Arguments[ 1 ] ) );
            // Get another screenshot.
            GetScreenshot( "wpf_screenshot_after.png" );
        }

        // Any JavaScript errors or JavaScript console.log calls,
        // will call this method.
        private void OnConsoleMessage( object sender, ConsoleMessageEventArgs e )
        {
            Debug.Print( "[Line: " + e.LineNumber + "] " + e.Message );
        }

        private void webControl_ShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            var uri = CleanUrl(e.TargetURL);
            //launch default web browser
            System.Diagnostics.Process.Start(uri.ToString());

            return;
        }

        private void webControl_WindowClose( object sender, WindowCloseEventArgs e )
        {
            // The page called 'window.close'. If the call
            // comes from a frame, ignore it.
            if ( !e.IsCalledFromFrame )
                this.Close();
        }
    }
}
