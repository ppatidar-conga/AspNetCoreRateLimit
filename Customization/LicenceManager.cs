using System;
using System.Collections.Generic;

namespace Customization
{
    public class LicenceManager : ILicenceManager
    {
        Dictionary<string, string> _clientTiers;
        public LicenceManager()
        {
            InitlizeClientTierDetails();
        }
        public string FindTier(string clientId)
        {
            if (_clientTiers.ContainsKey(clientId))
            {
                return _clientTiers[clientId];
            }
            else
                return "anon";
        }


        private void InitlizeClientTierDetails()
        {
            _clientTiers = new Dictionary<string, string>();
            _clientTiers.Add("c1", "silver");
            _clientTiers.Add("c2", "gold");
        }
    }
}
