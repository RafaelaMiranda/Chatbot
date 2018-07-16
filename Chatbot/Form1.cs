using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Speech.Recognition; // adicionar namespace

namespace Chatbot
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine; // engine de Reconhecimento

        public Form1()
        {
            InitializeComponent();
            textBot.Enabled = false;
            textUsuario.Enabled = false;
        }

        private void LoadSpeech()
        {
            try
            {
                engine = new SpeechRecognitionEngine(); // instância
                engine.SetInputToDefaultAudioDevice(); // microfone

                string[] words = { "olá", "boa noite" , "Rafaela"}; // futuramente chamar variavél do watson assistant

                // carregar a gramática
                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(audioLevel);

                engine.RecognizeAsync(RecognizeMode.Multiple); // iniciar o reconhecimento

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu no LoadSpeech(): " + ex.Message);
            }
        }

        // metódo que é chamado quando algo é reconhecido
        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Eva\eve_eyes_02.png");
            textUsuario.Text = e.Result.Text;
            Speaker.Speak("Como está Rafaela?"); // chamar futuramente resposta do watson assistant
            // escrever audio na caixa de texto textBot
        }

        private void audioLevel(object s, AudioLevelUpdatedEventArgs e)
        {
            this.progressBar1.Maximum = 100;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Value = e.AudioLevel;
        }

        private void btnMicrofone_Click(object sender, EventArgs e)
        {
            LoadSpeech();
        }
    }
}
