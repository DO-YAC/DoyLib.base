using doylib.Ai;
using DoyVestment.Framework.Models;

namespace doylib.tests.Integration.Ai;

[TestClass]
public class OnnxInferenceServiceIntegrationTests
{
    private const string FixtureRelativePath = "TestAssets/identity.onnx";
    private const string FixtureModelName = "identity";

    private OnnxInferenceService mSut = null!;

    [TestCleanup]
    public void TestCleanup()
    {
        mSut?.Dispose();
    }

    [TestMethod]
    public void LoadModel_FromValidFile_SessionAvailable()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings());

        CollectionAssert.Contains(mSut.LoadedModels.ToArray(), FixtureModelName);
        Assert.IsNotNull(mSut.GetSession(FixtureModelName));
    }

    [TestMethod]
    public void GetSession_AfterLoad_NameMatches()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings());
        var session = mSut.GetSession(FixtureModelName);

        Assert.AreEqual(FixtureModelName, session.Name);
    }

    [TestMethod]
    public void Run_IdentityModel_ReturnsInputUnchanged()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings());
        var session = mSut.GetSession(FixtureModelName);

        var input = new float[] { 1f, 2f, 3f, 4f };
        var shape = new long[] { input.Length };

        var output = session.Run("input", input, shape);

        CollectionAssert.AreEqual(input, output);
    }

    [TestMethod]
    public void Run_IdentityModel_DifferentShapeOnSecondCall_StillCorrect()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings());
        var session = mSut.GetSession(FixtureModelName);

        var first = new float[] { 7f, 8f };
        var firstOut = session.Run("input", first, new long[] { first.Length });
        CollectionAssert.AreEqual(first, firstOut);

        var second = new float[] { 1f, 2f, 3f, 4f, 5f };
        var secondOut = session.Run("input", second, new long[] { second.Length });
        CollectionAssert.AreEqual(second, secondOut);
    }

    [TestMethod]
    public void Warmup_OnLoad_DoesNotThrow()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings(warmupOnLoad: true));

        Assert.IsNotNull(mSut.GetSession(FixtureModelName));
    }

    [TestMethod]
    public void MultipleModels_RegisteredUnderDifferentNames_LoadIndependently()
    {
        EnsureFixturePresent();

        var settings = BuildSettings(models: new List<AiModelSettings>
        {
            new("identity_a", FixtureRelativePath, WarmupOnLoad: false, ProviderOverrides: null),
            new("identity_b", FixtureRelativePath, WarmupOnLoad: false, ProviderOverrides: null),
        });

        mSut = new OnnxInferenceService(settings);

        Assert.AreEqual(2, mSut.LoadedModels.Count);
        Assert.AreNotSame(mSut.GetSession("identity_a"), mSut.GetSession("identity_b"));
    }

    [TestMethod]
    public void Session_InputsAndOutputs_ReflectModelMetadata()
    {
        EnsureFixturePresent();

        mSut = new OnnxInferenceService(BuildSettings());
        var session = mSut.GetSession(FixtureModelName);

        Assert.IsTrue(session.Inputs.ContainsKey("input"));
        Assert.IsTrue(session.Outputs.ContainsKey("output"));
    }

    [TestMethod]
    public void Dispose_AfterLoad_ClearsLoadedModels()
    {
        EnsureFixturePresent();

        var sut = new OnnxInferenceService(BuildSettings());
        Assert.AreEqual(1, sut.LoadedModels.Count);

        sut.Dispose();

        Assert.AreEqual(0, sut.LoadedModels.Count);
    }

    #region Helpers

    private static AiSettings BuildSettings(
        List<AiModelSettings>? models = null,
        bool warmupOnLoad = false,
        AiExecutionProvider provider = AiExecutionProvider.CPU,
        bool allowCpuFallback = true,
        string graphOptimizationLevel = "ALL")
    {
        models ??= new List<AiModelSettings>
        {
            new(FixtureModelName, FixtureRelativePath, warmupOnLoad, ProviderOverrides: null)
        };

        return new AiSettings(
            Enabled: true,
            ExecutionProvider: provider,
            AllowCpuFallback: allowCpuFallback,
            GraphOptimizationLevel: graphOptimizationLevel,
            Models: models);
    }

    private static void EnsureFixturePresent()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, FixtureRelativePath);
        if (!File.Exists(fullPath))
        {
            Assert.Inconclusive(
                $"ONNX fixture not found at '{fullPath}'. " +
                "Generate it by running: cd doylib.tests/TestAssets && pip install onnx && python3 build-identity.py");
        }
    }

    #endregion
}
