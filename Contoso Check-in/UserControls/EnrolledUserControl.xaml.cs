using System;
using System.Windows;
using System.Windows.Controls;

namespace ContosoCheckIn.UserControls
{
    /// <summary>
    /// Interaction logic for EnrolledUserControl.xaml
    /// </summary>
    public partial class EnrolledUserControl : UserControl
    {
        public EnrolledUserControl(ParticipantIdentifyResult p)
        {
            InitializeComponent();
            try
            {
                UserName.Text = p.participant.firstName;
                FaceImage face = p.participant.faces[0];
                UserPicture.Source = face.Source();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}