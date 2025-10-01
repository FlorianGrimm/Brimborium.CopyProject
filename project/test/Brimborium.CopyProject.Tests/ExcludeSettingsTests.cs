namespace Brimborium.CopyProject.Tests;

public class ExcludeSettingsTests {
    [Test]
    public async Task IsIncludedPathOnePartTest() {
        var sut = new ExcludeSettings(["abc"], ["123"]);
        await Assert.That(sut.IsIncludedPath("abc")).IsFalse();
        await Assert.That(sut.IsIncludedPath("abc/")).IsFalse();
        await Assert.That(sut.IsIncludedPath("/abc")).IsFalse();
        await Assert.That(sut.IsIncludedPath("/abc/")).IsFalse();

        await Assert.That(sut.IsIncludedPath("def")).IsTrue();
        await Assert.That(sut.IsIncludedPath("def/")).IsTrue();
        await Assert.That(sut.IsIncludedPath("/def")).IsTrue();
        await Assert.That(sut.IsIncludedPath("/def/")).IsTrue();

        await Assert.That(sut.IsIncludedPath("123")).IsFalse();
        await Assert.That(sut.IsIncludedPath("123/")).IsFalse();
        await Assert.That(sut.IsIncludedPath("/123")).IsFalse();
        await Assert.That(sut.IsIncludedPath("/123/")).IsFalse();

        await Assert.That(sut.IsIncludedPath("456")).IsTrue();
        await Assert.That(sut.IsIncludedPath("456/")).IsTrue();
        await Assert.That(sut.IsIncludedPath("/456")).IsTrue();
        await Assert.That(sut.IsIncludedPath("/456/")).IsTrue();
    }

    [Test]
    public async Task IsIncludedPathMultiPartTest() {
        var sut = new ExcludeSettings(["abc"], null);
        await Assert.That(sut.IsIncludedPath("123/abc")).IsFalse();
        await Assert.That(sut.IsIncludedPath("456/def")).IsTrue();
    }

}