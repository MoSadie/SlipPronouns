using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SlipPronouns.pronouns
{
    public class PronounFetcher
    {
        private string url;
        private Dictionary<string, PronounResponse> pronounsCache;
        private Dictionary<string, PronounInfo> pronounInfo;

        private static HttpClient client;

        public PronounFetcher()
        {
            this.url = null;
            pronounsCache = new Dictionary<string, PronounResponse>();
            pronounInfo = new Dictionary<string, PronounInfo>();

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SlipPronouns/MoSadie " + PluginInfo.PLUGIN_VERSION);
        }

        public void init(string url)
        {
            SlipPronouns.Log.LogInfo($"Initializing PronounFetcher with URL: {url}");
            this.url = url;

            // Fetch pronouns from the API endpoint /pronouns
            try
            {
                string endpoint = $"{url}/pronouns";
                HttpResponseMessage response = client.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    Dictionary<string, PronounInfo> pronouns = JsonConvert.DeserializeObject<Dictionary<string, PronounInfo>>(responseBody);
                    pronounInfo = pronouns;
                    SlipPronouns.Log.LogInfo("Pronouns list fetched successfully");
                }
                else
                {
                    SlipPronouns.Log.LogError("An unknown error occurred while fetching pronouns");
                    SlipPronouns.Log.LogError($"Status code: {response.StatusCode}");
                    this.url = null; // Clear the URL so we don't attempt to fetch pronouns again
                }
            }
            catch (Exception e)
            {
                SlipPronouns.Log.LogError("An error occurred while fetching pronouns");
                SlipPronouns.Log.LogError(e.Message);
                this.url = null; // Clear the URL so we don't attempt to fetch pronouns again
            }
        }

        public string getSuffix(string user)
        {
            if (pronounsCache.ContainsKey(user))
            {
                return getSuffix(pronounsCache[user]);
            }
            
            // Attempt to fetch pronouns from the API
            PronounResponse pronouns = fetchUserPronouns(user);
            pronounsCache[user] = pronouns;
            return getSuffix(pronouns);
        }

        private PronounResponse fetchUserPronouns(string user)
        {
            if (url == null || url == "" || url == " ")
            {
                SlipPronouns.Log.LogWarning("API URL is not set. Please set the API URL before attempting to fetch pronouns.");
                return null;
            }
            // Fetch pronouns from the API endpoint /users/{user}
            try
            {

                string endpoint = $"{url}/users/{user}";
                HttpResponseMessage response = client.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    PronounResponse pronouns = JsonConvert.DeserializeObject<PronounResponse>(responseBody);
                    SlipPronouns.debugLogInfo($"Pronouns fetched successfully for {user}");
                    return pronouns;
                } else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    SlipPronouns.debugLogWarn($"No pronouns found for {user}");
                    return null;
                } else
                {
                    SlipPronouns.Log.LogError($"An unknown error occurred while fetching pronouns for {user}");
                    SlipPronouns.Log.LogError($"Status code: {response.StatusCode}");
                    return null;
                }
            } catch (Exception e)
            {
                SlipPronouns.Log.LogError($"An error occurred while fetching pronouns for {user}");
                SlipPronouns.Log.LogError(e.Message);
                return null;
            }

        }

        public string getSuffix(PronounResponse userPronouns)
        {
            if (userPronouns == null) // No pronouns
            {
                return "";
            }

            if (pronounInfo.ContainsKey(userPronouns.pronoun_id))
            {
                if (userPronouns.alt_pronoun_id != null && pronounInfo.ContainsKey(userPronouns.alt_pronoun_id))
                {
                    return $" ({pronounInfo[userPronouns.pronoun_id].subject}/{pronounInfo[userPronouns.alt_pronoun_id].subject})";
                }
                else
                {
                    PronounInfo pronouns = pronounInfo[userPronouns.pronoun_id];
                    if (pronouns.singular)
                    {
                        return $" ({pronouns.subject})";
                    }
                    else
                    {
                        return $" ({pronouns.subject}/{pronouns.obj})";
                    }
                }
            }
            return "";
        }
    }
}
