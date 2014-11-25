﻿using System.Collections.Generic;
using System.Collections.Specialized;
using Orchard;
using Transformalize.Orchard.Models;

namespace Transformalize.Orchard.Services {
    public interface ITransformalizeService : IDependency {
        IEnumerable<int> FilesCreated { get; }
        string InjectParameters(ConfigurationPart part, NameValueCollection query);
        string GetMetaData(string configuration);
        IEnumerable<ConfigurationPart> GetConfigurations();
        IEnumerable<ConfigurationPart> GetAuthorizedConfigurations();
        ConfigurationPart GetConfiguration(int id);
        TransformalizeResponse Run(TransformalizeRequest request);
    }


}



