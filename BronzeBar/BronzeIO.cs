﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BronzeBar
{
    public static class BronzeIO
    {
        public static readonly string WorkingDirectory = "C:\\bronzebar\\";
        public static readonly string PackagesDirectory = $"{Path.Combine(WorkingDirectory, "packages")}";

        //Taken and slightly adapted from the excellent example over at https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
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
        public static bool CreateBBD(string bddContent, string applicationName)
        {
            if (!Directory.Exists(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data")))) return false;

            try
            {
                using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"), $"{applicationName}.BBD")))
                {
                    sw.Write(bddContent);
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"CreateBBD encountered an exception: {ex.Message}");
                while(ex.InnerException != null)
                {
                    Console.WriteLine($"CreateBBD encountered an exception: {ex.InnerException}");
                    ex = ex.InnerException;
                }
                return false;
            }   
        }
    }
}
