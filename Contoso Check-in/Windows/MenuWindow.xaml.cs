using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EDSDKWrapper.Framework.Managers;
using EDSDKWrapper.Framework.Objects;
using EDSDKWrapper.Framework.Exceptions;
using System.Reflection;

namespace ContosoCheckIn.Windows
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : System.Windows.Window
    {
        public static Camera Canon
        {
            get;
            private set;
        }

        public static int WebcamNum = -1;
        public static EventHandler handler;

        public MenuWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.CanonSupport)
            {
                try
                {
                    Canon = new FrameworkManager().Cameras.First();
                }
                catch { }
            }
        }

        public static int GetWebcamNum()
        {
            if (WebcamNum == -1)
            {
                int num = 0;

                while (num < 100)
                {
                    using (var vc = VideoCapture.FromCamera(num))
                    {
                        if (vc.IsOpened())
                            ++num;
                        else
                            break;
                    }
                }
                WebcamNum = num;
            }

            return WebcamNum;
        }

        private void CameraList_Loaded(object sender, RoutedEventArgs e)
        {
            int numWebcams = GetWebcamNum();

            int numCameras = numWebcams;

            if (Canon != null)
                numCameras++;

            if (numCameras == 0)
            {
                FeedbackMessage.Text = "No cameras found!";
                return;
            }

            List<string> listElements = Enumerable.Range(0, numWebcams).Select(i => string.Format("Webcam {0}", i + 1)).ToList();

            if (Canon != null)
            {
                try
                {
                    listElements.Add(Canon.ProductName);
                }
                catch (Exception exc)
                {
                }
            }

            int selected = Properties.Settings.Default.CameraNum;
            if (selected >= listElements.Count())
                selected = 0;

            ComboBox comboBox = CameraList;
            comboBox.ItemsSource = listElements;
            comboBox.SelectedIndex = selected;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            FrameProvider provider = null;

            int selected = CameraList.SelectedIndex;
            if (selected < WebcamNum)
            {
                provider = new WebCamFrameProvider(selected);
                handler = new EventHandler(provider, 1);
            }
            else
            {
                provider = new CanonFrameProvider(Canon);
                handler = new EventHandler(provider, Properties.Settings.Default.CanonMultiplier);
            }


            MainWindow main = new MainWindow(handler);
            main.Show();
            Hide();

            Properties.Settings.Default.CameraNum = CameraList.SelectedIndex;
            Properties.Settings.Default.TwoMonitorMode = showparticipantwindow.SelectedIndex == 1;
            Properties.Settings.Default.LocalFaceDetection = uselocalface.SelectedIndex == 1;
            Properties.Settings.Default.Save();

            bool twoMonitor = Properties.Settings.Default.TwoMonitorMode;
            if (twoMonitor)
            {
                ParticipantWindow participantWindow = new ParticipantWindow(handler);
                int MonitorCount = System.Windows.Forms.Screen.AllScreens.Length;
                if (MonitorCount > 1)
                    participantWindow.ShowOnMonitor(MonitorCount - 1, participantWindow);
                participantWindow.Show();
            }
        }



        public static void CloseSecondWindow()
        {
            foreach (System.Windows.Window w in Application.Current.Windows)
            {
                if (w.Title.Contains("Participant"))
                {
                    w.Close();
                }
            }
        }

        private void showparticipantwindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> windowmodeitems = new List<string>();
            windowmodeitems.Add("Disable");
            windowmodeitems.Add("Enable");
            showparticipantwindow.ItemsSource = windowmodeitems;
            showparticipantwindow.SelectedIndex = Properties.Settings.Default.TwoMonitorMode ? 1 : 0;
        }

        private void uselocalface_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> items = new List<string>();
            items.Add("Disable");
            items.Add("Enable");
            uselocalface.ItemsSource = items;
            uselocalface.SelectedIndex = Properties.Settings.Default.LocalFaceDetection ? 1 : 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}