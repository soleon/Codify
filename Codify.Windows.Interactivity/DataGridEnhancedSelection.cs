using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Codify.Extensions;
using Codify.Windows.Extensions;

namespace Codify.Windows.Interactivity
{
    public class DataGridEnhancedSelection : Behavior<DataGrid>
    {
        private readonly MouseButtonEventHandler _previewMouseDownEventHandler, _dataGridRowMouseUpEventHandler;
        private object _previousSelection;

        public DataGridEnhancedSelection()
        {
            _previewMouseDownEventHandler = OnDataGridPreviewMouseDown;
            _dataGridRowMouseUpEventHandler = OnDataGridRowMouseUp;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(UIElement.PreviewMouseDownEvent, _previewMouseDownEventHandler, true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.PreviewMouseDownEvent, _previewMouseDownEventHandler);
            base.OnDetaching();
        }

        private void OnDataGridPreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            args.Handled = true;

            var dataGridRow = VisualTreeExtension.FindParentObject<DataGridRow>((DependencyObject) args.OriginalSource);
            if (dataGridRow == null)
                AssociatedObject.SelectedItems.Clear();
            else
            {
                var keyModifiers = Keyboard.Modifiers;

                // Remember selection start for future multi-select action when Shift key is down.
                if ((keyModifiers & ModifierKeys.Shift) != ModifierKeys.Shift || _previousSelection == null)
                    _previousSelection = dataGridRow.DataContext;

                if (AssociatedObject.SelectedItems.Count < 2)
                    ProcessItemSelection(dataGridRow);
                else
                    // Handles mouse up event on this item regardless if the event is set to handled.
                    dataGridRow.AddHandler(UIElement.MouseUpEvent, _dataGridRowMouseUpEventHandler, true);
            }
        }

        /// <summary>
        ///     Handles mouse up event after every mouse down event on a selected item if there are already multiple items
        ///     selected.
        /// </summary>
        private void OnDataGridRowMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dataGridRow = (DataGridRow) sender;

            // Remove this handler itself. It should be re-attached in the next mouse up event on a selected item if there are already multiple items selected.
            dataGridRow.RemoveHandler(UIElement.MouseUpEvent, _dataGridRowMouseUpEventHandler);

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
                        AssociatedObject.ItemContainerGenerator.ContainerFromIndex(currentIndex).UseIfNotNull(container => ((DataGridRow) container).IsSelected = true);
                    else
                    {
                        var end = previousSelectionIndex < currentIndex ? currentIndex : previousSelectionIndex;
                        for (var start = previousSelectionIndex < currentIndex ? previousSelectionIndex : currentIndex; start <= end; start++)
                            AssociatedObject.ItemContainerGenerator.ContainerFromIndex(start).UseIfNotNull(container => ((DataGridRow) container).IsSelected = true);
                    }
                    break;
                default:
                    selectedItems.Clear();
                    dataGridRow.IsSelected = true;
                    break;
            }
        }
    }
}