namespace Brimborium.CopyProject;

public static partial class AppConfigurationUtility {
    public static string GetRootFolder(string rootFolderAbsolute) {
        if (!System.IO.Directory.Exists(rootFolderAbsolute)) {
            throw new InvalidOperationException($"RootFolder '{rootFolderAbsolute}' does not exist.");
        }
        return rootFolderAbsolute;
    }

    public static string GetRootFolderFromMarkerFile(
        string rootMarkerFile, 
        string? rootFolderRelative) {
        string folder;
        {
            var nextFolder = System.IO.Path.GetDirectoryName(typeof(AppConfigurationService).Assembly.Location);
            if (string.IsNullOrEmpty(nextFolder)) {
                throw new InvalidOperationException($"RootMarkerFile '{rootMarkerFile}' not found.");
            } else {
                folder = nextFolder;
            }
        }

        while (folder is { Length: > 3 }) {
            if (System.IO.File.Exists(System.IO.Path.Combine(folder, rootMarkerFile))) {
                // marker found
                if (rootFolderRelative is { Length: > 0 }) {
                    folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(folder, rootFolderRelative));
                    if (!System.IO.Directory.Exists(folder)) {
                        throw new InvalidOperationException($"RootFolder '{folder}' does not exist.");
                    }
                    return folder;
                } else { 
                    return folder;
                }
            } else {
                // marker not found, go up one folder
                var nextFolder = System.IO.Path.GetDirectoryName(folder);
                if (string.IsNullOrEmpty(nextFolder)) {
                    throw new InvalidOperationException($"RootMarkerFile '{rootMarkerFile}' not found.");
                } else {
                    folder = nextFolder;
                }
            }
        }
        throw new InvalidOperationException($"RootMarkerFile '{rootMarkerFile}' not found.");
    }
}