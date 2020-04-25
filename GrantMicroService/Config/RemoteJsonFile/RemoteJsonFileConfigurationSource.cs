using Microsoft.Extensions.Configuration;

namespace GrantMicroService.Config.RemoteJsonFile
{
    public class RemoteJsonFileConfigurationSource : object, IConfigurationSource
    {
        /// <summary>
        /// The Uri for download file.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Determines if loading the file is optional.
        /// </summary>
        public bool Optional { get; set; }
        /// <summary>
        /// Builds the <see cref="T:Grant.NetCore.Superman.Configuration.RemoteJsonFileConfigurationProvider" /> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" />.</param>
        /// <returns>A <see cref="T:Grant.NetCore.Superman.Configuration.RemoteJsonFileConfigurationProvider" /></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RemoteJsonFileConfigurationProvider(this);
        }
    }
}
