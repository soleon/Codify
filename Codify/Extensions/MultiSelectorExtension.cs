using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Codify.Windows.Extensions
{
    public static class MultiSelectorExtension
    {
        private static readonly DependencyProperty IsSelectedItemsPropertyAttachedProperty = DependencyProperty.RegisterAttached("IsSelectedItemsPropertyAttached", typeof(bool), typeof(MultiSelectorExtension));

        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        ///     Identifies the <see cref="MultiSelectorExtension" />.SelectedItems attached property.
        /// </summary>
        /// <remarks>
        ///     This attached property sychronises with the <see cref="MultiSelector.SelectedItems" /> property and allows binding.
        ///     To bind to this property, <see cref="BindingMode.OneWayToSource" /> is recommended and always specify the
        ///     <see cref="Binding.FallbackValue" /> (e.g. set it to <c>null</c>).
        ///     Also make sure that the property in the view model is in a type that is competible to the value of the
        ///     <see cref="MultiSelector.SelectedItems" /> property.
        /// </remarks>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(MultiSelectorExtension),
            new PropertyMetadata(new object[] { }, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var multiSelector = sender as MultiSelector;
            if (multiSelector == null) throw new Exception("The SelectedItems dependency property can only be attached to a " + typeof(MultiSelector) + ".");
            if ((bool)multiSelector.GetValue(IsSelectedItemsPropertyAttachedProperty)) return;
            multiSelector.SetValue(IsSelectedItemsPropertyAttachedProperty, true);
            multiSelector.SelectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var multiSelector = (MultiSelector)sender;
            multiSelector.SetValue(SelectedItemsProperty, multiSelector.SelectedItems);
        }
    }
}