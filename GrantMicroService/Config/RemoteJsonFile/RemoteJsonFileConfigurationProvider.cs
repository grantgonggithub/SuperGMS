using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using GrantMicroService.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GrantMicroService.Config.RemoteJsonFile
{
    /// <summary>
    /// Base class for file based <see cref="T:Microsoft.Extensions.Configuration.ConfigurationProvider" />.
    /// </summary>
    public class RemoteJsonFileConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public RemoteJsonFileConfigurationProvider(RemoteJsonFileConfigurationSource source)
        {
            this.Source = source ?? throw new ArgumentNullException("source");
        }

        /// <summary>
        /// The source settings for this provider.
        /// </summary>
        public RemoteJsonFileConfigurationSource Source { get; }

        private void Load(bool reload)
        {
            using (Stream stream = GetFileContentFromUri(this.Source.Uri))
            {
                if (!stream.CanRead)
                {
                    if (!this.Source.Optional && !reload)
                    {
                        StringBuilder stringBuilder = new StringBuilder(string.Format("The configuration file uri '{0}' was not found and is not optional.", this.Source.Uri));
                        throw new FileNotFoundException(stringBuilder.ToString());
                    }
                    base.Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    if (reload)
                    {
                        base.Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    try
                    {
                        base.Data = JsonConfigurationFileParser.Parse(stream);
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new FormatException($"Could not parse the JSON file stream.", ex);
                    }
                }
            }
            base.OnReload();
        }

        /// <summary>
        /// Loads the contents of the file from remote url.
        /// </summary>
        public override void Load()
        {
            this.Load(false);
        }

        private Stream GetFileContentFromUri(string uri)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var dataBytes = webClient.DownloadData(uri);
                    //保存下载的远端配置文件
                    Task.Run(() =>
                    {
                        var configStr = Encoding.UTF8.GetString(dataBytes);
                        string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
                        if (!Directory.Exists(rootPath))
                            Directory.CreateDirectory(rootPath);
                        using (var fileWriter = File.CreateText(Path.Combine(rootPath, "grantsettings.json")))
                        {
                            fileWriter.Write(configStr);
                        }
                    });
                    Stream stream = new MemoryStream(dataBytes);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Download configuration file Error:{e.Message}", e);
            }
        }
    }
}
