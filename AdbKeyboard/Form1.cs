using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace AdbKeyboard
{
    public partial class AdbKeyboard : Form
    {

        public AdbKeyboard()
        {
            InitializeComponent();
            LoadMacros();
        }

        private Process process;

        private string history = string.Empty;
        private string textBuffer = string.Empty;
        private string keyEventBuffer = string.Empty;

        private Timer timer = new Timer();
        private int milliSeconds = 0;


        private void Enable()
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            pnlMacros.Enabled = true;
            lblMainHeader.Text = "ADB Keyboard Running";
            lblMainHeader.Focus();
        }

        private void Disable()
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            pnlMacros.Enabled = false;
            lblMainHeader.Text = "";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process = new Process {StartInfo = startInfo};
            process.Start();

            process.StandardInput.WriteLine("adb shell");

            Enable();

            timer.Tick += timer_Tick;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            process.Close();
            process = null;
            Disable();
            timer.Tick -= timer_Tick;
        }

        private void AdbKeyboard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (pnlMacros.ContainsFocus) return;

            if (process != null)
            {

                ResetTimer();

                switch (e.KeyChar)
                {
                    case (char) Keys.Space:
                        flushTextBuffer();
                        keyEventBuffer += "62 "; // spacebar
                        break;
                    case (char) Keys.Back:
                        flushTextBuffer();
                        keyEventBuffer += "67 "; // backspace
                        break;

                    case (char)Keys.Escape:
                        flushTextBuffer();
                        keyEventBuffer += "4 "; // back button
                        break;

                    case (char)Keys.Enter:
                        flushTextBuffer();
                        keyEventBuffer += "66 "; // enter
                        break;

                    default:
                        flushKeyBuffer();
                        textBuffer += e.KeyChar;
                        break;
                }
            }
        }
        
        private void AdbKeyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (pnlMacros.ContainsFocus) return;

            bool keyEventWasConsumed = true;
            string newBuffer = string.Empty;

            switch (e.KeyCode)
            {
                case Keys.F1:
                    newBuffer += "3 "; // home button
                    break;

                    case Keys.F2:
                    newBuffer += "26 "; // power button
                    break;

                    case Keys.End:
                    newBuffer += "123 "; // end
                    break;

                    case Keys.Home:
                    newBuffer += "122 "; // end
                    break;

                    case Keys.Delete:
                    newBuffer += "112 "; // delete
                    break;

                default:
                    keyEventWasConsumed = false;
                    break;
            }

            if (keyEventWasConsumed)
            {
                ResetTimer();
                flushTextBuffer();
                keyEventBuffer += newBuffer;
            }
               
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == Keys.Tab && !pnlMacros.ContainsFocus)
            {
                ResetTimer();
                flushTextBuffer();
                keyEventBuffer += "187 ";
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);   
            }
        }

        private void ResetTimer()
        {
            if (!timer.Enabled) timer.Start();
            milliSeconds = 0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            milliSeconds++;
            if (milliSeconds > 2 || keyEventBuffer.Length > 5 || textBuffer.Length > 10)
            {
                flushTextBuffer();
                flushKeyBuffer();
            }
        }

        private void flushTextBuffer()
        {
            if (textBuffer.Length > 0)
            {
                timer.Stop();
                milliSeconds = 0;

                process.StandardInput.WriteLine(@"input text " + textBuffer);
                history = textBuffer + "\r\n" + history;

                textBuffer = string.Empty;
            }
        }

        private void flushKeyBuffer()
        {
            if (keyEventBuffer.Length > 0)
            {
                timer.Stop();
                milliSeconds = 0;

                if (keyEventBuffer.Length > 0)
                {
                    process.StandardInput.WriteLine(@"input keyevent " + keyEventBuffer);
                    history = keyEventBuffer + "\r\n" + history;
                }

                keyEventBuffer = string.Empty;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMacro(tbMacro1);
        }

        private void btnSendMacro2_Click(object sender, EventArgs e)
        {
            SendMacro(tbMacro2);
        }

        private void btnSendMacro3_Click(object sender, EventArgs e)
        {
            SendMacro(tbMacro3);
        }

        private void btnSendMacro4_Click(object sender, EventArgs e)
        {
            SendMacro(tbMacro4);
        }

        private void btnSendMacro5_Click(object sender, EventArgs e)
        {
            SendMacro(tbMacro5);
        }

        private void SendMacro(TextBox textBox)
        {
            if (process != null)
            {
                if (textBox.Text.Length > 0)
                {
                    string inputString = textBox.Text;

                    string[] strings = inputString.Split('|');

                    if (strings.Count() > 1) // contains returns
                    {
                        foreach (var s in strings)
                        {
                            if (s.Length > 0)
                            {
                                process.StandardInput.WriteLine(@"input text " + s);
                            }
                            
                            process.StandardInput.WriteLine(@"input keyevent 66");
                        }
                    }
                    else // send through as one string
                    {
                        process.StandardInput.WriteLine(@"input text " + inputString);
                    }
                }
            }

            Properties.Settings.Default[textBox.Name] = textBox.Text;
            Properties.Settings.Default.Save(); // Saves settings in application configuration file

            lblMainHeader.Focus();
        }

        private void LoadMacros()
        {
            foreach(Control control in pnlMacros.Controls)
            {
                if (control is TextBox)
                {
                    TextBox textBox = (TextBox) control;
                    textBox.Text = Properties.Settings.Default[textBox.Name].ToString();
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/deano2390");
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This program is a tool for making repetitive text entry a little less painful when testing Android apps. Run this program and click the start button, while the app is focused it will forward your keyboard input to any Android device connected via ADB. Use the macro textboxes to save your commonly used text snippets.", "Help");
        }

        private void lnkMAcroHints_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Use | characters to automate enter key presses. So for a typical login screen we might use the macro 'my@email.com|myPassword|' to simulate entering the credentials and pressing the enter key in between.", "Help");
        }
    }
}
