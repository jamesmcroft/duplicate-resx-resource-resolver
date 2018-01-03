// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James Croft">
//   Copyright (c) James Croft.
// </copyright>
// <summary>
//   Defines the console application which resolves conflicting duplicate resource files in RESW and RESX files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ResxResolver
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using WinUX;

    /// <summary>
    /// Defines the console application which resolves conflicting duplicate resource files in RESW and RESX files.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">
        /// Program entry parameter, should be the directory where the resources are located.
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            List<FileInfo> resourceFiles = new List<FileInfo>();

            try
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(args[0]);

                Console.WriteLine($"Resolving duplicate resources for resource files in '{rootDirectory.FullName}'.");

                GetResourcesFromDirectory(rootDirectory, resourceFiles);
            }
            catch (Exception)
            {
                // ToDo
            }

            Console.WriteLine($"Attempting to remove duplicate resources in {resourceFiles.Count} files.");

            foreach (FileInfo resourceFile in resourceFiles)
            {
                RemoveDuplicateResourcesInFile(resourceFile);
            }
        }

        private static void GetResourcesFromDirectory(DirectoryInfo directoryInfo, List<FileInfo> resourceFiles)
        {
            if (resourceFiles == null)
            {
                resourceFiles = new List<FileInfo>();
            }

            // Ignore checking the obj or bin folders.
            if (directoryInfo.FullName.Contains("\\obj\\", CompareOptions.IgnoreCase) || directoryInfo.FullName.Contains("\\bin\\", CompareOptions.IgnoreCase))
            {
                return;
            }

            try
            {
                Console.WriteLine($"Looking for resource files in '{directoryInfo.FullName}'.");

                List<DirectoryInfo> childDirectories = directoryInfo.GetDirectories().ToList();

                if (childDirectories.Count > 0)
                {
                    foreach (DirectoryInfo directory in childDirectories)
                    {
                        GetResourcesFromDirectory(directory, resourceFiles);
                    }
                }

                resourceFiles.AddRange(directoryInfo.GetFiles("*.resx"));
                resourceFiles.AddRange(directoryInfo.GetFiles("*.resw"));
            }
            catch (Exception)
            {
                // ToDo
            }
        }

        private static void RemoveDuplicateResourcesInFile(FileSystemInfo fileInfo)
        {
            try
            {
                int duplicates = 0;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileInfo.FullName);

                SortedList<string, XmlNode> currentResources = new SortedList<string, XmlNode>();

                XmlNodeList resourceNodes = xmlDocument.SelectNodes("//data[@name]");
                if (resourceNodes != null)
                {
                    foreach (XmlNode resource in resourceNodes)
                    {
                        if (resource.Attributes != null)
                        {
                            string resourceName = resource.Attributes["name"].Value;

                            KeyValuePair<string, XmlNode> existingResource = currentResources.FirstOrDefault(
                                x => x.Key.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));

                            if (existingResource.Value == null)
                            {
                                currentResources.Add(resourceName, resource);
                            }
                            else
                            {
                                duplicates++;
                                Console.WriteLine(
                                    $"Duplicate resource '{resourceName}' removed from '{fileInfo.FullName}'.");
                            }
                        }

                        resource.ParentNode?.RemoveChild(resource);
                    }
                }

                foreach (string resourceKey in currentResources.Keys)
                {
                    xmlDocument.DocumentElement?.AppendChild(currentResources[resourceKey]);
                }

                xmlDocument.Save(fileInfo.FullName);

                    Console.WriteLine(
                        $"Removed {duplicates} duplicate resources from '{fileInfo.FullName}'.");
            }
            catch (Exception)
            {
                // ToDo
            }
        }
    }
}