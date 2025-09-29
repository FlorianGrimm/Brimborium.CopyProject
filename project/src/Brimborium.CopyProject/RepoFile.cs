namespace Brimborium.CopyProject;

public interface IRepoFile {
    string RelativePath { get; }
    CopyFileSettingsAction ActionE { get; }
}

public sealed class RepoFile : IRepoFile {
    public RepoFile(
        string relativePath, 
        CopyFileSettingsAction action) {
        this.RelativePath = relativePath;
        this.ActionE = action;
    }

    public string RelativePath { get; }

    public CopyFileSettingsAction ActionE { get; set; }
}