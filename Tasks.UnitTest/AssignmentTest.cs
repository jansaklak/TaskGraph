using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasks.UnitTest;

[TestClass]
public class AssignmentTest {
    [TestMethod]
    public void Assignment_AddAndRemoveTask_Works() {
        var wkr = new Worker(0);
        var inst = new Assignment(2, wkr);

        inst.AddTask(3);
        Assert.IsTrue(inst.GetTaskSet().Contains(3));
        inst.RemoveTask(3);
        Assert.IsFalse(inst.GetTaskSet().Contains(3));
    }

    [TestMethod]
    public void Assignment_CompareTo_Works() {
        var wkr1 = new Worker(1);
        var wkr2 = new Worker(2);

        var a1 = new Assignment(1, wkr1);
        var a2 = new Assignment(2, wkr1);
        var a3 = new Assignment(1, wkr2);

        Assert.IsTrue(a1.CompareTo(a2) < 0);
        Assert.IsTrue(a2.CompareTo(a1) > 1);
        Assert.IsTrue(a1.CompareTo(a3) < 0);
    }
}
