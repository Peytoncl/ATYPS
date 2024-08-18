using System.IO;
using System.Resources;
using System.Media;
using System.Diagnostics;
using NAudio.Wave;
using System;
using System.Threading;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace ATYPS
{
    class Program
    {
        private IKeyboardMouseEvents m_GlobalHook;

        static Program program;

        public int startTime = 45;

        public int time;

        public string[] processList = { "RobloxPlayerBeta", "FL64" };

        public string reason;

        [STAThread]
        static void Main(string[] args)
        {
            NotifyIcon tray = new NotifyIcon();
            tray.Visible = true;
            tray.Icon = ATYPS.Properties.Resources.icon;
            tray.Text = "ATYPS";

            tray.ContextMenuStrip = new ContextMenuStrip();
            tray.ContextMenuStrip.Items.Add("Stop Working", null).Click += OnExitPressed;

            new Thread(() => Console.ReadLine()).Start();

            program = new Program();

            program.Subscribe();

            Application.Run(new ApplicationContext());

            program.Unsubscribe();

            Application.Exit();
        }

        public static void OnExitPressed(object sender, EventArgs ev)
        {
            SendWebhook(Environment.UserName + " has stopped working. What a bum");

            Process.GetCurrentProcess().Kill();
        }

        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            StartTimer();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;

            AppDomain.CurrentDomain.ProcessExit += ApplicationExit;
        }

        public void CheckPrograms()
        {
            while(time > 0)
            {
                KillProcess(processList);

                Thread.Sleep(5000);
            }
        }

        public void KillProcess(string[] processes)
        {
            foreach (string processName in processes)
            {
                Process[] robloxProcesses = Process.GetProcessesByName(processName);
                if (robloxProcesses.Length > 0)
                {
                    foreach (Process process in robloxProcesses)
                    {
                        reason = "having " + process.ProcessName + ".exe open";

                        time = 0;

                        process.Kill();
                    }
                }
            }
        }

        public void StartTimer()
        {
            time = startTime;
            reason = "";

            Thread thread = new Thread(Timer);
            thread.Start();

            Thread programThread = new Thread(CheckPrograms);
            programThread.Start();
        }

        public void Timer()
        {
            while(time > 0)
            {
                Thread.Sleep(1000);
                time--;
            }

            if (reason == "") reason = "staring at the screen for too long";

            SendWebhook(Environment.UserName + " got caught " + reason);

            PlayAudio();
        }

        public void PlayAudio()
        {
            var reader = new Mp3FileReader(@"jingle.mp3");
            var waveOut = new WaveOutEvent();

            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                if (time == startTime)
                {
                    waveOut.Stop();

                    continue;
                }

                Thread.Sleep(100);
            }

            StartTimer();
        }


        public void ApplicationExit(object sender, EventArgs e)
        {
            Unsubscribe();
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            time = startTime;
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            time = startTime;
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            m_GlobalHook.Dispose();
        }

        static void SendWebhook(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1274697482151919638/yk12JwNRjk9OYfl4rqk601Jz9rYerSXdaKNKeSFvCT1Oi6WY_VI9jzjWDVAZh9zt5fRc";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }
    }
}
