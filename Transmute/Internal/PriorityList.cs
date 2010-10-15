using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Transmute.Internal
{
    public class PriorityList<TObjType> : IPriorityList<TObjType>
    {
        private static readonly IComparer<PrioritisedListEntry<TObjType>> Comparer = new PrioritisedListEntryComparer();

        private bool _listIsSorted;
        private int _insertionId = 0;
        private readonly List<PrioritisedListEntry<TObjType>> _entries = new List<PrioritisedListEntry<TObjType>>();
        private IEnumerable<TObjType> _sortedEnumerable;

        public PriorityList() {}

        public PriorityList(IPriorityList<TObjType> priorityList)
        {
            foreach (var entry in priorityList.GetPrioritisedList())
            {
                _entries.Add(entry);
                if (entry.InsertionOrder >= _insertionId)
                    _insertionId = entry.InsertionOrder+1;
            }
        }

        public void Add(TObjType entry)
        {
            Add(Priority.Normal, entry);
        }

        public void Add(Priority priority, TObjType entry)
        {
            _listIsSorted = false;
            _entries.Add(new PrioritisedListEntry<TObjType>(entry, _insertionId++, priority));
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public void RemoveOfType<TType>()
        {
            foreach(var entry in _entries.Where(e => e.Entry is TType).ToArray())
            {
                _entries.Remove(entry);
            }
        }

        public IEnumerable<PrioritisedListEntry<TObjType>> GetPrioritisedList()
        {
            return _entries;
        }

        public IEnumerator<TObjType> GetEnumerator()
        {
            if (!_listIsSorted)
            {
                _entries.Sort(Comparer);
                _sortedEnumerable = _entries.Select(e => e.Entry).ToArray();
                _listIsSorted = true;
            }
            return _sortedEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class PrioritisedListEntryComparer : IComparer<PrioritisedListEntry<TObjType>>
        {
            public int Compare(PrioritisedListEntry<TObjType> x, PrioritisedListEntry<TObjType> y)
            {
                if (x.Priority > y.Priority)
                    return 1;
                if(x.Priority == y.Priority)
                {
                    // x.InsertionOrder == y.InsertionOrder will only ever happen when comparing an object with itself
                    if (x.InsertionOrder == y.InsertionOrder)
                        return 0;
                    switch (x.Priority)
                    {
                        case Priority.Normal:
                        case Priority.RunLast:
                            // Insert first, return first
                            return x.InsertionOrder > y.InsertionOrder ? 1 : -1;
                        case Priority.RunFirst:
                            // Insert last, return first
                            return x.InsertionOrder < y.InsertionOrder ? 1 : -1;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                // if x.Priority < y.Priority
                return -1;
            }
        }
    }
}