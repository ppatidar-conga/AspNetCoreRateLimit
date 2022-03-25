using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customization
{
   public interface ILicencingManager
    {
        Task<string> GetOrganizationRateLimitPlan(string OrgId);
    }
}
