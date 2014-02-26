using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Codify.Windows
{
    /// <summary>
    ///     A debug time only class that is intended to provide data context mockups for the XAML view designer in Visual
    ///     Studio or Expression Blend.
    ///     Always wrap the body of the derived classes in the DEBUG flag so that no design time code is compiled into
    ///     production DLLs.
    ///     To use this class, create your own design time class extending this class, create a static resource of your class
    ///     in your XAML view,
    ///     then bind the d:DataContext property of the view to the appropriate property in your class by specifying:
    ///     d:DataContext="{Binding MyViewModel, Source={StaticResource MyDesignTimeClassKey}}"
    /// </summary>
    public class DesignTime
    {
#if DEBUG
        private static readonly Lazy<bool> LazyIsInDesignMode = new Lazy<bool>(() => (bool) DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof (FrameworkElement)).Metadata.DefaultValue);

        /// <summary>
        ///     Gets a value indicating whether this instance is in design mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is in design mode; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInDesignMode
        {
            get { return LazyIsInDesignMode.Value; }
        }

        /// <summary>
        ///     Gets the design time background. Required by the attached <see cref="BackgroundProperty" />.
        /// </summary>
        public static Brush GetBackground(DependencyObject obj)
        {
            return (Brush) obj.GetValue(BackgroundProperty);
        }

        /// <summary>
        ///     Sets the design time background. Required by the attached <see cref="BackgroundProperty" />.
        /// </summary>
        public static void SetBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(BackgroundProperty, value);
        }

        /// <summary>
        ///     An attached property that allows you to set a design time only backgroun in Expression Blend's designer view.
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background",
            typeof (Brush),
            typeof (DesignTime),
            new UIPropertyMetadata(null, OnBackgroundChanged));

        private static void OnBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsInDesignMode)
                return;
            var panel = sender as Panel;
            if (panel != null)
                panel.Background = (Brush) e.NewValue;
            else
            {
                var control = sender as Control;
                if (control != null)
                    control.Background = (Brush) e.NewValue;
            }
        }
#else
        public static bool IsInDesignMode
        {
            get { return false;}
        }
#endif
    }
}