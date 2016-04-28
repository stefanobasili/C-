using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TestDragDropTreeView
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        static int groupCounter_;
        static int cellCounter_;

        private Node_Base root_;
        private Point _lastMouseDown;
        private Node_Base sourceNode_;
        private Node_Base targetNode_;
        private Node_Base.enItemDropArea targetDropArea_;

        public Window1()
        {
            InitializeComponent();
        }

        #region properties
        /// <summary>
        ///     These are the items that will populate the TreeView.
        /// </summary>
        public Node_Base Root
        {
            get
            {
                if (root_ == null) {
                    CreateInitialData();
                }

                return root_;
            }
        }

        //public TreeViewItem SelectedTreeItem
        //{
        //    get { return (TreeViewItem)GetValue(SelectedTreeItemProperty); }
        //    set { SetValue(SelectedTreeItemProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SelectedTreeItem.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SelectedTreeItemProperty =
        //    DependencyProperty.Register("SelectedTreeItem", typeof(TreeViewItem), typeof(Window1), new PropertyMetadata(null));

        #region IsDragInProgress (Dependency Property)
        public bool IsDragInProgress
        {
            get { return (bool)GetValue(IsDragInProgressProperty); }
            set { SetValue(IsDragInProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragInProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragInProgressProperty =
            DependencyProperty.Register("IsDragInProgress", typeof(bool), typeof(Window), new PropertyMetadata(false));
        #endregion

        #region Scale (Dependency Property)

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(Window1), new PropertyMetadata(1.1));
        #endregion



        public Node_Base SelectedNode
        {
            get { return (Node_Base)GetValue(SelectedNodeProperty); }
            set { SetValue(SelectedNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register("SelectedNode", typeof(Node_Base), typeof(Window1), new PropertyMetadata(null));


        private Node_Base CurrentDropTarget { get; set; }
        #endregion

        /// <summary>
        ///     Creates sample data.
        /// </summary>
        private void CreateInitialData()
        {
            root_ = new Node_Base("Root");

            Node_Base source = new Node_Group("Source") { Index = 0 };
            for (int i = 0; i < 4; i++)
                source.Children.Add(new Node_Cell(GetNewCellName()));

            root_.Children.Add(source);

            Node_Base destination = new Node_Group("Destination") { Index = 1 };
            for (int i = 0; i < 4; i++)
                destination.Children.Add(new Node_Cell(GetNewCellName()));
            root_.Children.Add(destination);

            root_.UpdateIndexes();
        }
        private string GetNewCellName()
        {
            cellCounter_++;
            return cellCounter_.ToString();
        }
        private string GetNewGroupName()
        {
            groupCounter_++;
            return groupCounter_.ToString();
        }
        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                _lastMouseDown = e.GetPosition(TheTreeView);
            }
        }
        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point currentPosition = e.GetPosition(TheTreeView);

            // Note: This should be based on some accessibility number and not just 2 pixels
            if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 5.0) ||
                (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 5.0)) {
                Node_Base selectedItem = (Node_Base)TheTreeView.SelectedItem;

                sourceNode_ = selectedItem;
                Console.WriteLine("TreeView_MouseMove >>> selectedItem = " + selectedItem.Name);
                if (selectedItem == null)
                    return;

                //TreeViewItem container = (TreeViewItem)(TheTreeView.ItemContainerGenerator.ContainerFromIndex(TheTreeView.Items.CurrentPosition));
                TreeViewItem container = (TreeViewItem)(TheTreeView.ItemContainerGenerator.ContainerFromItem(sourceNode_));
                container = GetTreeViewItem(e.GetPosition(TheTreeView));

                if (container == null)
                    return;

                IsDragInProgress = true;
                DragDropEffects finalDropEffect = DragDrop.DoDragDrop(container, selectedItem, DragDropEffects.Move);
                IsDragInProgress = false;

                // check if we are trying to drop a node onto one of its children
                if (selectedItem.ContainsNode(targetNode_)) 
                    return;

                if (selectedItem == targetNode_)
                    return;

                // verificare che il target non sia figlio del selectedItem
                if ((finalDropEffect == DragDropEffects.Move) && (targetNode_ != null)) {
                    #region  A Move drop was accepted

                    selectedItem.Parent.Children.Remove(selectedItem);

                    switch (targetDropArea_) {
                        case Node_Base.enItemDropArea.Top:
                            targetNode_.Parent.Children.Insert(targetNode_.Index, selectedItem);
                            break;
                        case Node_Base.enItemDropArea.Center:
                            targetNode_.Children.Add(selectedItem);
                            break;
                        case Node_Base.enItemDropArea.Bottom:
                            targetNode_.Parent.Children.Insert(targetNode_.Index + 1, selectedItem);
                            break;
                        default:
                            break;
                    }
                    SelectNode(selectedItem);
                    #endregion
                }
                root_.UpdateIndexes();

                sourceNode_ = null;
                targetNode_ = null;
                IsDragInProgress = false;
            }
        }
        private TreeViewItem GetContainerFromNode(Node_Base node)
        {
            Stack<Node_Base> _stack = new Stack<Node_Base>();
            _stack.Push(node);
            Node_Base parent = node.Parent;

            while (parent != null) {
                _stack.Push(parent);
                parent = parent.Parent;
            }

            ItemsControl container = TheTreeView;
            while ((_stack.Count > 0) && (container != null)) {
                Node_Base top = _stack.Pop();
                container = (ItemsControl)container.ItemContainerGenerator.ContainerFromItem(top);
            }

            return container as TreeViewItem;
        }
        private TreeViewItem GetTreeViewItem(Point position)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(TheTreeView, position); ;
            DependencyObject obj = hitResult.VisualHit;

            //(obj as TextBlock).Background = new SolidColorBrush(Colors.BlueViolet);
            //(obj as TextBlock).Height = 40;
            do {
                obj = VisualTreeHelper.GetParent(obj);

            } while (obj != null && obj.GetType() != typeof(TreeViewItem));

            return obj as TreeViewItem;
        }
        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null)) {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }

            return container;
        }

        private void TheTreeView_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            //if (e.OriginalSource is Border) {
            //    Console.WriteLine("Dropped on Border " + (e.OriginalSource as Border).Name);
            //}
            // Verify that this is a valid drop and then store the drop target
            TreeViewItem container = GetNearestContainer(e.OriginalSource as UIElement);
            if (container != null) {
                Node_Base source = null;

                if (e.Data.GetDataPresent(typeof(Node_Cell))) {
                    source = e.Data.GetData(typeof(Node_Cell)) as Node_Base;
                } else if (e.Data.GetDataPresent(typeof(Node_Group))) {
                    source = e.Data.GetData(typeof(Node_Group)) as Node_Group;
                }

                Node_Base targetNode = (Node_Base)container.Header;
                if ((source != null) && (targetNode != null)) {
                    targetNode_ = targetNode;

                    string name = (e.OriginalSource as FrameworkElement).Name;

                    if (FindAncestorByName(e.OriginalSource as UIElement, "PART_TopDropArea") != null) {
                        targetDropArea_ = Node_Base.enItemDropArea.Top;
                    } else if (FindAncestorByName(e.OriginalSource as UIElement, "PART_BottomDropArea") != null) {
                        targetDropArea_ = Node_Base.enItemDropArea.Bottom;
                    } else {
                        targetDropArea_ = Node_Base.enItemDropArea.Center;
                    }



                    e.Effects = DragDropEffects.Move;
                }
            }
        }

        private void TheTreeView_CheckDropTarget(object sender, DragEventArgs e)
        {
            if (!IsValidDropTarget(e.OriginalSource as UIElement)) {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private bool IsValidDropTarget(UIElement target)
        {
            FrameworkElement dropTarget;

            if (target == null) {
                SwitchDropTarget(null);
                Console.WriteLine("IsValidDropTarget ====> target is NULL");
                return false;
            }

            TreeViewItem container = GetNearestContainer(target);
            if (container == null) {
                SwitchDropTarget(null);
                Console.WriteLine("IsValidDropTarget ====> container is NULL");
                return false;
            }
            Node_Base dataTarget = container.Header as Node_Base;

            if (dataTarget == null) {
                SwitchDropTarget(null);
                Console.WriteLine("IsValidDropTarget ====> dataTarget is null");
                return false;
            }

            /*
                if (CurrentDropTarget != null)
                    CurrentDropTarget.DropArea = Node_Base.enItemDropArea.None;

                CurrentDropTarget = source;
                CurrentDropTarget.DropArea = targetDropArea_;
            */
            #region check PART_TopDropArea
            dropTarget = FindAncestorByName(target, "PART_TopDropArea");
            if (dropTarget != null) {
                dataTarget.DropArea = Node_Base.enItemDropArea.Top;
                Console.WriteLine("IsValidDropTarget ====> Hovering on PART_TopDropArea");
                SwitchDropTarget(dataTarget);
                return true;
            }
            #endregion

            #region check PART_BottomDropArea
            dropTarget = FindAncestorByName(target, "PART_BottomDropArea");
            if (dropTarget != null) {
                dataTarget.DropArea = Node_Base.enItemDropArea.Bottom;
                Console.WriteLine("IsValidDropTarget ====> Hovering on PART_BottomDropArea");
                SwitchDropTarget(dataTarget);
                return true;
            }
            #endregion

            //TreeViewItem container = GetNearestContainer(target);

            if (container == null) {
                Console.WriteLine("IsValidDropTarget ====> container is NULL");
                SwitchDropTarget(null);
                return false;
            }

            if (sourceNode_.Children.Contains(targetNode_)) {
                Console.WriteLine("IsValidDropTarget ====> cannot drop parent on child");
                SwitchDropTarget(null);
                return false;
            }

            //Node_Base dataTarget = container.Header as Node_Base;

            if (sourceNode_ == dataTarget) {
                Console.WriteLine("IsValidDropTarget ====> itself");
                SwitchDropTarget(null);
                return false;
            }
            if (dataTarget is Node_Cell) {
                Console.WriteLine("IsValidDropTarget ====> cannot drop over cell");
                SwitchDropTarget(null);
                return false;
            }

            if (dataTarget is Node_Group) {
                dataTarget.DropArea = Node_Base.enItemDropArea.Center;
                SwitchDropTarget(dataTarget);
                dataTarget.IsExpanded = true;
            } else {
                dataTarget.DropArea = Node_Base.enItemDropArea.None;
                SwitchDropTarget(null);
            }

            Console.WriteLine("IsValidDropTarget ====> Hovering on " + dataTarget.Name);

            return (dataTarget is Node_Group);
        }

        private void SwitchDropTarget(Node_Base newTarget)
        {
            if (newTarget == CurrentDropTarget)
                return;

            if (CurrentDropTarget != null)
                CurrentDropTarget.DropArea = Node_Base.enItemDropArea.None;

            CurrentDropTarget = newTarget;
        }

        private string GetUIElementPath(UIElement target)
        {
            FrameworkElement fr = target as FrameworkElement;
            string ret = "";

            while (fr != null) {
                ret = "\\" + fr.GetType().Name + "[" + fr.Name + "]" + ret;
                fr = VisualTreeHelper.GetParent(fr) as FrameworkElement;
            }

            return ret;
        }
        private FrameworkElement FindAncestorByName(UIElement target, string name)
        {
            FrameworkElement fr = target as FrameworkElement;

            //Console.WriteLine("CHECKING : " + GetUIElementPath(target));
            while (fr != null) {
                if (fr.Name == name)
                    return fr;
                fr = VisualTreeHelper.GetParent(fr) as FrameworkElement;
            }

            return null;
        }

        private void btnAddGroup__Click(object sender, RoutedEventArgs e)
        {
            Node_Base node = TheTreeView.SelectedItem as Node_Base;

            if (node != null && node is Node_Group) {
                node.Children.Add(new Node_Group((++groupCounter_).ToString()));
            }
        }
        private void btnAddCell__Click(object sender, RoutedEventArgs e)
        {
            Node_Base node = TheTreeView.SelectedItem as Node_Base;

            if (node != null && node is Node_Group) {
                node.Children.Add(new Node_Cell(GetNewCellName()));
            }
        }

        private void PART_TopDropAreaIndicator_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine(".......................................................DRAG_ENTER");
        }

        private void PART_TopDropAreaIndicator_DragLeave(object sender, DragEventArgs e)
        {
            Console.WriteLine(".......................................................DRAG_LEAVE");
        }

        private void TreeView_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void TheWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void TheWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void TheTreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                Scale += e.Delta / 100;
        }

        private void TheTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Node_Base newSelection = e.NewValue as Node_Base;

            SelectNode(newSelection);

            //if (SelectedNode != null && SelectedNode != newSelection) { 
            //    SelectedNode.IsSelected = false;
            //}

            //SelectedNode = newSelection;
            //if (SelectedNode != null)
            //    SelectedNode.IsSelected = true;
        }

        private void SelectNode(Node_Base node)
        {
            if (SelectedNode != null && SelectedNode != node) {
                SelectedNode.IsSelected = false;
            }

            SelectedNode = node;
            if (SelectedNode != null)
                SelectedNode.IsSelected = true;
        }
    }
}
