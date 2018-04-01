using ContosoCheckIn.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ContosoCheckIn
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool IsParticipantWindowEnabled = false;
        public SettingsWindow()
        {
            InitializeComponent();
            try
            {
                TxtApikey.Text = Properties.Settings.Default.ApiKey;
                TxtApiurl.Text = Properties.Settings.Default.SiteUrl;
                //TxtFrameanalysisinterval.Text = Properties.Settings.Default.LocalFaceDetectInterval.ToString();
                TxtMaximumcandidates.Text = Properties.Settings.Default.MaximumCandidates.ToString();
                TxtMinimalthreshold.Text = Properties.Settings.Default.MinimalThreshold.ToString();
                TxtMinimumfacedeltapercent.Text = Properties.Settings.Default.MinFaceDeltaPercent.ToString();
                TxtPersonanalysisbreak.Text = Properties.Settings.Default.FPS.ToString();
                UseLocalFaceDetector.IsChecked = Properties.Settings.Default.LocalFaceDetection;
                DontUseLocalFaceDetector.IsChecked = !Properties.Settings.Default.LocalFaceDetection;
                ShowParticipantWindow.IsChecked = Properties.Settings.Default.TwoMonitorMode;
                DontShowParticipantWindow.IsChecked = !Properties.Settings.Default.TwoMonitorMode;

                IsParticipantWindowEnabled = Properties.Settings.Default.TwoMonitorMode;
            }
            catch (Exception)
            {
                
            }
        }

        private void savebutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ApiKey = TxtApikey.Text;
                Properties.Settings.Default.SiteUrl = TxtApiurl.Text;
                Properties.Settings.Default.FPS = int.Parse(TxtPersonanalysisbreak.Text);
                Properties.Settings.Default.MaximumCandidates = int.Parse(TxtMaximumcandidates.Text);
                Properties.Settings.Default.MinimalThreshold = float.Parse(TxtMinimalthreshold.Text);
                Properties.Settings.Default.MinFaceDeltaPercent = float.Parse(TxtMinimumfacedeltapercent.Text);
                Properties.Settings.Default.LocalFaceDetection = UseLocalFaceDetector.IsChecked == true;
                Properties.Settings.Default.TwoMonitorMode = ShowParticipantWindow.IsChecked == true;

                Properties.Settings.Default.Save();

                if (IsParticipantWindowEnabled != Properties.Settings.Default.TwoMonitorMode)
                {
                    MenuWindow.CloseSecondWindow();
                }
                this.Close();
                //TODO: Apply changes -- restart application

            }
            catch (Exception)
            {
                
            }
        }
    }
}
