//#define USE_GET_PHPSESSION

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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace webSimulator
{
    /// <summary>
    /// WebContainer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WebContainer : Window
    {
        private int _w;
        private int _h;
        private string _url;

#if USE_GET_PHPSESSION
        private string _myPhpSessionID;
#endif

        private int _currentTaskIDX = 0;
        private int _elapsedTicks;
        private int _waitTickCnts = 0;

        public WebContainer()
        {
            InitializeComponent();

            InitEventHook();
        }

        public void SetInfo(string url, int w, int h)
        {
            _url = url;
            _w = w;
            _h = h;

            this.Width = _w;
            this.Height = _h;
        }

        private void InitEventHook()
        {
            EventHook.MouseHook.Start();
            EventHook.MouseHook.MouseAction += new EventHandler(MyMouseEvent);
        }

        private void MyMouseEvent(object sender, EventArgs e)
        {
            Mouse.Capture(this);
            Point pointToWindow = Mouse.GetPosition(this);
            Point pointToScreen = PointToScreen(pointToWindow);
            Mouse.Capture(null);

            Console.WriteLine(string.Format("pos in window : {0} / pos in screen : {1}", pointToWindow.ToString(), pointToScreen));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myBrowser.Navigate(new Uri(_url));

            myBrowser.Width = _w;
            myBrowser.Height = _h;
            myBrowser.LoadCompleted += WebBrowser_LoadCompleted;
        }

        private async void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Console.WriteLine("page loaded");

#if USE_GET_PHPSESSION
            GetPhpSession();
#endif
            await Task.Delay(1000);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += new EventHandler(OnTick);
            dispatcherTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            //(sender as DispatcherTimer).Stop();

            ++_elapsedTicks;

            PlayTasks();
        }


#if USE_GET_PHPSESSION
        [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName,
               StringBuilder pchCookieData, ref System.UInt32 pcchCookieData,
               int dwFlags, IntPtr lpReserved);

        private void GetPhpSession()
        {
            //Get the cookie by name
            string cookieName = "PHPSESSID";
            int INTERNET_COOKIE_HTTPONLY = 0x00002000;
            StringBuilder cookie = new StringBuilder();
            System.UInt32 size = 256;
            InternetGetCookieEx(_url, cookieName, cookie, ref size,
                                INTERNET_COOKIE_HTTPONLY, IntPtr.Zero);

            string ck = cookie.ToString();
            string[] arr = ck.Split('=');
            _myPhpSessionID = arr[1];
        }
#endif

        private void PlayTasks()
        {
            if( 0 < _waitTickCnts)
            {
                --_waitTickCnts;
                return;
            }

            if ( _currentTaskIDX < TaskManager.Instance._taskInfo.task.Count )
            {
                TaskManager.TaskEnt t = TaskManager.Instance._taskInfo.task[_currentTaskIDX++];

                if ("click" == t.name)
                {
                    int x = 0;
                    int y = 0;
                    int.TryParse(t.p1, out x);
                    int.TryParse(t.p2, out y);

                    SystemFunctions.MouseClick(this, x, y);
                }
                else if ("move" == t.name)
                {
                    int x = 0;
                    int y = 0;
                    int.TryParse(t.p1, out x);
                    int.TryParse(t.p2, out y);

                    SystemFunctions.MouseMove(this, x, y);
                }
                else if ("wait" == t.name)
                {
                    int sec = 1;
                    int.TryParse(t.p1, out sec);

                    Console.WriteLine("wait ms :" + sec.ToString());
                    //SystemFunctions.Wait(this, ms);
                    _waitTickCnts = sec;
                }
                else if ("key" == t.name)
                {
                    SystemFunctions.Key(this, t.p1);
                }
                else if ("ckey" == t.name)
                {
                    byte code = 0;
                    byte.TryParse(t.p1, out code);
                    SystemFunctions.CKey(this, code);
                }
                else if ("viewSource" == t.name)
                {
                    byte code = 0;
                    byte.TryParse(t.p1, out code);

                    string errMsg;
                    string webSrc = WebUtil.WebText(myBrowser.Source.ToString(), out errMsg);

                    if( webSrc.Length == 0 || errMsg.Length != 0)
                    {
                        Console.WriteLine(string.Format("failed. url:{0} / errMsg:{1}" + myBrowser.Source.ToString(), errMsg));
                        return;
                    }

                    MessageBoxAutoHide dlg = new MessageBoxAutoHide();
                    dlg._msg = webSrc;
                    dlg.Owner = this;
                    dlg.Show();
                }
                else
                {
                    Console.WriteLine("not defined name : " + t.name);
                }
            }

        }

    }
}
