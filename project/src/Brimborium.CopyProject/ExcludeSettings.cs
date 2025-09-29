namespace Brimborium.CopyProject;

public sealed class ExcludeSettings {
    private readonly HashSet<string> _Excludes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _GlobalSettings = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _RepoSettings = new(StringComparer.OrdinalIgnoreCase);
    public ExcludeSettings(
        List<string>? globalSettings,
        List<string>? repoSettings) {
        if (globalSettings is { } gs) {
            foreach (var name in gs) {
                if (!string.IsNullOrEmpty(name)) {
                    this._Excludes.Add(name);
                    this._GlobalSettings.Add(name);
                }
            }
        }
        if (repoSettings is { } rs) {
            foreach (var name in rs) {
                if (!string.IsNullOrEmpty(name)) {
                    this._Excludes.Add(name);
                    this._RepoSettings.Add(name);
                }
            }
        }
    }

    public HashSet<string> Excludes => this._Excludes;

    public HashSet<string> GlobalSettings => this._GlobalSettings;

    public HashSet<string> RepoSettings => this._RepoSettings;

    private static readonly char[] _SplitChars = new char[] { '/', '\\'};
    public bool IsIncludedPath(string path) {
        var listPath = path.Split(_SplitChars);
        foreach (var currentPath in listPath) {
            if (this._Excludes.Contains(currentPath)) {
                return false;
            }
        }
        return true;
    }
}

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
