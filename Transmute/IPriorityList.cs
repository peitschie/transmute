using System.Collections.Generic;
using Transmute.Internal;

namespace Transmute
{
    public enum Priority
    {
        RunFirst,
        Normal,
        RunLast
    }

    public interface IPriorityList<TObjType> : IEnumerable<TObjType>
    {
        void Add(TObjType entry);
        void Add(Priority priority, TObjType entry);
        void Clear();
        void RemoveOfType<TType>();
        IEnumerable<PrioritisedListEntry<TObjType>> GetPrioritisedList();
    }
}