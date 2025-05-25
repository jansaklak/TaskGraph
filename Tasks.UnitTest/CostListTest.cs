using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasks.UnitTest;

[TestClass]
public class CostListTest {
    [TestMethod]
    public void CostList_Constructor_InitializesFields() {
        var cl = new CostList(2, 1, 2);
        Assert.IsNotNull(cl.GetTimes());
        Assert.IsNotNull(cl.GetGraph());
    }

    [TestMethod]
    public void CostList_AddTandomCompoundTask_Works() {
        var cl = new CostList(2, 1, 2);
       
        cl.GetWorkers().Add(new Worker(0));
        cl.GetWorkers().Add(new Worker(1));
        
        cl.GetTimes().LoadWkr(cl.GetWorkers());
        cl.GetTimes().SetRandomTimesAndCosts();
        
        int before = cl.GetTimes().TimeAndCosts.Count;
        cl.AddRandomCompoundTask();
        int after = cl.GetTimes().TimeAndCosts.Count;
        Assert.IsTrue(after > before);
    }

    [TestMethod]
    public void CostList_AssignEachToFastestWorker_Works() {
        var cl = new CostList(2, 2, 2);
        
        cl.GetWorkers().Add(new Worker(0));
        cl.GetWorkers().Add(new Worker(1));

        cl.GetTimes().LoadWkr(cl.GetWorkers());
        cl.GetTimes().SetRandomTimesAndCosts();

        cl.AssignEachToFastestWorker();
        Assert.IsTrue(cl.GetAssignments().Count > 0);
    }

    [TestMethod]
    public void CostList_AssignToCheaperstWorker_Works() {
        var cl = new CostList(2, 2, 2);

        cl.GetWorkers().Add(new Worker(0));
        cl.GetWorkers().Add(new Worker(1));
        
        cl.GetTimes().LoadWkr(cl.GetWorkers());
        cl.GetTimes().SetRandomTimesAndCosts();
        
        cl.AssignToCheapestWorker();
        Assert.IsTrue(cl.GetAssignments().Count > 0);
    }
}
