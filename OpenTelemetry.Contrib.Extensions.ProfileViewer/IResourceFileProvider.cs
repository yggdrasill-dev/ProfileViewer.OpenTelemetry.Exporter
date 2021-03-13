using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	interface IResourceFileProvider: IFileProvider
	{
	}
}
