using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp.Extensions;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Windows.Media;
using ContosoCheckIn.UserControls;
using System.ComponentModel;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Interop;

namespace ContosoCheckIn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EventHandler EventHandler;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public MainWindow(EventHandler eventHandler)
        {
            EventHandler = eventHandler;

            InitializeComponent();

            EventHandler.FrameProvider.ProcessingLiveFrame += FrameProvider_ProcessingLiveFrame;
            EventHandler.ProcessingLocalDetectionResult += EventHandler_ProcessingLocalDetectionResult;
            EventHandler.ProcessingRemoteDetectResult += EventHandler_ProcessingDetectResult;
            EventHandler.ProcessingIdentifyResult += EventHandler_ProcessingIdentifyResult;

            StartDetection();
        }

        /// <summary>
        /// Event of (live) frame providing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void EventHandler_ProcessingLocalDetectionResult(object sender, NewLocalDetectionEventArgs e)
        {
            // The callback may occur on a different thread, so we must use the
            // MainWindow.Dispatcher when manipulating the UI. 
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ClearMessageArea();

                if (e.Faces.Count() == 0)
                {
                    FeedbackImage.Source = null;
                    ShowCandidateScreen(new NoFaceUserControl());
                }
            }));
        }

        /// <summary>
        /// Event of the face detection result providing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandler_ProcessingDetectResult(object sender, NewRemoteDetectionEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.TimedOut)
                {
                    MessageArea.Text = "Server timed out under face detection.";
                    return;
                }

                if (e.Exception != null)
                {
                    string message = e.Exception.Message;
                    var faceEx = e.Exception as FaceAPIException;
                    if (faceEx != null)
                    {
                        message = faceEx.ErrorMessage;
                    }

                    MessageArea.Text = $"Failed detection on frame {e.Frame.Metadata.Index} caused by server error.";
                    ShowCandidateScreen(new ServerErrorUserControl());
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

        /// <summary>
        /// Event of face identify result providing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandler_ProcessingIdentifyResult(object sender, NewIdentifyEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.TimedOut)
                {
                    MessageArea.Text = "Server timed out under face detection.";
                    return;
                }

                if (e.Exception != null)
                {
                    MessageArea.Text = $"Server error in identifing:" + e.Exception.Message;
                    ShowCandidateScreen(new ServerErrorUserControl());
                    return;
                }

                ParticipantIdentifyResult[] candidates = e.Faces;

                switch (candidates.Length)
                {
                    case 0: // No identify result provided
                        ShowCandidateScreen(new NoIdentityUserControl());
                        break;
                    case 1:
                        ShowCandidateScreen(new SingleParticipantUserControl(candidates[0]));
                        break;
                    default:
                        ShowCandidateScreen(new MoreCandidatesUserControl(candidates, EventHandler));
                        break;
                }
            }));
        }

        private void ClearMessageArea()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageArea.Text = "";
            }));
        }

        private void ShowCandidateScreen(UserControl control)
        {
            CandidatesStack.Children.Clear();

            if (control == null)
            {
                RightColumn.Background = null;
                return;
            }

            control.MinWidth = (column1.ActualWidth) * Properties.Settings.Default.MinWidthOnUserControls;
            RightColumn.Background = control.Background;
            CandidatesStack.Children.Add(control);
        }

        private void StartDetection()
        {
            ClearMessageArea();

            EventHandler.Start();
        }

        private void StopDetection()
        {
            EventHandler.Stop();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
