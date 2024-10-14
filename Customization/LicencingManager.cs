using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

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
                var tiers = JsonSerializer.Deserialize<List<OrgAPITier>>(orgLicencesJson);
                if (tiers.Any(x => x.OrgId == OrgId))
                {
                    return tiers.FirstOrDefault(x => x.OrgId == OrgId).APITier;
                }
            }
            return defaultTier;
        }
    }
}
