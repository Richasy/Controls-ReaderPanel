using Richasy.Controls.Reader.Enums;
using System;
using Windows.Media.Core;

namespace Richasy.Controls.Reader.Models
{
    public class SpeechCueEventArgs : EventArgs
    {
        public SpeechCue SpeechCue { get; set; }
        public SpeechCueType Type { get; set; }
        public SpeechCueEventArgs()
        {

        }
        internal SpeechCueEventArgs(SpeechCue cue,SpeechCueType type)
        {
            SpeechCue = cue;
            Type = type;
        }
    }
}
