using ContosoCheckIn.UserControls;
using Microsoft.ProjectOxford.Face.Contract;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ContosoCheckIn
{
    /// <summary>
    /// Interaction logic for ParticipantWindow.xaml
    /// </summary>
    public partial class ParticipantWindow : Window
    {
        private EventHandler EventHandler;

        public ParticipantWindow(EventHandler eventHandler)
        {
            EventHandler = eventHandler;
            InitializeComponent();

            // Set up a listener for when the client receives a new frame.
            EventHandler.FrameProvider.ProcessingLiveFrame += FrameProvider_ProcessingLiveFrame;
            EventHandler.ProcessingRemoteDetectResult += EventHandler_ProcessingDetectResult;
            EventHandler.ProcessingIdentifyResult += EventHandler_ProcessingIdentifyResult;

            EventHandler.ProcessingLocalDetectionResult += (o, e) =>
            {
                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.Faces.Count() == 0)
                    {
                        FeedbackImage.Source = null;
                        ShowCandidateScreen(new NoFaceUserControl());
                    }
                }));
            };
        }

        public void ShowOnMonitor(int monitor, Window window)
        {
            try
            {
                var screen = ScreenHandler.GetScreen(monitor);
                var currentScreen = ScreenHandler.GetCurrentScreen(this);
                window.WindowState = WindowState.Normal;
                window.Left = screen.WorkingArea.Left;
                window.Top = screen.WorkingArea.Top;
                window.Width = screen.WorkingArea.Width;
                window.Height = screen.WorkingArea.Height;
                window.Loaded += Window_Loaded;
            }
            catch (Exception)
            {

            }
        }

        /*You can use this event for all the Windows*/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var senderWindow = sender as Window;
            senderWindow.WindowState = WindowState.Maximized;
        }

        private void FrameProvider_ProcessingLiveFrame(object sender, NewFrameEventArgs e)
        {
            // The callback may occur on a different thread, so we must use the
            // MainWindow.Dispatcher when manipulating the UI. 
            Dispatcher.BeginInvoke((Action)(() =>
            {
                // Display the image in the left pane.
                LiveImage.Source = e.Frame.Image.ToBitmapSource();
            }));
        }

        private void EventHandler_ProcessingDetectResult(object sender, NewRemoteDetectionEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.TimedOut || e.Exception != null)
                {
                    ClearPersonView();
                    return;
                }

                FeedbackImage.Source = Visualization.DrawFaces(e.Frame.Image.ToBitmapSource(), e.Faces);

                if (e.Faces == null || e.Faces.Length == 0)
                {
                    ShowCandidateScreen(new NoFaceUserControl());
                    return;
                }

                int biggestFaceIndex = Util.GetDominantFaceIndex(e.Faces);

                if (biggestFaceIndex == -1)
                {
                    ShowCandidateScreen(new SimilarFaceAreaUserControl());
                    return;
                }
            }));
        }

        private void EventHandler_ProcessingIdentifyResult(object sender, NewIdentifyEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ParticipantIdentifyResult[] candidates = e.Faces;

                switch (candidates.Length)
                {
                    case 0: // No identify result provided
                        ShowCandidateScreen(new NoIdentityUserControl());
                        break;
                    case 1: // 1 found
                        ShowCandidateScreen(new EnrolledUserControl(candidates[0]));
                        break;
                    default: // More than 1 found
                        ShowCandidateScreen(new ProblemUserControl());
                        break;
                }
            }));
        }

        private void ClearPersonView()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                StackPanel.Children.Clear();
                DataColumn.Background = null;
            }));
        }

        private void ShowCandidateScreen(UserControl control)
        {
            if (control == null)
            {
                StackPanel.Children.Clear();
                DataColumn.Background = null;
                return;
            }

            control.MinWidth = (column1.ActualWidth) * Properties.Settings.Default.MinWidthOnUserControls;
            StackPanel.Children.Clear();
            DataColumn.Background = control.Background;
            StackPanel.Children.Add(control);
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if(Properties.Settings.Default.TwoMonitorMode)
                Application.Current.Shutdown();
        }
    }
}
