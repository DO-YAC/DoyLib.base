using doylib.Services.Interfaces;
using doylib.Strategy.Modules;
using DoyVestment.Framework.Models.Enums;
using Moq;

namespace doylib.tests.Unit.Strategy.Modules;

[TestClass]
public class ExampleModuleTests
{
    [TestMethod]
    public void Name_IsExampleModule()
    {
        var sut = new ExampleModule(Mock.Of<ICandleWindowService>());

        Assert.AreEqual("ExampleModule", sut.Name);
    }

    [TestMethod]
    public void Evaluate_ReturnsOneOfNoneBuyOrSell()
    {
        var sut = new ExampleModule(Mock.Of<ICandleWindowService>());

        for (int i = 0; i < 25; i++)
        {
            var action = sut.Evaluate();
            Assert.IsTrue(
                action is TradeAction.NONE or TradeAction.BUY or TradeAction.SELL,
                $"Unexpected action: {action}");
        }
    }

    [TestMethod]
    public void Warmup_DoesNotThrow()
    {
        var sut = new ExampleModule(Mock.Of<ICandleWindowService>());

        sut.Warmup();
    }
}
