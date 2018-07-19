using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot
{
    public class WatsonConversationHelper
    {
        private readonly string _Server;
        private readonly NetworkCredential _NetCredential;
        public WatsonConversationHelper(string userId, string password)
        {
            _Server = string.Format("https://gateway.watsonplatform.net/assistant/api/v1/workspaces/377432dc-0526-4ea2-9fc4-942f004dee37/message?version=2018-07-10&nodes_visited_details=false");
            _NetCredential = new NetworkCredential(userId, password);
        }
        public async Task<string> GetResponse(string input, string context = null)
        {
            string req = null;
            if (string.IsNullOrEmpty(context)) req = "{\"input\": {\"text\": \"" + input + "\"}, \"alternate_intents\": true}";
            else req = "{\"input\": {\"text\": \"" + input + "\"}, \"alternate_intents\": true}, \"context\": \"" + context + "\"";
            using (var handler = new HttpClientHandler
            {
                Credentials = _NetCredential
            })
            using (var client = new HttpClient(handler))
            {
                var cont = new HttpRequestMessage();
                cont.Content = new StringContent(req.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync(_Server, cont.Content);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }

}
