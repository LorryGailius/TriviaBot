using Newtonsoft.Json;
using System.Collections.Generic;

namespace TriviaBot.External
{
    public class Response
    {
        [JsonProperty("status")]
        public int response_code { get; set; }

        [JsonProperty("results")]
        public List<Question> results { get; set; }
    }
}
