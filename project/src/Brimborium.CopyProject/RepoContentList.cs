namespace Brimborium.CopyProject;

public sealed class RepoContentList {
    private readonly Dictionary<string, RepoFile> _DictFileAction = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FileContent> _DictFileContent = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _RepoAbsolutePath;
    private readonly string _RootRelativePath;
    private readonly string _RootAbsolutePath;
    private readonly ExcludeSettings _ExcludeSettings;

    public Dictionary<string, RepoFile> DictFileAction => this._DictFileAction;

    public Dictionary<string, FileContent> DictFileContent => this._DictFileContent;

    public RepoContentList(
        string repoAbsolutePath,
        string repoRelativePath,
        string rootAbsolutePath,
        ExcludeSettings excludeSettings
        ) {
        ArgumentNullException.ThrowIfNullOrEmpty(repoAbsolutePath, nameof(repoAbsolutePath));
        ArgumentNullException.ThrowIfNullOrEmpty(repoRelativePath, nameof(repoRelativePath));
        ArgumentNullException.ThrowIfNullOrEmpty(rootAbsolutePath, nameof(rootAbsolutePath));
        this._RepoAbsolutePath = repoAbsolutePath;
        this._RootRelativePath = repoRelativePath;
        this._RootAbsolutePath = rootAbsolutePath;
        this._ExcludeSettings = excludeSettings;
    }

    public static RepoContentList Create(
        string rootDir,
        string repoDir,
        ExcludeSettings excludeSettings) {
        ArgumentNullException.ThrowIfNullOrEmpty(rootDir, nameof(rootDir));
        ArgumentNullException.ThrowIfNullOrEmpty(repoDir, nameof(repoDir));

        string repoRelativePath;
        string repoAbsolutePath;
        if (System.IO.Path.IsPathFullyQualified(repoDir)) {
            repoAbsolutePath = System.IO.Path.GetFullPath(repoDir);
            repoRelativePath = System.IO.Path.GetRelativePath(repoDir, rootDir);
        } else {
            repoRelativePath = repoDir;
            repoAbsolutePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, repoDir));
        }
        return new RepoContentList(
            repoAbsolutePath,
            repoRelativePath,
            rootDir,
            excludeSettings);
    }

    public void AddFile(string relativePath, CopyFileSettingsAction action = CopyFileSettingsAction.Invalid) {
        if (this._DictFileAction.TryGetValue(relativePath, out var relativeFile)) {
            if (action is not CopyFileSettingsAction.Invalid or CopyFileSettingsAction.Default) {
                relativeFile.ActionE = action;
            }
        } else {
            relativeFile = new RepoFile(
                relativePath,
                action == CopyFileSettingsAction.Invalid
                    ? CopyFileSettingsAction.Default
                    : action);
            this._DictFileAction.Add(relativePath, relativeFile);
        }
    }

    public bool RescanFolder() {
        this.Clear();
        return this.ScanFolder();
    }

    public ResultScanFolderInit ScanFolderInit() {
        return new ResultScanFolderInit(
                ListRelativeFilename: this._DictFileAction.Keys.ToList()
            );
    }

    public bool ScanFolder(string? folder = null) {
        if (string.IsNullOrEmpty(folder)) {
            folder = this._RepoAbsolutePath;
        } else if (!folder.StartsWith(this._RepoAbsolutePath, StringComparison.OrdinalIgnoreCase)) {
            throw new Exception($"Folder {folder} is not in RepoDir {this._RepoAbsolutePath}");
        }
        if (System.IO.Directory.Exists(folder) is false) {
            return false;
        }
        string repoDirWithSeparator = this._RepoAbsolutePath + System.IO.Path.DirectorySeparatorChar;
        System.Collections.Generic.List<string> listRelativeFile = new();
        System.IO.DirectoryInfo dirInfoFolder = new(folder);

        var listFileInfo = dirInfoFolder.EnumerateFiles("*", new EnumerationOptions() {
            RecurseSubdirectories = false,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
        });
        var listFileInfoFiltered = listFileInfo
            //.Where(fi => !this.IsExcludedFileInfo(fi))
            .ToList();

        foreach (var fileInfo in listFileInfoFiltered) {
            string fullName = fileInfo.FullName;
            if (!fullName.StartsWith(repoDirWithSeparator, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }
            string relativePath = fullName.Substring(repoDirWithSeparator.Length);
            this.AddFile(relativePath, CopyFileSettingsAction.Default);
        }

        List<DirectoryInfo> listDirInfo = [];
        listDirInfo.AddRange(
            dirInfoFolder.EnumerateDirectories(
                "*",
                new EnumerationOptions() {
                    RecurseSubdirectories = true,
                    MaxRecursionDepth = 10,
                    IgnoreInaccessible = true,
                    AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
                })
                .Where(di => this._ExcludeSettings.IsIncludedPath(di.Name))
                );

        foreach (var di in listDirInfo) {
            this.ScanFolder(di.FullName);
        }

        return true;
    }

    public void ScanFolderDone(bool setDeleteAction, ResultScanFolderInit? resultScanFolderInit) {
        if (resultScanFolderInit is null) {
            return;
        }
        var listRelativeFilename = resultScanFolderInit.ListRelativeFilename
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        listRelativeFilename.ExceptWith(this._DictFileAction.Keys);
        foreach (var relativePath in listRelativeFilename) {
            if (this._DictFileAction.TryGetValue(relativePath, out var relativeFile)) {
                if (setDeleteAction) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                } else {
                    this._DictFileAction.Remove(relativePath);
                }
            }
        }
    }

    public bool IsIncludedFileInfo(System.IO.FileInfo? fileInfo) {
        if (fileInfo is null) {
            return false;
        }
        string fullName = fileInfo.FullName;
        if (!fullName.StartsWith(this._RepoAbsolutePath, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (fullName.Length < this._RepoAbsolutePath.Length) {
            return false;
        }
        string relativePath = fullName.Substring(this._RepoAbsolutePath.Length + 1);
        return this._ExcludeSettings.IsIncludedPath(relativePath);
    }

    public string GetAbsolutePath(string relativePath) {
        return System.IO.Path.Combine(this._RepoAbsolutePath, relativePath);
    }

    public string ReadFile(string relativePath) {
        if (this._DictFileContent.TryGetValue(relativePath, out var fileContent)) {
            return fileContent.Content;
        } else {
            var fullPath = this.GetAbsolutePath(relativePath);
            fileContent = FileContent.Create(relativePath, fullPath, string.Empty);
            this._DictFileContent.Add(relativePath, fileContent);
            return fileContent.Content;
        }
    }

    public void WriteFile(string relativePath, string content) {
        string fullPath = this.GetAbsolutePath(relativePath);
        System.IO.File.WriteAllText(fullPath, content);
    }
    public bool DeleteFile(string relativePath) {
        string fullPath = this.GetAbsolutePath(relativePath);
        System.IO.FileInfo fileInfo = new(fullPath);
        if (fileInfo.Exists) {
            fileInfo.Delete();
            return true;
        } else {
            return false;
        }
    }

    public bool CompareFileContent(string srcFileContent, string dstFileContent) {
        if (string.Equals(srcFileContent, dstFileContent, StringComparison.Ordinal)) {
            return true;
        }
        srcFileContent = srcFileContent.ReplaceLineEndings();
        dstFileContent = dstFileContent.ReplaceLineEndings();
        if (string.Equals(srcFileContent, dstFileContent, StringComparison.Ordinal)) {
            return true;
        }
        return false;
    }


    public List<RepoFile> GetFiles() {
        return this._DictFileAction.Values.ToList();
    }

    public void Clear() {
        this._DictFileAction.Clear();
        this._DictFileContent.Clear();
    }
}

public record ResultScanFolderInit(
    List<string> ListRelativeFilename
    );

#if false
    Load() {
         this.DictFileAction.Clear()
         this.DictFileContent.Clear()
        
        if (Test-Path  this.FilelistJsonPath) {
             string  json =  System.IO.File .ReadAllText( this.FilelistJsonPath)
             System.Collections.Generic.List CopyFileSettings    listRelativePath =  System.Text.Json.JsonSerializer .Deserialize( json,  System.Collections.Generic.List CopyFileSettings  )
            foreach ( relativeFile in  listRelativePath) {
                 this.AddFile( relativeFile.RelativePath,  relativeFile.Action)
            }
        }
    }
    Save( bool   saveAll) {
         CopyFileSettings     listRelativeFile = (( this.DictFileAction.Values) | ? { ( saveAll -or ("" -ne  _.Action)) } | Sort-Object -Property RelativePath)
        if ( null -eq  listRelativeFile -or  listRelativeFile.Count -eq 0) {
             string  json = '  '
             System.IO.File .WriteAllText( this.FilelistJsonPath,  json)
        }
        else {
             System.Text.Json.JsonSerializerOptions   options =  System.Text.Json.JsonSerializerOptions .new()
             options.WriteIndented =  true
             string  json =  System.Text.Json.JsonSerializer .Serialize( listRelativeFile,  CopyFileSettings   ,  options)
             System.IO.File .WriteAllText( this.FilelistJsonPath,  json)
        }
    }
    Clear() {
         this.DictFileAction.Clear()
         this.DictFileContent.Clear()
    }
#endif
