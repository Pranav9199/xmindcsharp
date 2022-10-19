using System;
using System.Collections.Generic;
using XMindAPI.Configuration;
using XMindAPI.Models;
using XMindAPI.Writers;
using System.IO;
using System.Linq;
using XMindAPI.Zip;

// TODO: consider to change to XMindAPI.Extensions to follow convention
// but this functionality is essential
namespace XMindAPI
{
    public static class XMindConfigurationExtensions
    {
        // TODO: add API to write to stream, minor because it is always possible to implement IXMindWriter

        /// <summary>
        /// Writes file to <paramref name="basePath"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="basePath"></param>
        /// <param name="zip"></param>
        /// <returns></returns>
        public static XMindConfiguration WithFileWriter(
            this XMindConfiguration config,
            string? basePath = default,
            bool zip = true)
        {
            var result = config
                .WriteTo.Writers(FileWriterFactory.CreateStandardWriters(basePath))
                .WriteTo.SetWriterBinding(FileWriterFactory.CreateStandardResolvers());
            if (zip)
            {
                result.WriteTo.SetFinalizeAction(CreateZipXMindFolderCallback(basePath));
            }
            return result;
        }
        /// <summary>
        /// Write file to default location - "xmind-output"
        /// </summary>
        /// <param name="config"></param>
        /// <param name="zip"></param>
        /// <returns></returns>
        public static XMindConfiguration WithFileWriter(
            this XMindConfiguration config,
            bool zip = true)
        {
            return config.WithFileWriter(basePath: null, zip: zip);
        }

        public static XMindConfiguration WithInMemoryWriter(
            this XMindConfiguration config
        )
        {
            return config.WriteTo
                .Writer(
                    new InMemoryWriter(
                        new InMemoryWriterOutputConfig($"[in-memory-writer]")));
        }

        private static Action<List<XMindWriterContext>, XMindWorkBook> CreateZipXMindFolderCallback(
            string? basePath)
        {
            var xMindSettings = XMindConfigurationLoader.Configuration.XMindConfigCollection;
            if (basePath is null && xMindSettings is object)
            {
                basePath = xMindSettings["output:base"];
            }
            var filesToZipLabels = XMindConfigurationLoader
                .Configuration
                .GetOutputFilesDefinitions()
                .Values;
            return (ctx, workBook) =>
            {
                using ZipStorer zip = ZipStorer.Create(Path.Combine(basePath, workBook.Name), string.Empty);
                var filesToZip = XMindConfigurationLoader
                    .Configuration
                    .GetOutputFilesLocations().Where(kvp => filesToZipLabels.Contains(kvp.Key));
                foreach (var fileToken in filesToZip)
                {
                    var fileDir = Path.Combine(basePath, fileToken.Value);
                    var fullPath = Path.Combine(fileDir, fileToken.Key);
                    if (fileToken.Value == string.Empty)
                    {
                        zip.AddFile(ZipStorer.Compression.Deflate, fullPath, fileToken.Key, string.Empty);
                    }
                    else
                    {
                        zip.AddDirectory(ZipStorer.Compression.Deflate, fileDir, null);
                    }
                    File.Delete(fullPath);
                    if (!string.IsNullOrEmpty(fileToken.Value) && Directory.Exists(fileDir))
                    {
                        Directory.Delete(fileDir);
                    }
                }
            };
        }

    }
}
