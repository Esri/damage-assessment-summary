using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ConfigureSummaryReport.Model;

namespace ConfigureSummaryReport.Controls
{
    public class DraggableListView : ListView
    {
        Point ptMouseDown;
        bool canInitiateDrag;
        int indexToSelect;
        bool isDragInProgress;
        StringItems2 itemUnderDragCursor;
        ListView listView;

        public DraggableListView()
        {
            this.canInitiateDrag = false;
            this.indexToSelect = -1;
            ListView = this;
        }

        public ListView ListView
        {
            get { return listView; }
            set
            {
                //if (this.IsDragInProgress)
                //    throw new InvalidOperationException("Cannot set the ListView property during a drag operation.");

                if (this.listView != null)
                {
                    this.listView.PreviewMouseLeftButtonDown -= DraggableListView_PreviewMouseLeftButtonDown;
                    this.listView.PreviewMouseMove -= lv_PreviewMouseMove;
                    this.listView.DragOver -= DraggableListView_DragOver;
                    this.listView.Drop -= lv_Drop;
                }

                this.listView = value;

                if (this.listView != null)
                {
                    if (!this.listView.AllowDrop)
                        this.listView.AllowDrop = true;

                    this.listView.PreviewMouseLeftButtonDown += DraggableListView_PreviewMouseLeftButtonDown;
                    this.listView.PreviewMouseMove += lv_PreviewMouseMove;
                    this.listView.DragOver += DraggableListView_DragOver;
                    this.listView.Drop += lv_Drop;
                }
            }
        }

        void DraggableListView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            // Update the item which is known to be currently under the drag cursor.
            int index = this.IndexUnderDragCursor;
            this.ItemUnderDragCursor = index < 0 ? null : this.ListView.Items[index] as StringItems2;
        }

        void DraggableListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = this.IndexUnderDragCursor;
            this.canInitiateDrag = index > -1;

            if (this.canInitiateDrag)
            {
                // Remember the location and index of the ListViewItem the user clicked on for later.
                this.ptMouseDown = MouseUtilities.GetMousePosition(listView);
                this.indexToSelect = index;
            }
            else
            {
                this.ptMouseDown = new Point(-10000, -10000);
                this.indexToSelect = -1;
            }
        }

        public bool IsDragInProgress
        {
            get { return this.isDragInProgress; }
            private set { this.isDragInProgress = value; }
        }

        public event EventHandler<ProcessDropEventArgs<StringItems2>> ProcessDrop;

        void lv_Drop(object sender, DragEventArgs e)
        {
            if (this.ItemUnderDragCursor != null)
                this.ItemUnderDragCursor = null;

            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(typeof(StringItems2)))
                return;

            // Get the data object which was dropped.
            StringItems2 data = e.Data.GetData(typeof(StringItems2)) as StringItems2;
            if (data == null)
                return;

            // Get the ObservableCollection<ItemType> which contains the dropped data object.
            ObservableCollection<StringItems2> itemsSource = this.listView.ItemsSource as ObservableCollection<StringItems2>;
            if (itemsSource == null)
                throw new Exception(
                    "A ListView managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>.");

            int oldIndex = itemsSource.IndexOf(data);
            int newIndex = this.IndexUnderDragCursor;

            if (newIndex < 0)
            {
                // The drag started somewhere else, and our ListView is empty
                // so make the new item the first in the list.
                if (itemsSource.Count == 0)
                    newIndex = 0;

                // The drag started somewhere else, but our ListView has items
                // so make the new item the last in the list.
                else if (oldIndex < 0)
                    newIndex = itemsSource.Count;

                // The user is trying to drop an item from our ListView into
                // our ListView, but the mouse is not over an item, so don't
                // let them drop it.
                else
                    return;
            }

            // Dropping an item back onto itself is not considered an actual 'drop'.
            if (oldIndex == newIndex)
                return;

            if (this.ProcessDrop != null)
            {
                // Let the client code process the drop.
                ProcessDropEventArgs<StringItems2> args = new ProcessDropEventArgs<StringItems2>(itemsSource, data, oldIndex, newIndex, e.AllowedEffects);
                //this.ProcessDrop(this, args);
                this.ProcessDrop(sender, args);//TODO
                e.Effects = args.Effects;
            }
            else
            {
                // Move the dragged data object from it's original index to the
                // new index (according to where the mouse cursor is).  If it was
                // not previously in the ListBox, then insert the item.
                if (oldIndex > -1)
                    itemsSource.Move(oldIndex, newIndex);
                else
                    itemsSource.Insert(newIndex, data);

                // Set the Effects property so that the call to DoDragDrop will return 'Move'.
                e.Effects = DragDropEffects.Move;
            }
        }

        void lv_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanStartDragOperation)
                return;

            // Select the item the user clicked on.
            if (this.SelectedIndex != this.indexToSelect)
                this.SelectedIndex = this.indexToSelect;

            // If the item at the selected index is null, there's nothing
            // we can do, so just return;
            if (this.SelectedItem == null)
                return;

            ListViewItem itemToDrag = this.GetListViewItem(this.listView.SelectedIndex);
            if (itemToDrag == null)
                return;

            this.InitializeDragOperation(itemToDrag);
            this.PerformDragOperation();
            this.FinishDragOperation(itemToDrag);
        }

        bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return false;

                if (!this.canInitiateDrag)
                    return false;

                if (this.indexToSelect == -1)
                    return false;

                if (!this.HasCursorLeftDragThreshold)
                    return false;

                return true;
            }
        }

        void FinishDragOperation(ListViewItem draggedItem)
        {
            // Let the ListViewItem know that it is not being dragged anymore.
            ListViewItemDragState.SetIsBeingDragged(draggedItem, false);

            this.IsDragInProgress = false;

            if (this.ItemUnderDragCursor != null)
                this.ItemUnderDragCursor = null;
        }

        void InitializeDragOperation(ListViewItem itemToDrag)
        {
            // Set some flags used during the drag operation.
            this.IsDragInProgress = true;
            this.canInitiateDrag = false;

            // Let the ListViewItem know that it is being dragged.
            ListViewItemDragState.SetIsBeingDragged(itemToDrag, true);
        }

        bool _IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.
            if (target != null)
            {

                Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
                Point mousePos = MouseUtilities.GetMousePosition(target);

                return bounds.Contains(mousePos);
            }
            else
            {
                return false;
            }
        }

        bool HasCursorLeftDragThreshold
        {
            get
            {
                if (this.indexToSelect < 0)
                    return false;

                ListViewItem item = this.GetListViewItem(this.indexToSelect);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point ptInItem = this.TranslatePoint(this.ptMouseDown, item);

                // In case the cursor is at the very top or bottom of the ListViewItem
                // we want to make the vertical threshold very small so that dragging
                // over an adjacent item does not select it.
                double topOffset = Math.Abs(ptInItem.Y);
                double btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                double vertOffset = Math.Min(topOffset, btmOffset);

                double width = SystemParameters.MinimumHorizontalDragDistance * 2;
                double height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                Size szThreshold = new Size(width, height);

                Rect rect = new Rect(this.ptMouseDown, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                Point ptInListView = MouseUtilities.GetMousePosition(this);
                return !rect.Contains(ptInListView);
            }
        }

        StringItems2 ItemUnderDragCursor
        {
            get { return this.itemUnderDragCursor; }
            set
            {
                if (this.itemUnderDragCursor == value)
                    return;

                // The first pass handles the previous item under the cursor.
                // The second pass handles the new one.
                for (int i = 0; i < 2; ++i)
                {
                    if (i == 1)
                        this.itemUnderDragCursor = value;

                    if (this.itemUnderDragCursor != null)
                    {
                        ListViewItem listViewItem = GetListViewItem(this.itemUnderDragCursor);
                        if (listViewItem != null)
                            ListViewItemDragState.SetIsUnderDragCursor(listViewItem, i == 1);
                    }
                }
            }
        }

        ListViewItem GetListViewItem(StringItems2 dataItem)
        {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return this.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
        }

        void PerformDragOperation()
        {
            StringItems2 selectedItem = this.listView.SelectedItem as StringItems2;
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;
            if (DragDrop.DoDragDrop(this, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                // The item was dropped into a new location,
                // so make it the new selected item.
                this.listView.SelectedItem = selectedItem;
            }
        }

        //void listView_PreviewDragOver(object sender, DragEventArgs e)
        //{
        //    int overIndex = this.IndexUnderDragCursor;
        //}

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    System.Diagnostics.Debug.WriteLine(current.ToString());
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        int IndexUnderDragCursor
        {
            get
            {
                int index = -1;
                for (int i = 0; i < this.listView.Items.Count; ++i)
                {
                    ListViewItem item = GetListViewItem(i);
                    if (_IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                    else
                    {
                    }
                }
                return index;
            }
        }

        ListViewItem GetListViewItem(int index)
        {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return this.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        /// <summary>
        /// Returns true if the mouse cursor is over a scrollbar in the ListView.
        /// </summary>
        bool IsMouseOverScrollbar
        {
            get
            {
                Point ptMouse = MouseUtilities.GetMousePosition(this.listView);
                HitTestResult res = VisualTreeHelper.HitTest(this.listView, ptMouse);
                if (res == null)
                    return false;

                DependencyObject depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                        return true;

                    // VisualTreeHelper works with objects of type Visual or Visual3D.
                    // If the current object is not derived from Visual or Visual3D,
                    // then use the LogicalTreeHelper to find the parent element.
                    if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else
                        depObj = LogicalTreeHelper.GetParent(depObj);
                }

                return false;
            }
        }

    }
    
    /// <summary>
    /// Event arguments used to handle ProcessDrop event.
    /// </summary>
    /// <typeparam name="ItemType">The type of data object being dropped.</typeparam>
    public class ProcessDropEventArgs<ItemType> : EventArgs where ItemType : class
    {

        ObservableCollection<ItemType> itemsSource;
        ItemType dataItem;
        int oldIndex;
        int newIndex;
        DragDropEffects allowedEffects = DragDropEffects.None;
        DragDropEffects effects = DragDropEffects.None;

        internal ProcessDropEventArgs(
            ObservableCollection<ItemType> itemsSource,
            ItemType dataItem,
            int oldIndex,
            int newIndex,
            DragDropEffects allowedEffects)
        {
            this.itemsSource = itemsSource;
            this.dataItem = dataItem;
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            this.allowedEffects = allowedEffects;
        }

        public ObservableCollection<ItemType> ItemsSource
        {
            get { return this.itemsSource; }
        }

        public ItemType DataItem
        {
            get { return this.dataItem; }
        }

        public int OldIndex
        {
            get { return this.oldIndex; }
        }

        public int NewIndex
        {
            get { return this.newIndex; }
        }

        public DragDropEffects AllowedEffects
        {
            get { return allowedEffects; }
        }

        public DragDropEffects Effects
        {
            get { return effects; }
            set { effects = value; }
        }
    }

    /// <summary>
    /// Exposes attached properties to allow triggers to modify the appearance of ListViewItems
    /// in a ListView during a drag-drop operation.
    /// </summary>
    public static class ListViewItemDragState
    {
        /// <summary>
        /// Identifies the ListViewItemDragState's IsBeingDragged attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsBeingDraggedProperty =
            DependencyProperty.RegisterAttached(
                "IsBeingDragged",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListViewItem is being dragged, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsBeingDragged(ListViewItem item)
        {
            return (bool)item.GetValue(IsBeingDraggedProperty);
        }

        /// <summary>
        /// Sets the IsBeingDragged attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is being dragged, else false.</param>
        internal static void SetIsBeingDragged(ListViewItem item, bool value)
        {
            item.SetValue(IsBeingDraggedProperty, value);
        }

        /// <summary>
        /// Identifies the ListViewItemDragState's IsUnderDragCursor attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached(
                "IsUnderDragCursor",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListViewItem is currently underneath the cursor 
        /// during a drag-drop operation, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsUnderDragCursor(ListViewItem item)
        {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        /// <summary>
        /// Sets the IsUnderDragCursor attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
        internal static void SetIsUnderDragCursor(ListViewItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }
    }
}
