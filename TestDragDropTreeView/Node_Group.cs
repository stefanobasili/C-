using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDragDropTreeView
{
    public class Node_Group: Node_Base
    {
        public Node_Group(string name): base("GRP-" + name)
        {
            IsGroup = true;
        }

        public override string ToString()
        {
            return "Group:" + this.Name;
        }
    }
}
