using System;
using System.Linq;
using Transformalize.Model;
using Transformalize.Operations;
using Transformalize.Rhino.Etl.Core;

namespace Transformalize.Processes {

    public class TransformProcess : EtlProcess {

        private readonly Process _process;

        public TransformProcess(Process process) : base(process.Name) {
            _process = process;
        }

        protected override void Initialize() {
            Register(new ParametersExtract(_process));
            Register(new ProcessTransform(_process));
            RegisterLast(new ResultsLoad(_process));
        }

        protected override void PostProcessing() {

            var errors = GetAllErrors().ToArray();
            if (errors.Any()) {
                foreach (var error in errors) {
                    Error(error.InnerException, "Message: {0}\r\nStackTrace:{1}\r\n", error.Message, error.StackTrace);
                }
                throw new InvalidOperationException("Houstan.  We have a problem.");
            }

            base.PostProcessing();
        }

    }

}
