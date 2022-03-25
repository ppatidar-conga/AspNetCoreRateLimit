using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace Customization
{
    public class LicencingManager : ILicencingManager
    {
        public async Task<string> GetOrganizationRateLimitPlan(string OrgId)
        {
            //TO-DO , actual implementation , for now just read from some json file
            string defaultTier = "default";
            if (File.Exists("client.json"))
            {
                var orgLicencesJson = File.ReadAllText("client.json");
                //var tiers = new Dictionary<string, string>();
                //tiers.Add("c1", "silver");
                //tiers.Add("c2", "gold");
                var tiers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(orgLicencesJson);
                //var tiers = JsonConvert.DeserializeObject<Dictionary<string, string>>(orgLicencesJson);
                if (tiers.ContainsKey(OrgId))
                {
                    return tiers[OrgId];
                }
            }
            return defaultTier;
        }
    }
}
