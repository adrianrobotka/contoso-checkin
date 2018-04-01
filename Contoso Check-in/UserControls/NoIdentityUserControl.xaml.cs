using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ContosoCheckIn.UserControls
{
    /// <summary>
    /// Interaction logic for NoIdentityUserControl.xaml
    /// </summary>
    public partial class NoIdentityUserControl : UserControl
    {
        public NoIdentityUserControl()
        {
            InitializeComponent();
            SoundNotification.PlayWarningSound();
        }
    }
}
