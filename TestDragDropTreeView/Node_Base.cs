using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace TestDragDropTreeView
{
    public class Node_Base : INotifyPropertyChanged
    {
        #region protected members
        protected string name_;
        protected int index_;
        protected bool isFirstChild_;
        protected bool isTopLevelAndFirstChild_;
        protected bool isTopLevelAndLastChild_;
        protected bool isLastChild_;
        protected bool isExpanded_;
        protected bool isSelected_;
        protected int depth_;
        protected Node_Base parent_;
        protected enItemDropArea dropArea_ = enItemDropArea.None;
        protected ObservableCollection<Node_Base> children_;
        #endregion

        public enum enItemDropArea
        {
            None,
            Top,
            Center,
            Bottom,
        }

        #region constructors
        public Node_Base(string name)
        {
            Name = name;
        }
        #endregion

        #region properties
        public string Name
        {
            get
            {
                return name_;
            }
            set
            {
                name_ = value;
                NotifyPropertyChanged("Name");
            }
        }
        public int Index
        {
            get { return index_; }
            set
            {
                index_ = value;
                NotifyPropertyChanged("Index");
            }
        }
        public bool IsFirstChild
        {
            get { return isFirstChild_; }
            set
            {
                if (isFirstChild_ != value) { 
                    isFirstChild_ = value;
                    NotifyPropertyChanged("IsFirstChild");
                }
            }
        }
        public bool IsTopLevelAndFirstChild
        {
            get { return isTopLevelAndFirstChild_; }
            set
            {
                isTopLevelAndFirstChild_ = value;
                NotifyPropertyChanged("IsTopLevelAndFirstChild");
            }
        }
        public bool IsTopLevelAndLastChild
        {
            get { return isTopLevelAndLastChild_; }
            set
            {
                isTopLevelAndLastChild_ = value;
                NotifyPropertyChanged("IsTopLevelAndLastChild");
            }
        }
        public bool IsLastChild
        {
            get { return isLastChild_; }
            set
            {
                isLastChild_ = value;
                NotifyPropertyChanged("IsLastChild");
            }
        }
        public int Depth
        {
            get { return depth_; }
            set
            {
                if (depth_ != value) {
                    depth_ = value;

                    foreach (Node_Base n in Children)
                        n.Depth = depth_ + 1;

                    NotifyPropertyChanged("Depth");
                }
            }
        }
        public Thickness Margin { get { return new Thickness(Depth * 4, 0, 0, 0); } }
        public enItemDropArea DropArea
        {
            get { return dropArea_; }
            set
            {
                if (dropArea_ != value) {
                    dropArea_ = value;
                    NotifyPropertyChanged("DropArea");
                }
            }
        }
        public Node_Base Parent
        {
            get { return parent_; }
            set
            {
                if (value == null)
                    Console.WriteLine(Name + " ===> parent set to null");
                else
                    Console.WriteLine(Name + " ===> parent set to " + value.Name);
                parent_ = value;
                NotifyPropertyChanged("Parent");
            }
        }
        public bool IsGroup { get; protected set; }
        public bool IsCell { get; protected set; }
        /// <summary>
        ///     The child data for this item.
        /// </summary>
        public ObservableCollection<Node_Base> Children
        {
            get
            {
                if (children_ == null) {
                    children_ = new ObservableCollection<Node_Base>();
                    children_.CollectionChanged += new NotifyCollectionChangedEventHandler(OnChildrenCollectionChanged);
                }

                return children_;
            }
        }
        public bool IsExpanded 
        {
            get { return isExpanded_; }
            set
            {
                isExpanded_ = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }
        public bool IsSelected
        {
            get { return isSelected_; }
            set
            {
                isSelected_ = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        #endregion

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Note: This section does not account for multiple items being involved in change operations.
            // Note: This section does not account for the replace operation.
            if (e.Action == NotifyCollectionChangedAction.Add) {
                Node_Base newNode = (Node_Base)e.NewItems[0];
                newNode.Parent = this;
                newNode.Depth = this.Depth + 1;

                Console.WriteLine("Node: " + this.Name + ">>> added child " + newNode.Name);
            } else if (e.Action == NotifyCollectionChangedAction.Remove) {
                Node_Base removedNode = (Node_Base)e.OldItems[0];

                Console.WriteLine("Node: " + this.Name + ">>> removed child " + removedNode.Name);

                if (removedNode.Parent == this)
                    removedNode.Parent = null;
                else
                    throw new InvalidOperationException();
            } else {
                throw new InvalidOperationException();
            }

            UpdateIndexes();
        }
        public void UpdateIndexes()
        {
            UpdateIndexes(Depth);
        }
        public void UpdateIndexes(int depth)
        {
            // re-assign indexes
            int index = 0;

            this.Depth = depth;

            foreach (Node_Base n in Children) {
                n.Index = index;
                n.IsFirstChild = (index == 0);
                n.IsTopLevelAndFirstChild = (index == 0) && (n.Depth <= 1);
                n.IsTopLevelAndLastChild = (index == Children.Count -1) && (n.Depth <= 1);
                n.IsLastChild = (index == (Children.Count - 1));
                n.UpdateIndexes(Depth + 1);
                index++;
            }
        }

        public bool ContainsNode(Node_Base targetNode_)
        {
            foreach(Node_Base child in Children) {
                if (child == targetNode_)
                    return true;

                if (child.ContainsNode(targetNode_))
                    return true;
            }

            return false;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
