using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Contains multiple tokens, separated by a semicolon. Tags can be used to find associated builds.
    /// </summary>
    public class TagsCollection : ICollection<string>
    {
        private readonly List<string> _tags = new List<string>();

        /// <summary>
        /// Splits provided string into separate tags and stores them.
        /// </summary>
        /// <param name="item">The string with tags.</param>
        public void Add(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                return;

            var tags = item
                .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim()).ToArray();

            for (var i = 0; i < tags.Count(); i++)
            {
                _tags.Add(tags[i]);
            }
        }

        #region Wrappers

        public bool IsReadOnly => false;

        public int Count => _tags.Count;

        public IEnumerator<string> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear() => _tags.Clear();

        public bool Contains(string item) => _tags.Contains(item);

        public void CopyTo(string[] array, int arrayIndex) => _tags.CopyTo(array, arrayIndex);

        public bool Remove(string item) => _tags.Remove(item);

        #endregion
    }
}