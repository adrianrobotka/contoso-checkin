using System.Windows;
using System.Windows.Controls;

namespace ContosoCheckIn.UserControls
{
    /// <summary>
    /// Interaction logic for SingleParticipantUserControl.xaml
    /// </summary>
    public partial class SingleParticipantUserControl : UserControl
    {
        private ParticipantIdentifyResult Candidate = null;

        public SingleParticipantUserControl(ParticipantIdentifyResult person)
        {
            Candidate = person;
            InitializeComponent();
            SmartBorder.MinWidth = 300;

            FillUserData();
            SoundNotification.PlaySuccessSound();
        }

        private void FillUserData()
        {
            Name.Text = Candidate.participant.firstName + " " + Candidate.participant.lastName;
            Company.Text = Candidate.participant.company;
            WorkTitle.Text = Candidate.participant.workTitle;
            Birth.Text = Candidate.participant.birth;
            Email.Text = Candidate.participant.email;
            Group.Text = Candidate.participant.groupName;
            ConfidencePercentage.Text = (int)(Candidate.confidence * 100) + "%";

            FillImages();
        }

        private void FillImages()
        {
            foreach (FaceImage face in Candidate.participant.faces)
            {
                Image image = new Image();
                image.Source = face.Source();
                image.Margin = new Thickness(5, 0, 5, 0);
                image.Height = 200;
                UserImages.Children.Add(image);
            }
        }
    }
}
