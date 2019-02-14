using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Events
{
    public class EventBuffer : IEventBuffer
    {
        private readonly Dictionary<object, ITypedEventBuffer> _typedBuffers =
            new Dictionary<object, ITypedEventBuffer>();

        private bool _buffering;

        public void Buffer<T>(IBufferableEvent<T> sender, T args)
        {
            if (!_buffering)
            {
                sender.Invoke(new[] { args });
                return;
            }

            var buffer = (TypedEventBuffer<T>) _typedBuffers.GetOrAdd(sender, _=> new TypedEventBuffer<T>(sender));
            buffer.Buffer(args);
        }

        public void StartBuffering()
            => _buffering = true;

        public void StopBuffering()
            => _buffering = false;

        public void Flush()
        {
            while (_typedBuffers.Any())
            {
                var buffers = _typedBuffers.Values.ToList();
                _typedBuffers.Clear();
                foreach (var typedBuffer in buffers)
                {
                    typedBuffer.Flush();
                }
            }
        }

        private interface ITypedEventBuffer
        {
            void Flush();
        }

        private class TypedEventBuffer<T> : ITypedEventBuffer
        {
            private readonly IBufferableEvent<T> _sender;
            private readonly List<T> _args = new List<T>();

            public TypedEventBuffer(IBufferableEvent<T> sender)
                => _sender = sender;

            public void Buffer(T args)
                => _args.Add(args);

            public void Flush()
            {
                _sender.Invoke(_args.ToList());
                _args.Clear();
            }
        }
    }
}