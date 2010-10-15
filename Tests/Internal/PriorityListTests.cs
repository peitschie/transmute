using System.Linq;
using NUnit.Framework;
using Transmute.Internal;

namespace Transmute.Tests.Internal
{
    [TestFixture]
    public class PriorityListTests
    {
        private PriorityList<int> _priorityList;

        [SetUp]
        public void SetUp()
        {
            _priorityList = new PriorityList<int>();
        }

        [Test]
        public void Add_Normal_InsertOrderPreserved()
        {
            _priorityList.Add(1);
            _priorityList.Add(2);
            _priorityList.Add(3);
            Assert.AreEqual(new[] { 1, 2, 3 }, _priorityList.ToArray());
        }

        [Test]
        public void Add_RunFirst_AlwaysAtStartOfList()
        {
            _priorityList.Add(1);
            _priorityList.Add(Priority.RunFirst, 2);
            _priorityList.Add(3);
            Assert.AreEqual(new []{2, 1, 3}, _priorityList.ToArray());
        }

        [Test]
        public void Add_RunLast_AlwaysAtEndOfList()
        {
            _priorityList.Add(1);
            _priorityList.Add(Priority.RunLast, 2);
            _priorityList.Add(3);
            Assert.AreEqual(new[] { 1, 3, 2 }, _priorityList.ToArray());
        }

        [Test]
        public void Add_MixOfFirstAndLastAndNormal()
        {
            _priorityList.Add(1);
            _priorityList.Add(2);
            _priorityList.Add(Priority.RunFirst, 5);
            _priorityList.Add(3);
            _priorityList.Add(Priority.RunLast, 4);
            _priorityList.Add(Priority.RunFirst, 6);
            Assert.AreEqual(new[] { 6, 5, 1, 2, 3, 4 }, _priorityList.ToArray());
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            _priorityList.Add(1);
            _priorityList.Add(2);
            _priorityList.Add(Priority.RunFirst, 5);
            _priorityList.Add(3);
            _priorityList.Add(Priority.RunLast, 4);
            _priorityList.Add(Priority.RunFirst, 6);
            _priorityList.Clear();
            Assert.AreEqual(new int[0], _priorityList.ToArray());
        }

        [Test]
        public void Construct_CorrectlyReusesInsertOrder()
        {
            _priorityList.Add(1);
            _priorityList.Add(2);
            _priorityList.Add(3);
            var newPriorityList = new PriorityList<int>(_priorityList);
            Assert.AreEqual(new[] { 1, 2, 3 }, newPriorityList.ToArray());
            newPriorityList.Add(-4);
            Assert.AreEqual(new[] { 1, 2, 3, -4 }, newPriorityList.ToArray());
        }
    }
}