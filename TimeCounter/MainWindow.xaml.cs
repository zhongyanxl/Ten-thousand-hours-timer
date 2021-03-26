using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TimeCounter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// Author: nekopg
    /// Date: 2018-12-25
    /// </summary>
    public partial class MainWindow : Window
    {
        static int SAVETIME = 3;
        static int LAZYTIME = 60;
        int h = 9999, m = 59, s = 59;
        int lazy_time = LAZYTIME;
        bool isStart = false;
        int saveTime = SAVETIME;
        private KeyEventHandler myKeyEventHandeler = null;//按键钩子
        private KeyboardHook k_hook = new KeyboardHook();
        double mp_x = System.Windows.Forms.Control.MousePosition.X;
        double mp_y = System.Windows.Forms.Control.MousePosition.Y;

        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                string[] ss = File.ReadAllText("life").Split(' ');
                h = int.Parse(ss[0]);
                m = int.Parse(ss[1]);
                s = int.Parse(ss[2]);
                setTime();
            }
            catch (Exception)
            {

            }
            timer.Tick += new EventHandler(Start);
            timer.Interval = TimeSpan.FromSeconds(1);
        }
        
        //点击“开始/暂停”按钮
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (isStart)
            {
                stopCounter();
                toSave();
            }
            else
            {
                startCounter();
            }
        }

        void stopCounter()
        {
            stopListen();
            isStart = false;
            btn.Content = "倒计时开始";
            hours.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            minute.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            second.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            timer.Stop();
        }

        void startCounter()
        {
            startListen();
            isStart = true;
            btn.Content = "倒计时暂停";
            hours.Foreground = new SolidColorBrush(Color.FromRgb(150, 0, 0));
            minute.Foreground = new SolidColorBrush(Color.FromRgb(150, 0, 0));
            second.Foreground = new SolidColorBrush(Color.FromRgb(150, 0, 0));
            timer.Start();
        }

        //设置时间文本
        void setTime()
        {
            hours.Text = string.Concat(h);
            minute.Text = string.Concat(m);
            second.Text = string.Concat(s);
        }

        //计时器主程序
        void Start(object sender, EventArgs e)
        {
            if (h < 0)
            {
                stopCounter();
                System.Windows.MessageBox.Show("恭喜你，你现在已经是带师了！");
                return;
            }
            if ((bool)lazy_mode_cb.IsChecked)
            {
                if (lazy_time == 0)
                {
                    stopCounter();
                    System.Windows.Forms.MessageBox.Show("偷懒中，时间暂停！", "提示",MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
                    lazy_time = LAZYTIME;
                    return;
                }
                mousePos();
                lazy_time -= 1;
            }
            //setTime();
            Console.WriteLine(lazy_time);
            s -= 1;
            if (s < 0)
            {
                s = 59;
                m -= 1;
                if (m < 0)
                {
                    m = 59;
                    h -= 1;
                }
            }
            if (saveTime <= 0)
            {
                saveTime = SAVETIME;
                toSave();
            }
            else
            {
                saveTime--;
            }
            setTime();

        }

        void toSave()
        {
            File.WriteAllText("life", string.Concat(h) + " " + string.Concat(m) + " " + string.Concat(s));
            Console.WriteLine("saved！");
        }

        void mousePos()
        {
            double nmp_x = System.Windows.Forms.Control.MousePosition.X;
            double nmp_y = System.Windows.Forms.Control.MousePosition.Y;
            if(nmp_x != mp_x || nmp_y != mp_y)
            {
                lazy_time = LAZYTIME;
            }
            mp_x = nmp_x;
            mp_y = nmp_y;
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            lazy_time = LAZYTIME;
        }

        //lazy时间改变
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string text = lazy_tb.Text.Trim();
            if (text != "")
            {
                lazy_tb.Text = Regex.Replace(text, @"\D+", "");
            }
            else
            {
                lazy_tb.Text = "60";
            }
            LAZYTIME = int.Parse(lazy_tb.Text.Trim());
            lazy_time = LAZYTIME;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            toSave();
        }

        public void startListen()
        {
            myKeyEventHandeler = new KeyEventHandler(hook_KeyDown);
            k_hook.KeyDownEvent += myKeyEventHandeler;//钩住键按下
            k_hook.Start();//安装键盘钩子
        }

        public void stopListen()
        {
            if (myKeyEventHandeler != null)
            {
                k_hook.KeyDownEvent -= myKeyEventHandeler;//取消按键事件
                myKeyEventHandeler = null;
                k_hook.Stop();//关闭键盘钩子
            }
        }
    }

    class KeyboardHook
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        static int hKeyboardHook = 0; //声明键盘钩子处理的初始值
        //值在Microsoft SDK的Winuser.h里查询
        public const int WH_KEYBOARD_LL = 13;   //线程键盘钩子监听鼠标消息设为2，全局键盘监听鼠标消息设为13
        HookProc KeyboardHookProcedure; //声明KeyboardHookProcedure作为HookProc类型
        //键盘结构
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;  //定一个虚拟键码。该代码必须有一个价值的范围1至254
            public int scanCode; // 指定的硬件扫描码的关键
            public int flags;  // 键标志
            public int time; // 指定的时间戳记的这个讯息
            public int dwExtraInfo; // 指定额外信息相关的信息
        }
        //使用此功能，安装了一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


        //调用此函数卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);


        //使用此功能，通过信息钩子继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        // 取得当前线程编号（线程钩子需要用到）
        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        //使用WINDOWS API函数代替获取当前实例的函数,防止钩子失效
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void Start()
        {
            // 安装键盘钩子
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName), 0);
                //hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                //************************************
                //键盘线程钩子
                SetWindowsHookEx(13, KeyboardHookProcedure, IntPtr.Zero, GetCurrentThreadId());//指定要监听的线程idGetCurrentThreadId(),
                //键盘全局钩子,需要引用空间(using System.Reflection;)
                //SetWindowsHookEx( 13,MouseHookProcedure,Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),0);
                //
                //关于SetWindowsHookEx (int idHook, HookProc lpfn, IntPtr hInstance, int threadId)函数将钩子加入到钩子链表中，说明一下四个参数：
                //idHook 钩子类型，即确定钩子监听何种消息，上面的代码中设为2，即监听键盘消息并且是线程钩子，如果是全局钩子监听键盘消息应设为13，
                //线程钩子监听鼠标消息设为7，全局钩子监听鼠标消息设为14。lpfn 钩子子程的地址指针。如果dwThreadId参数为0 或是一个由别的进程创建的
                //线程的标识，lpfn必须指向DLL中的钩子子程。 除此以外，lpfn可以指向当前进程的一段钩子子程代码。钩子函数的入口地址，当钩子钩到任何
                //消息后便调用这个函数。hInstance应用程序实例的句柄。标识包含lpfn所指的子程的DLL。如果threadId 标识当前进程创建的一个线程，而且子
                //程代码位于当前进程，hInstance必须为NULL。可以很简单的设定其为本应用程序的实例句柄。threaded 与安装的钩子子程相关联的线程的标识符
                //如果为0，钩子子程与所有的线程关联，即为全局钩子
                //************************************
                //如果SetWindowsHookEx失败
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }
        public void Stop()
        {
            bool retKeyboard = true;


            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }

            if (!(retKeyboard)) throw new Exception("卸载钩子失败！");
        }
        //ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, //[in] 指定虚拟关键代码进行翻译。
                                         int uScanCode, // [in] 指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压）
                                         byte[] lpbKeyState, // [in] 指针，以256字节数组，包含当前键盘的状态。每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。在低比特，如果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。在切换状态的NUM个锁和滚动锁定键被忽略。
                                         byte[] lpwTransKey, // [out] 指针的缓冲区收到翻译字符或字符。
                                         int fuState); // [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise.

        //获取按键的状态
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private const int WM_KEYDOWN = 0x100;//KEYDOWN
        private const int WM_KEYUP = 0x101;//KEYUP
        private const int WM_SYSKEYDOWN = 0x104;//SYSKEYDOWN
        private const int WM_SYSKEYUP = 0x105;//SYSKEYUP

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 侦听键盘事件
            if ((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                // raise KeyDown
                if (KeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDownEvent(this, e);
                }

                //键盘按下
                if (KeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPressEvent(this, e);
                    }
                }

                // 键盘抬起
                if (KeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUpEvent(this, e);
                }

            }
            //如果返回1，则结束消息，这个消息到此为止，不再传递。
            //如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
        ~KeyboardHook()
        {
            Stop();
        }
    }

}
