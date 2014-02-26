using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Codify.Windows.Threading.Tasks;

namespace Codify.Windows.Extensions
{
    public static class TreeViewExtensions
    {
        public static bool GetBringSelectedItemIntoView(DependencyObject obj)
        {
            return (bool)obj.GetValue(BringSelectedItemIntoViewProperty);
        }

        public static void SetBringSelectedItemIntoView(DependencyObject obj, bool value)
        {
            obj.SetValue(BringSelectedItemIntoViewProperty, value);
        }

        public static readonly DependencyProperty BringSelectedItemIntoViewProperty = DependencyProperty.RegisterAttached(
            "BringSelectedItemIntoView",
            typeof(bool),
            typeof(TreeViewExtensions),
            null);

        public static DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewExtensions),
            new PropertyMetadata(new object(), OnSelectedItemChanged));

        public static object GetSelectedItem(TreeView treeView)
        {
            return treeView.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(TreeView treeView, object value)
        {
            treeView.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject attachedControl, DependencyPropertyChangedEventArgs args)
        {
            var treeView = attachedControl as TreeView;
            if (treeView == null)
                throw new InvalidOperationException("TreeViewExtensions.SelectedItem property must be attached to a TreeView.");
            var bringSelectedItemIntoView = GetBringSelectedItemIntoView(treeView);
            treeView.SelectedItemChanged -= TreeViewItemChanged;
            treeView.SelectedItemChanged += TreeViewItemChanged;
            if (args.NewValue == null)
            {
                if (args.OldValue != null)
                    treeView.FindItemContainer(args.OldValue).ContinueWith(task =>
                    {
                        if (task.Exception != null || task.Result == null) return;
                        treeView.SelectedItemChanged -= TreeViewItemChanged;
                        task.Result.IsSelected = false;
                        treeView.SelectedItemChanged += TreeViewItemChanged;
                    }, UITaskScheduler.FromCurrentSynchronizationContext());
            }
            else
                treeView.FindItemContainer(args.NewValue).ContinueWith(task =>
                {
                    if (task.Exception != null || task.Result == null) return;
                    treeView.SelectedItemChanged -= TreeViewItemChanged;
                    var item = task.Result;
                    item.IsSelected = true;
                    if (bringSelectedItemIntoView)
                        item.BringIntoView();
                    treeView.SelectedItemChanged += TreeViewItemChanged;
                }, UITaskScheduler.FromCurrentSynchronizationContext());
        }

        private static void TreeViewItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetSelectedItem((TreeView)sender, e.NewValue);
        }

        /// <summary>
        ///     Recursively finds a the container of a particular data item asynchronously. The result of the task returned by this
        ///     method is the item container to be found.
        /// </summary>
        private static Task<TreeViewItem> FindItemContainer(this ItemsControl itemsControl, object dataItem)
        {
            var source = new TaskCompletionSource<TreeViewItem>();
            if (dataItem == null) source.SetResult(null);
            else
            {
                var generator = itemsControl.ItemContainerGenerator;
                if (generator.Status == GeneratorStatus.ContainersGenerated)
                {
                    var item = generator.ContainerFromItem(dataItem) as TreeViewItem;
                    if (item == null)
                        foreach (var childItem in itemsControl.Items.Cast<object>().Select(generator.ContainerFromItem).Cast<TreeViewItem>().Where(childItem => !source.Task.IsCompleted && childItem != null))
                            childItem.FindItemContainer(dataItem).ContinueWith(task => { if (!source.Task.IsCompleted) source.SetResult(task.Result); });
                    else if (item.Visibility != Visibility.Visible) source.SetResult(null);
                    else source.SetResult(item);
                }
                else
                {
                    EventHandler containersGeneratedEventHandler = null;
                    containersGeneratedEventHandler = (sender, args) =>
                    {
                        if (generator.Status != GeneratorStatus.ContainersGenerated) return;
                        generator.StatusChanged -= containersGeneratedEventHandler;
                        itemsControl.FindItemContainer(dataItem).ContinueWith(task =>
                        {
                            if (task.Exception == null) source.SetResult(task.Result);
                            else source.SetException(task.Exception.InnerException);
                        });
                    };
                    generator.StatusChanged += containersGeneratedEventHandler;
                }
            }

            return source.Task;
        }
    }
}