using NUnit.Framework;
using POESKillTree.TreeGenerator.Algorithm;

namespace PoESkillTree.Tests.TreeGenerator.Algorithm
{
    [TestFixture]
    public class LinkedListPriorityQueueTest
    {
        [Test]
        public void Test()
        {
            int[] queueTestOrder = { 10, 3, 11, 6, -3, 17, 13, -6, 2, 8, -2, -8 };

            var queue = new LinkedListPriorityQueue<TestNode>(30, queueTestOrder.Length);

            foreach (int t in queueTestOrder)
            {
                if (t > 0)
                    queue.Enqueue(new TestNode(t), (uint) t);
                if (t < 0)
                    Assert.IsTrue(queue.Dequeue().Priority == -t);
            }
        }

        private class TestNode
        {
            public int Priority { get; }

            public TestNode(int priority)
            {
                Priority = priority;
            }
        }
    }
}