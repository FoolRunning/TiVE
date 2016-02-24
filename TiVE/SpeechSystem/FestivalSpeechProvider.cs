using System.IO;
using System.Reflection;
using Festival;
using ProdigalSoftware.TiVE.Core;

namespace ProdigalSoftware.TiVE.SpeechSystem
{
    internal sealed class FestivalSpeechProvider : ISpeechProvider
    {
        #region Implementation of ISpeechProvider
        public void Dispose()
        {
            FestivalInterop.CleanUp();
        }

        public void Initialize()
        {
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            FestivalInterop.Initialize(Path.Combine(currentDir, ResourceLoader.DataDirName, "festivalLib"));
        }

        public void SayText(string text, SpeechParameters parameters = null)
        {
            if (parameters != null)
            {
                if (!string.IsNullOrEmpty(parameters.VoiceName))
                    FestivalInterop.ExecuteCommand(string.Format("(voice_{0})", parameters.VoiceName));
                FestivalInterop.ExecuteCommand(string.Format("(set! duffint_params '((start {0}) (end {1})))", parameters.IntonationStart, parameters.IntonationEnd));
                FestivalInterop.ExecuteCommand("(Parameter.set 'Int_Method 'DuffInt)");
                FestivalInterop.ExecuteCommand("(Parameter.set 'Int_Target_Method Int_Targets_Default)");

                FestivalInterop.ExecuteCommand(string.Format("(Parameter.set 'Duration_Stretch {0})", parameters.SpeedPercentage));
            }
            FestivalInterop.SayText(text);
        }
        #endregion
    }
}
