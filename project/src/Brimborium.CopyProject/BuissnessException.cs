namespace Brimborium.CopyProject;

[Serializable]
public class BuissnessException : Exception {
    public BuissnessException() {
    }

    public BuissnessException(string? message) : base(message) {
    }

    public BuissnessException(string? message, Exception? innerException) : base(message, innerException) {
    }
}