using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVE.SpeechSystem
{
    internal sealed class SpeechParameters
    {
        public readonly string VoiceName;

        public readonly int IntonationStart;
        public readonly int IntonationEnd;
        public readonly float SpeedPercentage;

        public SpeechParameters(string voiceName)
        {
            VoiceName = voiceName;
            IntonationStart = 170;
            IntonationEnd = 170;
            SpeedPercentage = 1.0f;
        }
    }
}
