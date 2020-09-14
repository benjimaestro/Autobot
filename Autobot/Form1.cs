using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Autobot
{
    public partial class Form1 : Form
    {
        static Int32 timeLeft = 2;
        static bool botEnabled = false;
        static bool settingsOpen = false;
        static string message;
        static string message2;
        static string message3;

        //This stuff is important, I think.
        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();
        public Form1()

        {
            InitializeComponent();

            timer.Interval = 2000;
            timer.Enabled = true;
            timer.Start();

            timer.Tick += Timer_Tick;

            btnStop.Enabled = false;
            btnStart.Enabled = true;

            txtMessageSettings.Text = Properties.Settings.Default.SavedText;
            message = Properties.Settings.Default.SavedText;

            textBox1.Text = Properties.Settings.Default.SavedText2;
            message2 = Properties.Settings.Default.SavedText2;

            textBox2.Text = Properties.Settings.Default.SavedText3;
            message3 = Properties.Settings.Default.SavedText3;

            this.Width = 800;
            this.Height = 560;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStop.ForeColor = System.Drawing.Color.Black;
            btnStart.ForeColor = System.Drawing.Color.Black;
        }

        private void BtnStart_Click(object sender, EventArgs e) //Runs when start is pressed, allowing the loop to proceed
        {
            botEnabled = true;
            btnStop.Enabled = true;
            btnStop.ForeColor = System.Drawing.Color.Black;
            btnStart.Enabled = false;
            btnStart.ForeColor = System.Drawing.Color.Black;
        }

        private void Timer_Tick(object sender, EventArgs e)//Runs on every timer tick event
        {
            timeLeft--;

            if (timeLeft <= 0)
            {
                timer.Stop();
                timer.Start();
                timeLeft = 2;
                if (botEnabled == true)
                {
                    foreach (HtmlElement element in webBrowser1.Document.GetElementsByTagName("iframe"))
                    {
                        if (element.GetAttribute("height") == "78" && element.GetAttribute("width") == "304")
                        {
                            FlashWindow.Flash(this, 2);
                        }
                    }
                    string formattedMessage = message.Replace("'", "\\'");//Formats saved message to prevent it interfering with the JS
                    string formattedMessage2 = message2.Replace("'", "\\'");//Formats saved message to prevent it interfering with the JS
                    string formattedMessage3 = message3.Replace("'", "\\'");//Formats saved message to prevent it interfering with the JS

                    //jsPostMessage is the JS code that is injected into the webview. The code is used to automate Omegle.
                    //window.alert = function() {} supresses the alert window when attempting to exit a conversation
                    //The rest of it will automatically fill text and press buttons depending on what's on screen currently
                    string jsPostMessage = @"window.alert = function() {}
                                            try
                                            {
                                                if (document.getElementsByClassName('statuslog')[0].outerText.startsWith('You'))
                                                {
                                                    document.getElementsByClassName('chatmsg')[0].value = '" + formattedMessage + @"';
                                                    document.getElementsByClassName('sendbtn')[0].click();
                                                    document.getElementsByClassName('chatmsg')[0].value = '" + formattedMessage2 + @"';
                                                    document.getElementsByClassName('sendbtn')[0].click();
                                                    document.getElementsByClassName('chatmsg')[0].value = '" + formattedMessage3 + @"';
                                                    document.getElementsByClassName('sendbtn')[0].click();

                                                    if (document.getElementsByClassName('disconnectbtn')[0].outerText.startsWith('Stop'))
                                                    {
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                    }

                                                    if (document.getElementsByClassName('disconnectbtn')[0].outerText.startsWith('Really'))
                                                    {
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                    }
                                                    if (document.getElementsByClassName('disconnectbtn')[0].outerText.startsWith('New'))
                                                    {
                                                        document.getElementsByClassName('disconnectbtn')[0].click();
                                                    }
                                                }
                                            }
                                            catch(error)
                                            {
                                                try
                                                {
                                                document.getElementById('textbtn').click();
                                                }
                                                catch(error)
                                                {
                                                }
                                            }";
                    webBrowser1.Invoke(new Action(() => webBrowser1.Document.InvokeScript("eval", new object[] { jsPostMessage })));//Runs the JS code in webview
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e) //Runs when the Stop button is pressed, shutting down the script loop and navigating back to the main page
        {
            botEnabled = false;
            btnStop.Enabled = false;
            btnStop.ForeColor = System.Drawing.Color.Black;
            btnStart.Enabled = true;
            btnStart.ForeColor = System.Drawing.Color.Black;
            webBrowser1.Navigate("https://omegle.com/");
        }

        private void BtnSettings_Click(object sender, EventArgs e) //Extends the window, revealing the settings because having additional child windows is annoying
        {
            if (settingsOpen == false)
            {
                this.Height = 805;
                settingsOpen = true;
            }
            else
            {
                this.Height = 560;
                settingsOpen = false;
            }
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e) //Saves text you've entered
        {
            message = txtMessageSettings.Text;
            Properties.Settings.Default.SavedText = message;
            Properties.Settings.Default.Save();

            message2 = textBox1.Text;
            Properties.Settings.Default.SavedText2 = message2;
            Properties.Settings.Default.Save();

            message3 = textBox2.Text;
            Properties.Settings.Default.SavedText3 = message3;
            Properties.Settings.Default.Save();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e) //When checked, ScriptErrorsSuppressed is set to false so IE script error windows may appear at captchas for some unknown reason.
        {
            if (checkBox1.Checked == true)
            {
                webBrowser1.ScriptErrorsSuppressed = false;
            }
            else
            {
                webBrowser1.ScriptErrorsSuppressed = true;
            }
        }

        private void webBrowser1_NewWindow_1(object sender, CancelEventArgs e)
        {
            webBrowser1.Navigate(webBrowser1.StatusText);
            e.Cancel = true;
        }
    }
    public static class FlashWindow //Class to flash the taskbar icon for CAPTCHA alerts
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;


        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <returns></returns>
        public static bool Flash(System.Windows.Forms.Form form)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(System.Windows.Forms.Form form, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL, count, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// Start Flashing the specified Window (form)
        /// </summary>
        /// <param name="form">The Form (Window) to Flash.</param>
        /// <returns></returns>
        public static bool Start(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// Stop Flashing the specified Window (form)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static bool Stop(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_STOP, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return System.Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
