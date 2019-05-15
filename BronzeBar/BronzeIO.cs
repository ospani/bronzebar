using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BronzeBar
{
    public static class BronzeIO
    {
        public static string WorkingDirectory = "C:\\bronzebar\\";
        public static string PackagesDirectory = $"{Path.Combine(WorkingDirectory, "packages")}";

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (File.Exists(temppath))
                {
                    Console.WriteLine($"UPDATE:\t{temppath}");
                }
                else
                {
                    Console.WriteLine($"NEW:\t{temppath}");
                }

                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static bool PackageExists(string packageName)
        {
            bool a = Directory.Exists(Path.Combine(PackagesDirectory, packageName));
            bool b = Directory.Exists(Path.Combine(PackagesDirectory, packageName, "data"));
            bool c = Directory.Exists(Path.Combine(PackagesDirectory, packageName, "deployments"));
            return a && b && c;
        }

        public static string GetPackageFullPath(string packageName)
        {
            if (!PackageExists(packageName))
            {
                return "";
            }
            return Path.Combine(PackagesDirectory, packageName);
        }
        public static string GetSysFolderInPackage(string packageName, string folderWithinPackage)
        {
            if (!Directory.Exists(Path.Combine(GetPackageFullPath(packageName), folderWithinPackage)))
            {
                return "";
            }
            return Path.Combine(GetPackageFullPath(packageName), folderWithinPackage);
        }
    }
}
