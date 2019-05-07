using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BronzeBar
{
    class BatchScriptFactory
    {
        private readonly string programRootDirectory;
        public BatchScriptFactory(string workingDir)
        {
            programRootDirectory = workingDir;
        }
        public string CreateAppDeployer(string packageName, string appName, string fullDestinationPath)
        {
            string batchFile = File.ReadAllText(Path.Combine(programRootDirectory, "template_appdeployer.bat"));
            batchFile = string.Format(batchFile, fullDestinationPath, appName);
            return batchFile;
        }
    }
}
