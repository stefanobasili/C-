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
        private enItemDropArea targetDropArea_;

        public enum enItemDropArea
        {
            Top,
            Center,
            Bottom,
        }
        public Window1()
        {
            InitializeComponent();
        }

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



        public TreeViewItem SelectedTreeItem
        {
            get { return (TreeViewItem)GetValue(SelectedTreeItemProperty); }
            set { SetValue(SelectedTreeItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTreeItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTreeItemProperty =
            DependencyProperty.Register("SelectedTreeItem", typeof(TreeViewItem), typeof(Window1), new PropertyMetadata(null));


        /// <summary>
        ///     Creates sample data.
        /// </summary>
        private void CreateInitialData()
        {
            root_ = new Node_Base("Root");

            Node_Base source = new Node_Group("Source") { Index = 0 };
            root_.Children.Add(source);
            root_.Children.Add(new Node_Group("Destination") { Index = 1 });

            source.Children.Add(new Node_Cell(GetNewCellName()));
            source.Children.Add(new Node_Cell(GetNewCellName()));
            source.Children.Add(new Node_Cell(GetNewCellName()));
            source.Children.Add(new Node_Cell(GetNewCellName()));
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

        public bool IsDragInProgress
        {
            get { return (bool)GetValue(IsDragInProgressProperty); }
            set { SetValue(IsDragInProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragInProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragInProgressProperty =
            DependencyProperty.Register("IsDragInProgress", typeof(bool), typeof(Window), new PropertyMetadata(false));


        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                _lastMouseDown = e.GetPosition(TheTreeView);
            }
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Point currentPosition = e.GetPosition(TheTreeView);

                // Note: This should be based on some accessibility number and not just 2 pixels
                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 5.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 5.0)) {
                    Node_Base selectedItem = (Node_Base)TheTreeView.SelectedItem;

                    sourceNode_ = selectedItem;
                    Console.WriteLine("TreeView_MouseMove >>> selectedItem = " + selectedItem.Name);
                    if ((selectedItem != null) /*&& (selectedItem.Parent != null)*/) {
                        //TreeViewItem container = (TreeViewItem)(TheTreeView.ItemContainerGenerator.ContainerFromIndex(TheTreeView.Items.CurrentPosition));
                        TreeViewItem container = (TreeViewItem)(TheTreeView.ItemContainerGenerator.ContainerFromItem(sourceNode_));
                        container = GetTreeViewItem(e.GetPosition(TheTreeView));

                        if (container != null) {
                            IsDragInProgress = true;
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(container, selectedItem, DragDropEffects.Move);
                            IsDragInProgress = false;
                            if ((finalDropEffect == DragDropEffects.Move) && (targetNode_ != null)) {
                                // A Move drop was accepted

                                selectedItem.Parent.Children.Remove(selectedItem);

                                switch (targetDropArea_) {
                                    case enItemDropArea.Top:
                                        targetNode_.Parent.Children.Insert(targetNode_.Index, selectedItem);
                                        break;
                                    case enItemDropArea.Center:
                                        targetNode_.Children.Add(selectedItem);
                                        break;
                                    case enItemDropArea.Bottom:
                                        targetNode_.Parent.Children.Insert(targetNode_.Index + 1, selectedItem);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            sourceNode_ = null;
                            targetNode_ = null;
                            IsDragInProgress = false;
                        }
                    }
                }
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


                    if (FindAncestorByName(e.OriginalSource as UIElement, "PART_TopDropArea") != null)
                        targetDropArea_ = enItemDropArea.Top;
                    else if (FindAncestorByName(e.OriginalSource as UIElement, "PART_BottomDropArea") != null)
                        targetDropArea_ = enItemDropArea.Bottom;
                    else
                        targetDropArea_ = enItemDropArea.Center;
                    
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
                Console.WriteLine("IsValidDropTarget ====> target is NULL");
                return false;
            }

            #region check PART_TopDropArea
            dropTarget = FindAncestorByName(target, "PART_TopDropArea");
            if (dropTarget != null) {
                Console.WriteLine("IsValidDropTarget ====> Hovering on PART_TopDropArea");
                return true;
            }
            #endregion

            #region check PART_BottomDropArea
            dropTarget = FindAncestorByName(target, "PART_BottomDropArea");
            if (dropTarget != null) {
                Console.WriteLine("IsValidDropTarget ====> Hovering on PART_BottomDropArea");
                return true;
            }
            #endregion

            TreeViewItem container = GetNearestContainer(target);

            if (container == null) {
                Console.WriteLine("IsValidDropTarget ====> container is NULL");
                return false;
            }
            
            if (sourceNode_.Children.Contains(targetNode_)) {
                Console.WriteLine("IsValidDropTarget ====> cannot drop parent on child");
                return false;
            }
            
            Node_Base dataTarget = container.Header as Node_Base;

            if (sourceNode_ == dataTarget) {
                Console.WriteLine("IsValidDropTarget ====> itself");
                return false;
            }
            if (dataTarget is Node_Cell) {
                Console.WriteLine("IsValidDropTarget ====> cannot drop over cell");
                return false;
            }

            Console.WriteLine("IsValidDropTarget ====> Hovering on " + dataTarget.Name);

            return (dataTarget is Node_Group);
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
    }
}
