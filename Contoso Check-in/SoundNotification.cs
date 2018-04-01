using System;
using System.Media;

namespace ContosoCheckIn
{
    class SoundNotification
    {
        private static SoundPlayer successAudio = new SoundPlayer(Properties.Resources.Success);
        private static SoundPlayer warningAudio = new SoundPlayer(Properties.Resources.Fail);

        public static void PlaySuccessSound()
        {
            successAudio.Play();
        }
        public static void PlayWarningSound()
        {
            warningAudio.Play();
        }
    }
}