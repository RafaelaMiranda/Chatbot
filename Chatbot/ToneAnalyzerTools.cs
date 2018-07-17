using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot
{
    class ToneAnalyzerTools
    {
        // Tools to calculate tone analysis values and expose utilities
            public static string JsonPrettify(string json)
            {
                return Newtonsoft.Json.Linq.JObject.Parse(json).ToString();
            }
    }
}
