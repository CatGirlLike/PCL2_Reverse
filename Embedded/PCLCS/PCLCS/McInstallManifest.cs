using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PCLCS;

public record McInstallManifest(int SchemaVersion, McComponents Components, DateTime LastModifiedUtc, IReadOnlyList<ManagedFile> ManagedFiles)
{
	public record ManagedFile(string RelativePath, string Sha1, DateTime LastWriteTimeUtc, [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Component = null);

	public McInstallManifest RefreshModComponents(string gameFolder)
	{
		List<ManagedFile> invalidFiles = new List<ManagedFile>();
		foreach (ManagedFile managedFile in ManagedFiles)
		{
			if (managedFile.Component != null)
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(gameFolder, managedFile.RelativePath));
				if (!fileInfo.Exists || fileInfo.LastWriteTimeUtc != managedFile.LastWriteTimeUtc.ToUniversalTime())
				{
					invalidFiles.Add(managedFile);
				}
			}
		}
		if (!invalidFiles.Any())
		{
			return this;
		}
		return this with
		{
			Components = Components with
			{
				OptiFineEntry = (invalidFiles.Any((ManagedFile file) => file.Component == "OptiFine") ? null : Components.OptiFineEntry),
				FabricApiVersion = (invalidFiles.Any((ManagedFile file) => file.Component == "FabricApi") ? null : Components.FabricApiVersion),
				OptiFabricVersion = (invalidFiles.Any((ManagedFile file) => file.Component == "OptiFabric") ? null : Components.OptiFabricVersion)
			},
			ManagedFiles = ManagedFiles.Where((ManagedFile file) => !invalidFiles.Contains(file)).ToList(),
			LastModifiedUtc = DateTime.UtcNow
		};
	}
}
