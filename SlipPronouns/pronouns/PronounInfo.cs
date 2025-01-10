using Newtonsoft.Json;

namespace SlipPronouns.pronouns
{
    public class PronounInfo
    {
        public string name;
        public string subject;
        [JsonProperty("object")]
        public string obj;
        public bool singular;
    }
}
