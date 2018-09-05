using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Speech.Recognition;
using System.Web;
using System.Collections.Generic;

namespace Chatbot
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine; // engine de Reconhecimento
        string emotion = "";

        public Form1()
        {
            InitializeComponent();
            LoadSpeech();
            textBot.Hide();
            textUsuario.Hide();
        }
        #region speech
        private async void LoadSpeech()
        {
            try
            {
                engine = new SpeechRecognitionEngine(); // instância
                engine.SetInputToDefaultAudioDevice(); // microfone

                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
                var subscriptionKey = "4beff44499c8492a8359493e5b1d8bd9";

                // The request header contains your subscription key
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                var uri = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/467f5821-0494-4de0-bde7-70cb65aaa195/versions/0.1/export";
                var response = await client.GetAsync(uri);
                var strResponseContent = await response.Content.ReadAsStringAsync();
                // Display the JSON result from LUIS
                JObject rss = JObject.Parse(strResponseContent);
                string[] words = new string[18];
                for (int i = 0; i < 18; i++)
                {
                    words[i] = (string)rss["utterances"][i]["text"];
                    engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words[i]))));
                }

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Luis);
                engine.RecognizeAsync(RecognizeMode.Multiple); // iniciar o reconhecimento
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu no LoadSpeech(): " + ex.Message);
            }
        }

        #endregion
        #region translate
        async void Translate()
        {
            string host = "https://api.cognitive.microsofttranslator.com";
            string path = "/translate?api-version=3.0";
            string params_ = "&to=en";
            string uri = host + path + params_;
            string key = "7315970e7abf4e3ba12f900725ff9dd5";
            string text = textUsuario.Text;

            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);

                JToken rss = JToken.Parse(result);
                string rssTitle = (string)rss[0]["translations"][0]["text"];
                textBot.Text = rssTitle;
                ToneAnalyzer();
            }
        }
        #endregion
        #region toneAnalyzer
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
            string postData = "{\"text\": \"" + textBot.Text + "\"}";

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
            // Transforma o resultado vindo do servidor em JSON
            JObject rss = JObject.Parse(responseFromServer);

            if (emotion.Equals("Joy"))
            {
                pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_05.png");
            }
            else if (emotion.Equals("Sadness"))
            {
                pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_06.png");
            }
            else if (emotion.Equals("Anger"))
            {
                pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_08.png");
            }
            else
            {
                pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_02.png");
            }

        }
        #endregion
        #region luis
        private async void Luis(object s, SpeechRecognizedEventArgs e)
        {
            textUsuario.Text = e.Result.Text;

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
            var luisAppId = "467f5821-0494-4de0-bde7-70cb65aaa195";
            var subscriptionKey = "4beff44499c8492a8359493e5b1d8bd9";

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = textUsuario.Text;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";

            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            // Display the JSON result from LUIS
            JObject rss = JObject.Parse(strResponseContent);
            string intent = (string)rss["topScoringIntent"]["intent"];
            engine.RequestRecognizerUpdate();

            if (intent.Equals("Contato"))
            {
                emotion = "Joy";
                Speaker.Speak("Você pode entrar em contato pelo telefone (19) 3406-5776");
            }
            else if (intent.Equals("AconteceVestibularFatec"))
            {
                emotion = "Anger";
                Speaker.Speak("Essa informação você encontrará com precisão no site vestibularfatec.com.br");
            }
            else if (intent.Equals("Cumprimento"))
            {
                emotion = "Joy";
                Speaker.Speak("Olá. Como posso te ajudar?");
            }
            else if (intent.Equals("Curso"))
            {
                emotion = "Sadness";
                Speaker.Speak("Os cursos oferecidos são Análise e Desenvolvimento de Sistemas, Segurança da Informação, Jogos Digitais, Logistica, Gestão Empresarial, Produção Textil e Têxtil e Moda");
            }
            Translate();
        }

    }
    #endregion
}
