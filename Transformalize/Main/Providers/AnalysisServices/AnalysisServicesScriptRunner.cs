#region License

// /*
// Transformalize - Replicate, Transform, and Denormalize Your Data...
// Copyright (C) 2013 Dale Newman
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

#endregion

using System;
using Microsoft.AnalysisServices;

namespace Transformalize.Main.Providers.AnalysisServices {
    public class AnalysisServicesScriptRunner : IScriptRunner {

        public IScriptReponse Execute(AbstractConnection connection, string script, int timeOut = 0) {
            var response = new ScriptResponse();
            var server = new Server();

            try {
                TflLogger.Debug(string.Empty, string.Empty, "Connecting to {0} on {1}.", connection.Database, connection.Server);
                server.Connect(connection.GetConnectionString());

                var results = server.Execute(script);

                foreach (XmlaResult result in results) {
                    foreach (XmlaMessage message in result.Messages) {
                        response.Messages.Add(message.Description);
                    }
                }
                response.Success = response.Messages.Count == 0;
            } catch (Exception e) {
                TflLogger.Debug(string.Empty, string.Empty, e.Message + (e.InnerException != null ? " " + e.InnerException.Message : string.Empty));
                response.Messages.Add(e.Message);
            } finally {
                if (server.Connected) {
                    TflLogger.Debug(string.Empty, string.Empty, "Disconnecting from {0} on {1}.", connection.Database, connection.Server);
                    server.Disconnect();
                }
            }
            return response;
        }
    }
}