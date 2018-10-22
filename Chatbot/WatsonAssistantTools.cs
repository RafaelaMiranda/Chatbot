using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot
{
    class WatsonAssistantTools
    {
        public static string Assistant(string responseBody)
        {
            var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);

            JToken rss = JToken.Parse(result);
            string rssTitle = (string)rss[0]["text"];
            return rssTitle;
        }
    }
}
