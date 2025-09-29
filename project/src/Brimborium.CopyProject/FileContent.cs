namespace Brimborium.CopyProject;

public sealed class FileContent {
    public FileContent(string relativePath, string content) {
        this.RelativePath = relativePath;
        this.Content = content;
    }

    public string RelativePath { get; }

    public string Content { get; }

    public static FileContent Create(string relativePath, string fullPath, string content = "") {
        if (string.IsNullOrEmpty(content)) {

            if (System.IO.File.Exists(fullPath)) {
                string contentRead = System.IO.File.ReadAllText(fullPath);
                return new(relativePath, contentRead);
            } else {
                return new(relativePath, "");
            }
        } else {
            return new(relativePath, content);
        }
    }
}
