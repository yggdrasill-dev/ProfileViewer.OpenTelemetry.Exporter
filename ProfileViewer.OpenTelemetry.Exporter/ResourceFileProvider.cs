using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OpenTelemetry.Exporter.ProfileViewer;

internal class ResourceFileProvider : IResourceFileProvider
{
    private readonly EmbeddedFileProvider m_EmbeddedFileProvider;

    public ResourceFileProvider()
    {
        m_EmbeddedFileProvider = new EmbeddedFileProvider(
            Assembly.GetExecutingAssembly(),
            "OpenTelemetry.Exporter.ProfileViewer");
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
