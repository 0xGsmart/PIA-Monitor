/*
    PIA-Monitor is a program to kill and restart an application based on the status of PIA.
    Copyright (C) 2015 Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace PIA_Monitor
{
    class Program
    {
        private static NotifyIcon notifyIcon = new NotifyIcon();

        static void Main(string[] args)
        {
            notifyIcon.Visible = true;
            notifyIcon.Icon = SystemIcons.Asterisk;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.Text = "PIA Monitor";
            notifyIcon.BalloonTipTitle = "PIA Monitor";
            notifyIcon.BalloonTipText = "PIA Monitor";

            // run once
            T_Elapsed(null, null);

            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += T_Elapsed;
            t.Interval = 4010;
            t.Enabled = true;
            t.Start();

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                bool isUp = PIAUp();
                
                string appPath = @"PATH TO APPLICATION";
                string appName = appPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().Replace(".exe","");


                if (isUp)
                {
                    notifyIcon.Text = "PIA is connected.";

                    // if PIA is up, check if the program is running. If not, start it.
                    Process[] localByName = Process.GetProcessesByName(appName);
                    if (localByName.Count() == 0)
                    {
                        ShowBalloon("PIA Monitor", "PIA is UP, starting " + appName + "!");
                        Process.Start(appPath);
                        Console.WriteLine(DateTime.Now + " Started: " + appName);
                    }
                    notifyIcon.Icon = SystemIcons.Shield;
                }
                else
                {
                    notifyIcon.Text = "PIA is NOT connected.";
                    notifyIcon.Icon = SystemIcons.Error;
                    Process[] localByName = Process.GetProcessesByName(appName);

                    foreach (var item in localByName)
                    {
                        ShowBalloon("PIA Monitor", "PIA is DOWN, killing " + appName);
                        item.Kill();
                        Console.WriteLine(DateTime.Now + " Killed: " + item.ProcessName);
                    }
                }
            }
            catch (Exception ex)
            {
                notifyIcon.Icon = SystemIcons.Error;
                Console.WriteLine(ex.Message);
            }
        }

        private static void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private static bool PIAUp()
        {
            try
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

                foreach (NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (NI.Description.Contains("TAP-Windows"))
                    {
                        GatewayIPAddressInformationCollection addr = NI.GetIPProperties().GatewayAddresses;
                        if (addr.Count() > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowBalloon("Network", e);
            }

            return false;
        }

        private static void ShowBalloon(string Title, Exception e)
        {
            notifyIcon.BalloonTipTitle = Title;
            notifyIcon.BalloonTipText = e.Message;
            notifyIcon.ShowBalloonTip(2000);
        }

        private static void ShowBalloon(string Title, string Message)
        {
            notifyIcon.BalloonTipTitle = Title;
            notifyIcon.BalloonTipText = Message;
            notifyIcon.ShowBalloonTip(2000);
        }
    }
}
