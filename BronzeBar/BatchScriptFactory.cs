using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BronzeBar
{
    class BatchScriptFactory
    {
        private readonly string programRootDirectory;
        private readonly string programScriptsDirectory;

        public BatchScriptFactory(string workingDir)
        {
            programRootDirectory = workingDir;
            programScriptsDirectory = Path.Combine(programRootDirectory, @"scripts\");
        }

        public string CreateAppDeployer(string packageName, string appName, string fullDestinationPath)
        {
            string batchFile = null;
            if (Directory.Exists(programScriptsDirectory) && File.Exists(Path.Combine(programScriptsDirectory, "template_appdeployer.bat")))
            {
                batchFile = File.ReadAllText(Path.Combine(programScriptsDirectory, "template_appdeployer.bat"));
                batchFile = string.Format(batchFile, Path.Combine(fullDestinationPath), appName, Path.Combine(fullDestinationPath, appName));
            }
            return batchFile;     
        }

        public string CreatePackageDeployer(string packageName, string fullDestinationPath)
        {
            string batchFile = null;
            if (Directory.Exists(programScriptsDirectory) && File.Exists(Path.Combine(programScriptsDirectory, "template_packdeployer.bat")))
            {
                batchFile = File.ReadAllText(Path.Combine(programRootDirectory, @"scripts\", "template_packdeployer.bat"));
            }
            return batchFile;
        }
    }
}
