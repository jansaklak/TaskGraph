using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasks.UnitTest;

[TestClass]
public class GrafTest {
    [TestMethod]
    [DataRow(1, 2, 5)]
    [DataRow(11, 22, 3)]
    [DataRow(0, 32, 1)]
    [DataRow(5, 6, 5)]
    [DataRow(1, 10, 13)]
    public void Graf_AddEdge_And_CheckEdge_Works(int ver1, int ver2, int weight) {
        var graf = new Graf();
        graf.AddEdge(ver1, ver2, weight);
        Assert.IsTrue(graf.CheckEdge(ver1, ver2));
        Assert.IsFalse(graf.CheckEdge(ver2, ver1));
        Assert.AreEqual(weight, graf.GetWeightEdge(ver1, ver2));
    }

    [TestMethod]
    [DataRow(0, 2, 5, 12)]
    [DataRow(0, 22, 3, 4)]
    [DataRow(0, 32, 1, 2)]
    [DataRow(0, 4, 2, 50)]
    [DataRow(0, 10, 13, 11)]
    public void Graf_BFS_ReturnsCorrectOrder(int ver1, int ver2, int ver3, int ver4) {
        var graf = new Graf();
        graf.AddEdge(ver1, ver2);
        graf.AddEdge(ver2, ver3);
        graf.AddEdge(ver2, ver4);
        var bfs = graf.BFS();
        CollectionAssert.AreEqual(new List<int> { ver1, ver2, ver3, ver4 }, bfs);
    }

    [TestMethod]
    [DataRow(1, 2, 5)]
    [DataRow(11, 22, 3)]
    [DataRow(0, 32, 1)]
    [DataRow(5, 4, 2)]
    [DataRow(1, 10, 13)]
    public void Graf_DFS_ReturnsAllPaths(int ver1, int ver2, int ver3) {
        var graf = new Graf();
        graf.AddEdge(ver1, ver2);
        graf.AddEdge(ver2, ver3);
        var paths = graf.DFS(ver1, ver3);
        Assert.IsTrue(paths.Any(p => p.SequenceEqual(new List<int> { ver1, ver2, ver3 })));
    }
}
