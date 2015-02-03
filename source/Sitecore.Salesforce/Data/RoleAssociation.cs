using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Salesforce.Data
{
  using Client.Data;
  using Newtonsoft.Json;

  public class RoleAssociation: SObject
  {
    [JsonProperty("Id")]
    public string Id { get; protected set; }

    public RoleAssociation()
    {
    }
  }
}
