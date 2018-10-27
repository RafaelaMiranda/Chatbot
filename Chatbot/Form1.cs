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
using System.Linq;

namespace Chatbot
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine; // engine de Reconhecimento

        public Form1()
        {
            InitializeComponent();
            SpeechToText();
            textBot.Hide();
            textUsuario.Hide();
        }
        #region speech
        private async void SpeechToText()
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

                string[] words = new string[109];
                for (int i = 0; i < 109; i++)
                {
                    words[i] = (string)rss["utterances"][i]["text"];
                    engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words[i]))));
                }

                var speaker = textUsuario.Text;

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(TranslateMessageToEnglish);
                engine.RecognizeAsync(RecognizeMode.Multiple); // iniciar o reconhecimento
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Assistant);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu no LoadSpeech(): " + ex.Message);
            }
        }

        #endregion

        #region translate
        // Utilização da API de tradução pelo fato do Tone Analyzer (emoções) não possuir compatibilidade com português
        async void TranslateMessageToEnglish(object s, SpeechRecognizedEventArgs e)
        {
            string host = "https://api.cognitive.microsofttranslator.com";
            string path = "/translate?api-version=3.0";
            string params_ = "&to=en";
            string uri = host + path + params_;
            string key = "7315970e7abf4e3ba12f900725ff9dd5";
            string text = e.Result.Text;

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
                await ToneAnalyzerAsync();
            }
        }
        #endregion
        #region ToneAnalyzer
        private async System.Threading.Tasks.Task ToneAnalyzerAsync()
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

            string emotion = "";
            float score = 0;

            for (int i = 0; i < 5; i++)
            {
                float rssEmotion = (float)rss["document_tone"]["tone_categories"][0]["tones"][i]["score"];

                if (i == 0)
                    rssEmotion = score;

                if (rssEmotion > score)
                {
                    rssEmotion = score;
                    emotion = (string)rss["document_tone"]["tone_categories"][0]["tones"][i]["tone_name"];
                }
            }

            var client2 = new HttpClient();
            var queryString2 = HttpUtility.ParseQueryString(string.Empty);

            // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
            var subscriptionKey2 = "4beff44499c8492a8359493e5b1d8bd9";

            // The request header contains your subscription key
            client2.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey2);
            var uri2 = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/467f5821-0494-4de0-bde7-70cb65aaa195/versions/0.1/examples?skip=0&take=120";

            var response2 = await client2.GetAsync(uri2);
            var strResponseContent2 = await response2.Content.ReadAsStringAsync();
            // Display the JSON result from LUIS
            JArray rss2 = JArray.Parse(strResponseContent2);

            string[] intents = new string[109];
            for (int i = 0; i < 109; i++)
            {
                if ((string)rss2[i]["text"] == textUsuario.Text)
                {
                    intents[i] = (string)rss2[i]["intentLabel"];

                    if (emotion.Equals("Joy") || ((string)rss2[i]["intentLabel"]).Equals("Cumprimento") || ((string)rss2[i]["intentLabel"]).Equals("Cursos") || ((string)rss2[i]["intentLabel"]).Equals("Duração") || ((string)rss2[i]["intentLabel"]).Equals("Tecnólogo") || ((string)rss2[i]["intentLabel"]).Equals("Site") || ((string)rss2[i]["intentLabel"]).Equals("Telefone") || ((string)rss2[i]["intentLabel"]).Equals("Vestibular"))
                    {
                        pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_05.png");
                    }
                    else if (emotion.Equals("Anger"))
                    {
                        pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_08.png");
                    }
                    else if (((string)rss2[i]["intentLabel"]).Equals("Localização") || ((string)rss2[i]["intentLabel"]).Equals("Contato") || ((string)rss2[i]["intentLabel"]).Equals("DiferençasETEC") || ((string)rss2[i]["intentLabel"]).Equals("Graduação") || ((string)rss2[i]["intentLabel"]).Equals("Inscrições") || ((string)rss2[i]["intentLabel"]).Equals("MEC") || ((string)rss2[i]["intentLabel"]).Equals("ManualAluno"))
                    {
                        pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_02.png");
                    }
                    else
                    {                    
                        pImagem.BackgroundImage = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\TCC\Codigo\Chatbot\Chatbot\Resources\Eva\eve_eyes_06.png");
                    }
                }
            }
        }
        #endregion
        #region WatsonAssistant
        async void Assistant(object s, SpeechRecognizedEventArgs e)
        {
            var client = new HttpClient();

            textUsuario.Text = e.Result.Text;

            string host = "https://node-red-chatbot-fatecamericana.mybluemix.net/facebook?mensagem=";
            string message = textUsuario.Text;
            string url = host + message;

            var response = await client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            string assistant = WatsonAssistantTools.Assistant(responseBody);

            Speaker.Speak(assistant);
        }
        #endregion
    }
}
