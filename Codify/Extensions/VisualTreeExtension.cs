using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Codify.Windows.Extensions
{
    public static class VisualTreeExtension
    {
        public static UIElement FindRootUIElement(DependencyObject obj)
        {
            UIElement rootUIElement = Window.GetWindow(obj);

            if (rootUIElement != null) return rootUIElement;
            var parentUIElement = VisualTreeHelper.GetParent(obj) as UIElement;
            while (parentUIElement != null)
            {
                rootUIElement = parentUIElement;
                parentUIElement = VisualTreeHelper.GetParent(parentUIElement) as UIElement;
            }
            if (rootUIElement == null)
                throw new Exception("Unable to find the root UI element of the attached object.");
            return rootUIElement;
        }

        /// <summary>
        ///     Finds the top most adorner layer in the specified element.
        /// </summary>
        /// <returns>
        ///     This is the complete opposite to <see cref="AdornerLayer.GetAdornerLayer" /> method.
        /// </returns>
        public static AdornerLayer FindRootAdornerLayer(DependencyObject obj)
        {
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < visualChildrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i) as AdornerDecorator;
                if (child != null) return child.AdornerLayer;
            }
            for (var i = 0; i < visualChildrenCount; i++)
            {
                var layer = FindRootAdornerLayer(VisualTreeHelper.GetChild(obj, i));
                if (layer != null) return layer;
            }
            return null;
        }

        public static T FindParentObject<T>(DependencyObject childObject) where T : DependencyObject
        {
            while (true)
            {
                childObject = VisualTreeHelper.GetParent(childObject);
                if (childObject == null) return null;
                var parent = childObject as T;
                if (parent != null) return parent;
            }
        }
    }
}