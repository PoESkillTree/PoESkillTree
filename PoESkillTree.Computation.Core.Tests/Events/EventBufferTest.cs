using System.Collections.Generic;
using System.Linq;
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
            var bufferable = CreateImmediatelyInvokedEvent(1);

            sut.Buffer(bufferable.Object, 1);

            bufferable.Verify();
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
            var bufferable = CreateImmediatelyInvokedEvent(1);
            sut.StartBuffering();

            sut.StopBuffering();
            sut.Buffer(bufferable.Object, 1);

            bufferable.Verify();
        }

        [Test]
        public void FlushInvokesBufferedCall()
        {
            var sut = new EventBuffer();
            var bufferable = CreateBufferInvokedEvent(1);
            sut.StartBuffering();

            sut.Buffer(bufferable.Object, 1);
            sut.Flush();

            bufferable.Verify();
        }

        [Test]
        public void FlushMergesCallsWithSameSender()
        {
            var sut = new EventBuffer();
            var bufferable = CreateBufferInvokedEvent(1, 2);
            sut.StartBuffering();

            sut.Buffer(bufferable.Object, 1);
            sut.Buffer(bufferable.Object, 2);
            sut.Flush();

            bufferable.Verify();
        }

        [Test]
        public void FlushKeepsCallsWithDifferentSender()
        {
            var sut = new EventBuffer();
            var bufferable1 = CreateBufferInvokedEvent(1);
            var bufferable2 = CreateBufferInvokedEvent(2);
            sut.StartBuffering();

            sut.Buffer(bufferable1.Object, 1);
            sut.Buffer(bufferable2.Object, 2);
            sut.Flush();
            
            bufferable1.Verify();
            bufferable2.Verify();
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

        [Test]
        public void FlushLoopsOverBuffers()
        {
            var sut = new EventBuffer();
            var bufferable1 = CreateBufferInvokedEvent(1);
            var bufferable2 = CreateBufferInvokedEvent(2);
            bufferable1.Setup(b => b.Invoke(It.IsAny<List<int>>()))
                .Callback(() => sut.Buffer(bufferable2.Object, 2));
            sut.StartBuffering();

            sut.Buffer(bufferable1.Object, 1);
            sut.Flush();
            
            bufferable1.Verify();
            bufferable2.Verify();
        }

        private static Mock<IBufferableEvent<T>> CreateImmediatelyInvokedEvent<T>(T invokeArgs)
        {
            var bufferable = new Mock<IBufferableEvent<T>>();
            bufferable.Setup(b => b.Invoke(It.IsAny<T>()))
                .Callback((T t) => Assert.AreEqual(invokeArgs, t))
                .Verifiable();
            return bufferable;
        }

        private static Mock<IBufferableEvent<T>> CreateBufferInvokedEvent<T>(params T[] invokeArgs)
        {
            var bufferable = new Mock<IBufferableEvent<T>>();
            bufferable.Setup(b => b.Invoke(It.IsAny<List<T>>()))
                .Callback((List<T> t) => Assert.AreEqual(invokeArgs, t))
                .Verifiable();
            return bufferable;
        }

        private static void VerifyInvoke<T>(Mock<IBufferableEvent<T>> mock, params T[] invokeArgs)
            => mock.Verify(b => b.Invoke(invokeArgs.ToList()));
    }
}