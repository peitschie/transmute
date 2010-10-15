namespace Transmute.Internal
{
    public class PrioritisedListEntry<TObjType>
    {
        public PrioritisedListEntry(TObjType entry, int insertionOrder, Priority priority)
        {
            _entry = entry;
            _insertionOrder = insertionOrder;
            _priority = priority;
        }

        private readonly TObjType _entry;
        public TObjType Entry
        {
            get { return _entry; }
        }

        private readonly int _insertionOrder;
        public int InsertionOrder
        {
            get { return _insertionOrder; }
        }

        private readonly Priority _priority;
        public Priority Priority
        {
            get { return _priority; }
        }
    }
}