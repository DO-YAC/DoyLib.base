using doylib.Ai;
using DoyVestment.Framework.Models;

namespace doylib.tests.Unit.Ai;

[TestClass]
public class OnnxInferenceServiceTests
{
    private OnnxInferenceService mSut = null!;
    private AiSettings mSettings = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mSettings = BuildSettings(models: new List<AiModelSettings>());
    }

    [TestCleanup]
    public void TestCleanup()
    {
        mSut?.Dispose();
    }

    [TestMethod]
    public void Constructor_WhenSettingsHasNoModels_LoadedModelsIsEmpty()
    {
        // Act
        mSut = new OnnxInferenceService(mSettings);

        // Assert
        Assert.AreEqual(0, mSut.LoadedModels.Count);
    }

    [TestMethod]
    public void Constructor_WhenModelFileMissing_ThrowsAndDoesNotSwallow()
    {
        // Arrange
        var settings = BuildSettings(models: new List<AiModelSettings>
        {
            new("missing", "/nonexistent/path/to/model.onnx", WarmupOnLoad: false, ProviderOverrides: null)
        });

        // Act + Assert
        Assert.Throws<System.Exception>(() => mSut = new OnnxInferenceService(settings));
    }

    [TestMethod]
    public void GetSession_WhenNameNotLoaded_ThrowsKeyNotFoundWithAvailableList()
    {
        // Arrange
        mSut = new OnnxInferenceService(mSettings);

        // Act + Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => mSut.GetSession("nope"));

        StringAssert.Contains(ex.Message, "nope");
        StringAssert.Contains(ex.Message, "Available:");
    }

    [TestMethod]
    public void LoadedModels_OnEmptyService_IsEmptyCollection()
    {
        // Arrange
        mSut = new OnnxInferenceService(mSettings);

        // Act
        var loaded = mSut.LoadedModels;

        // Assert
        Assert.IsNotNull(loaded);
        Assert.AreEqual(0, loaded.Count);
    }

    [TestMethod]
    public void Dispose_OnEmptyService_DoesNotThrow()
    {
        // Arrange
        var sut = new OnnxInferenceService(mSettings);

        // Act + Assert
        sut.Dispose();
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var sut = new OnnxInferenceService(mSettings);

        // Act + Assert
        sut.Dispose();
        sut.Dispose();
    }

    #region Helpers

    private static AiSettings BuildSettings(
        List<AiModelSettings> models,
        AiExecutionProvider provider = AiExecutionProvider.CPU,
        bool allowCpuFallback = true,
        string graphOptimizationLevel = "ALL")
    {
        return new AiSettings(
            Enabled: true,
            ExecutionProvider: provider,
            AllowCpuFallback: allowCpuFallback,
            GraphOptimizationLevel: graphOptimizationLevel,
            Models: models);
    }

    #endregion
}
