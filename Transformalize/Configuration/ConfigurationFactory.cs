using System;
using System.Collections.Specialized;
using System.IO;
using Transformalize.Main;
using Transformalize.Runner;

namespace Transformalize.Configuration {
    public class ConfigurationFactory {
        private readonly NameValueCollection _query;
        private const StringComparison IC = StringComparison.OrdinalIgnoreCase;
        private readonly string _resource;

        /// <summary>
        /// Turns configuration into a collection of processes.
        /// </summary>
        /// <param name="resource">May be a named process in your application *.config, an xml file name, a url pointing to an xml file on a web server, or the xml configuration itself.  Way too many choices!</param>
        /// <param name="query">an optional collection of named values for replacing $(parameter) place-holders in the configuration.</param>
        public ConfigurationFactory(string resource, NameValueCollection query = null)
        {
            _resource = resource;
            _query = query;
        }

        public ProcessElementCollection Create() {
            return CreateReader().Read();
        }

        private IReader<ProcessElementCollection> CreateReader() {

            if (_resource.StartsWith("<")) {
                return new ProcessXmlConfigurationReader(string.Empty, new ContentsStringReader(_query));
            } else {
                var name = _resource.Contains("?") ? _resource.Substring(0, _resource.IndexOf('?')) : _resource;
                if (Path.HasExtension(name)) {
                    return _resource.StartsWith("http", IC) ?
                        new ProcessXmlConfigurationReader(_resource, new ContentsWebReader()) :
                        new ProcessXmlConfigurationReader(_resource, new ContentsFileReader());
                }
            }

            return new ProcessConfigurationReader(_resource);

        }
    }
}