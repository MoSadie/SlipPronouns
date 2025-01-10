namespace SlipPronouns.pronouns
{
    public class PronounResponse
    {
        internal string channel_id;
        internal string channel_login;
        internal string pronoun_id;
        internal string alt_pronoun_id;

        public PronounResponse(string channel_id, string channel_login, string pronoun_id, string alt_pronoun_id)
        {
            this.channel_id = channel_id;
            this.channel_login = channel_login;
            this.pronoun_id = pronoun_id;
            this.alt_pronoun_id = alt_pronoun_id;
        }
    }
}
