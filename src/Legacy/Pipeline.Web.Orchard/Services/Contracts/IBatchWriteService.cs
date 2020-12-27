﻿using System.Collections.Generic;
using System.Web;
using Orchard;
using Transformalize.Configuration;

namespace Pipeline.Web.Orchard.Services.Contracts {
    public interface IBatchWriteService : IDependency {
        int Write(HttpRequestBase request, Process process, IDictionary<string,string> parameters);
    }
}