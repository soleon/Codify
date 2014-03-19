using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Codify.Extensions;

namespace Codify.Windows.Interactivity
{
    public class DataGridEnhancedSelection : Behavior<DataGrid>
    {
        private object _previousSelection;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(UIElement.PreviewMouseDownEvent, (MouseButtonEventHandler)OnDataGridPreviewMouseDown, true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.PreviewMouseDownEvent, (MouseButtonEventHandler)OnDataGridPreviewMouseDown);
            base.OnDetaching();
        }

        private void OnDataGridPreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            // Only proceed if the event happens on a DataGridRow that is directly in the associated DataGrid and there's no nested DataGrid.
            var associatedDataGrid = AssociatedObject;
            DataGridRow dataGridRow = null;
            var element = args.OriginalSource as DependencyObject;
            while (element != associatedDataGrid && element != null)
            {
                if (element is DataGrid)
                {
                    args.Handled = true;
                    return;
                }
                if (dataGridRow == null)
                    dataGridRow = element as DataGridRow;
                element = VisualTreeHelper.GetParent(element);
            }

            if (dataGridRow == null) return;

            args.Handled = true;

            var keyModifiers = Keyboard.Modifiers;

            // Remember selection start for future multi-select action when Shift key is down.
            if ((keyModifiers & ModifierKeys.Shift) != ModifierKeys.Shift || _previousSelection == null)
                _previousSelection = dataGridRow.DataContext;

            if (!dataGridRow.IsSelected)
                ProcessItemSelection(dataGridRow);
            else
            {
                if (AssociatedObject.SelectedItems.Count < 2 && keyModifiers != ModifierKeys.Control)
                    ProcessItemSelection(dataGridRow);
                else
                    // Handles mouse up event on this item regardless if the event is set to handled.
                    dataGridRow.AddHandler(UIElement.MouseUpEvent, (MouseButtonEventHandler)OnDataGridRowMouseUp, true);
            }
        }

        /// <summary>
        ///     Handles mouse up event after every mouse down event on a selected item if there are already multiple items
        ///     selected.
        /// </summary>
        private void OnDataGridRowMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dataGridRow = (DataGridRow)sender;

            // Remove this handler itself. It should be re-attached in the next mouse up event on a selected item if there are already multiple items selected.
            dataGridRow.RemoveHandler(UIElement.MouseUpEvent, (MouseButtonEventHandler)OnDataGridRowMouseUp);

            if (e.ChangedButton == MouseButton.Left)
                ProcessItemSelection(dataGridRow);
        }

        private void ProcessItemSelection(DataGridRow dataGridRow)
        {
            var selectedItems = AssociatedObject.SelectedItems;
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.Control:
                    dataGridRow.IsSelected = !dataGridRow.IsSelected;
                    break;
                case ModifierKeys.Shift:
                    // When mouse down while the shift key is pressed, we need to find items between 
                    // the selection start and the current item and mark them as selected while clearing out selection on other items.
                    if (_previousSelection == null)
                        break;

                    selectedItems.Clear();
                    var previousSelectionIndex = AssociatedObject.Items.IndexOf(_previousSelection);
                    if (previousSelectionIndex < 0)
                        break;
                    var currentIndex = dataGridRow.GetIndex();
                    if (previousSelectionIndex == currentIndex)
                        SelectItemAt(currentIndex);
                    else
                        for (var i = previousSelectionIndex < currentIndex ? previousSelectionIndex : currentIndex; i <= (previousSelectionIndex < currentIndex ? currentIndex : previousSelectionIndex); i++)
                            SelectItemAt(i);
                    break;
                default:
                    for (var i = selectedItems.Count - 1; i >= 0; i--)
                    {
                        var item = selectedItems[i];
                        if (item == dataGridRow || item == dataGridRow.DataContext)
                            continue;
                        selectedItems.RemoveAt(i);
                    }
                    dataGridRow.IsSelected = true;
                    break;
            }
        }

        private void SelectItemAt(int index)
        {
            AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index).UseIfNotNull(container =>
            {
                // Only allow selection on the visible rows.
                var row = ((DataGridRow)container);
                row.IsSelected = row.Visibility == Visibility.Visible;
            });
        }
    }
}