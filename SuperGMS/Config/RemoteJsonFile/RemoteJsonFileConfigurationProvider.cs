﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SuperGMS.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.Config.RemoteJsonFile
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
                        var jsonConfigurationFileParser = new JsonConfigurationFileParser();
                        base.Data = jsonConfigurationFileParser.Parse(stream);
                        _jObject= jsonConfigurationFileParser.jObject;
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
                        string rootPath = GetRootPath();
                        if (!Directory.Exists(rootPath))
                            Directory.CreateDirectory(rootPath);
                        using (var fileWriter = File.CreateText(GetTempConfigPath()))
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

        private JObject _jObject;

        public JObject jObject { get { return _jObject; } }

        /// <summary>
        /// 获取配置根路径
        /// </summary>
        /// <returns></returns>
        public static string GetRootPath()=> Path.Combine(Directory.GetCurrentDirectory(), "temp");

        /// <summary>
        /// 获取临时配置文件的路径
        /// </summary>
        /// <returns></returns>
        public static string GetTempConfigPath() => Path.Combine(GetRootPath(), ConfigFileName);

        /// <summary>
        /// 配置文件名称
        /// </summary>
        public static string ConfigFileName = "config.json";

    }
}
