using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ListViewSample.Model
{
    public class GroupInfoList : ObservableCollection<object>
    {
        public object Key { get; set; }
    }
}
