using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDragDropTreeView
{
    public class Node_Cell: Node_Base
    {
        public Node_Cell(string name): base("CELL-" + name)
        {
            IsCell = true;
        }
        public override string ToString()
        {
            return "Cell:" + this.Name;
        }
    }
}
