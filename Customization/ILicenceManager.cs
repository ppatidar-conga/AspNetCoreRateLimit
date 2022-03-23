using System;
using System.Collections.Generic;
using System.Text;

namespace Customization
{
   public interface ILicenceManager
    {
        string FindTier(string clientId);
    }
}
