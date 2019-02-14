using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Tests.Events
{
    [TestFixture]
    public class EventBufferTest
    {
        [Test]
        public void BufferInvokesWhenNotBuffering()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();

            sut.Buffer(bufferable.Object, 1);

            VerifyInvoke(bufferable, 1);
        }

        [Test]
        public void BufferDoesNotInvokeWhenBuffering()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();

            sut.StartBuffering();
            sut.Buffer(bufferable.Object, 1);

            bufferable.VerifyNoOtherCalls();
        }

        [Test]
        public void BufferInvokesWhenBufferingStopped()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();
            sut.StartBuffering();

            sut.StopBuffering();
            sut.Buffer(bufferable.Object, 1);

            VerifyInvoke(bufferable, 1);
        }

        [Test]
        public void FlushInvokesBufferedCall()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();
            sut.StartBuffering();

            sut.Buffer(bufferable.Object, 1);
            sut.Flush();

            VerifyInvoke(bufferable, 1);
        }

        [Test]
        public void FlushMergesCallsWithSameSender()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();
            sut.StartBuffering();

            sut.Buffer(bufferable.Object, 1);
            sut.Buffer(bufferable.Object, 2);
            sut.Flush();

            VerifyInvoke(bufferable, 1, 2);
        }

        [Test]
        public void FlushKeepsCallsWithDifferentSender()
        {
            var sut = new EventBuffer();
            var bufferable1 = new Mock<IBufferableEvent<int>>();
            var bufferable2 = new Mock<IBufferableEvent<int>>();
            sut.StartBuffering();

            sut.Buffer(bufferable1.Object, 1);
            sut.Buffer(bufferable2.Object, 2);
            sut.Flush();

            VerifyInvoke(bufferable1, 1);
            VerifyInvoke(bufferable2, 2);
        }

        [Test]
        public void FlushClearsBuffer()
        {
            var sut = new EventBuffer();
            var bufferable = new Mock<IBufferableEvent<int>>();
            sut.StartBuffering();
            sut.Buffer(bufferable.Object, 1);
            sut.Flush();
            bufferable.Reset();

            sut.Flush();

            bufferable.VerifyNoOtherCalls();
        }

        private static void VerifyInvoke<T>(Mock<IBufferableEvent<T>> mock, params T[] invokeArgs)
            => mock.Verify(b => b.Invoke(invokeArgs));
    }
}