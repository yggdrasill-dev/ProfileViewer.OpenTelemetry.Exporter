using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	internal class ResourceFileProvider : IResourceFileProvider
	{
		private readonly EmbeddedFileProvider m_EmbeddedFileProvider;

		public ResourceFileProvider()
		{
			m_EmbeddedFileProvider = new EmbeddedFileProvider(GetType().GetTypeInfo().Assembly);
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return m_EmbeddedFileProvider.GetDirectoryContents(subpath);
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			return m_EmbeddedFileProvider.GetFileInfo(subpath);
		}

		public IChangeToken Watch(string filter)
		{
			return m_EmbeddedFileProvider.Watch(filter);
		}
	}
}
