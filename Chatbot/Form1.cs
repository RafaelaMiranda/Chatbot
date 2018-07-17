using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Text;
using Microsoft.Speech.Recognition; // adicionar namespace
using Newtonsoft.Json.Linq;

namespace Chatbot
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine; // engine de Reconhecimento

        public Form1()
        {
            InitializeComponent();
            LoadSpeech();
            textBot.Enabled = false;
            textUsuario.Enabled = false;
        }

        private void LoadSpeech()
        {
            try
            {
                engine = new SpeechRecognitionEngine(); // instância
                engine.SetInputToDefaultAudioDevice(); // microfone

                string[] words = { "estou feliz", "estou triste" }; // futuramente chamar variavél do watson assistant

                // carregar a gramática
                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);

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
            pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_02.png");
            textUsuario.Text = e.Result.Text;
            Speaker.Speak("Oi, sou a assistente da Fatec Americana"); // chamar futuramente resposta do watson assistant
            // escrever audio na caixa de texto textBot
            ToneAnalyzer();
        }

        private void ToneAnalyzer()
        {
            string baseURL;
            string username;
            string password;

            // Credenciais do Tone Analyzer
            baseURL = "https://gateway.watsonplatform.net/tone-analyzer/api/v3/tone?version=2016-05-19&sentences=false";
            username = "8c9fc121-66a2-4ba3-b718-f4f30dc5a620";
            password = "Yeccgc0LtMUx";

            // Obtem os dados a serem analisados ​
            string postData = "{\"text\": \"" + textUsuario.Text + "\"}";

            // Cria a solicitação da web
            var request = (HttpWebRequest)WebRequest.Create(baseURL);

            // Configura as credenciais do BlueMix
            string auth = string.Format("{0}:{1}", username, password);
            string auth64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
            string credentials = string.Format("{0} {1}", "Basic", auth64);

            // Define os parâmetros da solicitação da web
            request.Headers[HttpRequestHeader.Authorization] = credentials;
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Define a propriedade ContentLength do WebRequest
            request.ContentLength = byteArray.Length;

            // Obtem o fluxo das solicitações
            Stream dataStream = request.GetRequestStream();
            // Grava os dados no fluxo de solicitação
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Obtem a resposta
            WebResponse response = request.GetResponse();
            // Exibe o status
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Obtem o fluxo contendo o conteúdo retornado pelo serviço
            dataStream = response.GetResponseStream();
            // Abre o fluxo usando um StreamReader para facilitar o acesso
            StreamReader reader = new StreamReader(dataStream);
            // Lê e formata o conteúdo
            string responseFromServer = reader.ReadToEnd();
            responseFromServer = ToneAnalyzerTools.JsonPrettify(responseFromServer);
            // Captura a emoção com score mais alto do Tone Analyzer
            JObject rss = JObject.Parse(responseFromServer);

            // Exibe o conteúdo
            Console.WriteLine(rss);
        }

    }
}
