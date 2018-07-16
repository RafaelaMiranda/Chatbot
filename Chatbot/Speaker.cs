using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis; //namespace

namespace Chatbot
{
    public class Speaker
    {
        private static SpeechSynthesizer sp = new SpeechSynthesizer();
        public static void Speak(string text)
        {
            // caso esteja falando
            if (sp.State == SynthesizerState.Speaking)
            {
                sp.SpeakAsyncCancelAll();
            }
            sp.SpeakAsync(text);
        }
    }
}
