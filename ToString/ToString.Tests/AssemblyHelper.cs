﻿using System.IO;
using System.Reflection;
using System.Text;

namespace ToString.Tests
{
    public static class AssemblyHelper
    {
        private const string RootNamespaceResources = "ToString.Tests.Resources";

        /// <summary>
        /// Gets the string from resource file asynchronous.
        /// </summary>
        /// <param name="resourceFile">The resource file name.</param>
        public static string GetStringFromResourceFileAsync(string resourceFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"{RootNamespaceResources}.{resourceFile}");
            using var reader = new StreamReader(resourceStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
