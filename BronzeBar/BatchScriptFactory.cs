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
            string batchFile = File.ReadAllText(Path.Combine(programRootDirectory, @"scripts\", "template_appdeployer.bat"));
            batchFile = string.Format(batchFile, Path.Combine(fullDestinationPath), appName, Path.Combine(fullDestinationPath, appName));
            return batchFile;
        }

        public string CreatePackageDeployer(string packageName, string fullDestinationPath)
        {
            string batchFile = File.ReadAllText(Path.Combine(programRootDirectory, @"scripts\", "template_packdeployer.bat"));
            //batchFile = string.Format(batchFile, Path.Combine(fullDestinationPath), appName, Path.Combine(fullDestinationPath, appName));
            return batchFile;
        }
    }
}
