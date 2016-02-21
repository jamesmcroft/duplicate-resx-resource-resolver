// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James Croft">
//   Copyright (c) James Croft. 
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ResxResolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// The resource resolver program.
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

            var resourceFiles = new List<FileInfo>();

            try
            {
                var rootDirectory = new DirectoryInfo(args[0]);
                GetResourcesFromDirectory(rootDirectory, resourceFiles);
            }
            catch (Exception)
            {
                // ToDo
            }

            foreach (var resourceFile in resourceFiles)
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

            try
            {
                var childDirectories = directoryInfo.GetDirectories().ToList();

                if (childDirectories.Count > 0)
                {
                    foreach (var directory in childDirectories)
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

        private static void RemoveDuplicateResourcesInFile(FileSystemInfo fi)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(fi.FullName);

                var currentResources = new SortedList<string, XmlNode>();

                var resourceNodes = xmlDocument.SelectNodes("//data[@name]");
                if (resourceNodes != null)
                {
                    foreach (XmlNode resource in resourceNodes)
                    {
                        if (resource.Attributes != null)
                        {
                            var resourceName = resource.Attributes["name"].Value.ToLower();
                            if (!currentResources.ContainsKey(resourceName))
                            {
                                currentResources.Add(resourceName, resource);
                            }
                        }

                        resource.ParentNode?.RemoveChild(resource);
                    }
                }

                foreach (var resourceKey in currentResources.Keys)
                {
                    xmlDocument.DocumentElement?.AppendChild(currentResources[resourceKey]);
                }

                xmlDocument.Save(fi.FullName);
            }
            catch (Exception)
            {
                // ToDo
            }
        }
    }
}