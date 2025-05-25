using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasks.UnitTest;

[TestClass]
public sealed class EdgeTest {
    [TestMethod]
    [DataRow(1, 2, 5)]
    [DataRow(11, 22, 3)]
    [DataRow(0, 32, 1)]
    [DataRow(5, 6, 5)]
    [DataRow(1, 10, 13)]
    public void Edge_ConstructorAndGetter_WorkCorrectly(int ver1, int ver2, int weight) {
        var edge = new Edge(ver1, ver2, weight);
        Assert.AreEqual(ver1, edge.GetV1());
        Assert.AreEqual(ver2, edge.GetV2());
        Assert.AreEqual(weight, edge.GetWeight());
    }
}
