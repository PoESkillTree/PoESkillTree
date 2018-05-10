using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;

namespace PoESkillTree.Computation.Core.Tests.NodeCollections
{
    [TestFixture]
    public class PathDefinitionCollectionTest
    {
        [Test]
        public void AddAddsToDefaultView()
        {
            var path = PathDefinition.MainPath;
            var sut = CreateSut();

            sut.Add(path);

            CollectionAssert.Contains(sut.DefaultView, path);
        }

        [Test]
        public void AddAddsToSuspendableView()
        {
            var path = PathDefinition.MainPath;
            var sut = CreateSut();

            sut.Add(path);

            CollectionAssert.Contains(sut.SuspendableView, path);
        }

        [Test]
        public void RemoveRemovesFromViews()
        {
            var path = PathDefinition.MainPath;
            var sut = CreateSut();
            sut.Add(path);

            sut.Remove(path);

            CollectionAssert.IsEmpty(sut.DefaultView);
            CollectionAssert.IsEmpty(sut.SuspendableView);
        }

        [Test]
        public void RemoveDoesNotRemoveIfAddedTwice()
        {
            var path = PathDefinition.MainPath;
            var sut = CreateSut();
            sut.Add(path);
            sut.Add(path);

            sut.Remove(path);

            CollectionAssert.Contains(sut.DefaultView, path);
        }

        [Test]
        public void RemoveDoesNothingIfNotAdded()
        {
            var path = PathDefinition.MainPath;
            var sut = CreateSut();

            sut.Remove(path);
        }

        private static PathDefinitionCollection CreateSut() =>
            new PathDefinitionCollection(SuspendableEventViewProvider.Create(new ObservableCollection<PathDefinition>(),
                new SuspendableObservableCollection<PathDefinition>()));
    }
}