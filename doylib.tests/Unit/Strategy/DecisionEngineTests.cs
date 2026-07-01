using AutoFixture;
using AutoFixture.AutoMoq;
using doylib.Ai.Interfaces;
using doylib.Strategy;
using doylib.Strategy.Interfaces;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using Moq;

namespace doylib.tests.Unit.Strategy;

[TestClass]
public class DecisionEngineTests
{
    private IFixture mFixture = null!;
    private Mock<IAiInferenceService> mAiMock = null!;
    private DoyLibSettings mSettings = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mFixture = new Fixture().Customize(new AutoMoqCustomization());
        mAiMock = mFixture.Freeze<Mock<IAiInferenceService>>();
        mSettings = BuildSettings(quorum: 0.5);
    }

    [TestMethod]
    public void Evaluate_NoModulesRegistered_ReturnsNone()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.NONE, result);
    }

    [TestMethod]
    public void Register_PlainStrategyModule_NeverInvokesAttachAi()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings, mAiMock.Object);
        var module = BuildModuleMock("M1", TradeAction.BUY);

        // Act
        sut.Register(module.Object);

        // Assert
        mAiMock.Verify(a => a.GetSession(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void Register_AiModule_WhenAiServiceProvided_CallsAttachAi()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings, mAiMock.Object);
        var aiModule = new Mock<IAiStrategyModule>();
        aiModule.SetupGet(m => m.Name).Returns("AiM");

        // Act
        sut.Register(aiModule.Object);

        // Assert
        aiModule.Verify(m => m.AttachAi(mAiMock.Object), Times.Once);
    }

    [TestMethod]
    public void Register_AiModule_WhenAiServiceNull_DoesNotCallAttachAi()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings, ai: null);
        var aiModule = new Mock<IAiStrategyModule>();
        aiModule.SetupGet(m => m.Name).Returns("AiM");

        // Act
        sut.Register(aiModule.Object);

        // Assert
        aiModule.Verify(m => m.AttachAi(It.IsAny<IAiInferenceService>()), Times.Never);
    }

    [TestMethod]
    public void GetActiveModules_AfterRegister_ContainsAllNamesInOrder()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("M1", TradeAction.NONE).Object);
        sut.Register(BuildModuleMock("M2", TradeAction.NONE).Object);
        sut.Register(BuildModuleMock("M3", TradeAction.NONE).Object);

        // Act
        var names = sut.GetActiveModules();

        // Assert
        CollectionAssert.AreEqual(new[] { "M1", "M2", "M3" }, names);
    }

    [TestMethod]
    public void Evaluate_AllModulesAgree_AboveQuorum_ReturnsThatAction()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("A", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("B", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("C", TradeAction.BUY).Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.BUY, result);
    }

    [TestMethod]
    public void Evaluate_TopVoteJustAboveQuorum_ReturnsTopAction()
    {
        // Arrange — 2 of 3 BUY (0.666 > 0.5)
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("A", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("B", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("C", TradeAction.SELL).Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.BUY, result);
    }

    [TestMethod]
    public void Evaluate_TopVoteAtExactQuorum_ReturnsNone()
    {
        // Arrange — quorum is strictly > , so 1/2 BUY at quorum 0.5 returns NONE
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("A", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("B", TradeAction.SELL).Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.NONE, result);
    }

    [TestMethod]
    public void Evaluate_BelowQuorum_ReturnsNone()
    {
        // Arrange — 1/3 BUY, 1/3 SELL, 1/3 NONE; top count is 1, ratio 0.333 not > 0.5
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("A", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("B", TradeAction.SELL).Object);
        sut.Register(BuildModuleMock("C", TradeAction.NONE).Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.NONE, result);
    }

    [TestMethod]
    public void Evaluate_OneModuleThrows_TreatedAsNone_OthersStillCounted()
    {
        // Arrange — 2 BUY + 1 throwing; quorum 0.5 → 2/3 BUY wins
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildModuleMock("A", TradeAction.BUY).Object);
        sut.Register(BuildModuleMock("B", TradeAction.BUY).Object);
        sut.Register(BuildThrowingModuleMock("C").Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.BUY, result);
    }

    [TestMethod]
    public void Evaluate_AllModulesThrow_ReturnsNone()
    {
        // Arrange — all throw → all NONE → top is NONE; even if ratio > quorum,
        // NONE is the returned action.
        var sut = new DecisionEngine(mSettings);
        sut.Register(BuildThrowingModuleMock("A").Object);
        sut.Register(BuildThrowingModuleMock("B").Object);
        sut.Register(BuildThrowingModuleMock("C").Object);

        // Act
        var result = sut.Evaluate();

        // Assert
        Assert.AreEqual(TradeAction.NONE, result);
    }

    [TestMethod]
    public void Warmup_CallsWarmupOnAllRegisteredModules()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings);
        var a = BuildModuleMock("A", TradeAction.NONE);
        var b = BuildModuleMock("B", TradeAction.NONE);
        sut.Register(a.Object);
        sut.Register(b.Object);

        // Act
        sut.Warmup();

        // Assert
        a.Verify(m => m.Warmup(), Times.Once);
        b.Verify(m => m.Warmup(), Times.Once);
    }

    [TestMethod]
    public void Warmup_OneModuleThrows_OthersStillWarmedUp()
    {
        // Arrange
        var sut = new DecisionEngine(mSettings);
        var throwing = new Mock<IStrategyModule>();
        throwing.SetupGet(m => m.Name).Returns("Bad");
        throwing.Setup(m => m.Warmup()).Throws(new InvalidOperationException("boom"));
        var ok = BuildModuleMock("Good", TradeAction.NONE);
        sut.Register(throwing.Object);
        sut.Register(ok.Object);

        // Act
        sut.Warmup();

        // Assert
        ok.Verify(m => m.Warmup(), Times.Once);
    }

    #region Helpers

    private static DoyLibSettings BuildSettings(double quorum)
    {
        return new DoyLibSettings(
            Path: "/tmp/whatever",
            WarmupMethod: "none",
            PreloadCount: 0,
            Timeframe: "M1",
            Symbol: "EURUSD",
            Quorum: quorum,
            MaxCandleWindowSize: 100,
            Ai: null,
            ApiBaseUrl: "http://localhost:3000");
    }

    private static Mock<IStrategyModule> BuildModuleMock(string name, TradeAction evaluateResult)
    {
        var mock = new Mock<IStrategyModule>();
        mock.SetupGet(m => m.Name).Returns(name);
        mock.Setup(m => m.Evaluate()).Returns(evaluateResult);
        return mock;
    }

    private static Mock<IStrategyModule> BuildThrowingModuleMock(string name)
    {
        var mock = new Mock<IStrategyModule>();
        mock.SetupGet(m => m.Name).Returns(name);
        mock.Setup(m => m.Evaluate()).Throws(new InvalidOperationException("boom"));
        return mock;
    }

    #endregion
}
