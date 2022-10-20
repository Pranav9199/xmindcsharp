using System.Collections.Generic;

namespace XMindAPI.Core
{
    public interface INote
    {
        void AddNotes(List<KeyValuePair<string,string>> keyValuess);
    }
}