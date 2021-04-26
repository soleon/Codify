using System.Windows;
using Codify.System.ComponentModel;

namespace Codify.System.Windows.Controls
{
    public class ViewModel<T> : NotificationObject where T : FrameworkElement, new()
    {
        private T _view;

        public virtual T View
        {
            get { return _view ??= CreateNewView(); }
            set
            {
                var old = _view;
                if (!SetValue(ref _view, value))
                {
                    return;
                }

                if (old != null)
                {
                    old.Loaded -= OnLoaded;
                    old.Unloaded -= OnUnloaded;
                }

                if (value == null)
                {
                    return;
                }

                value.Loaded += OnLoaded;
                value.Unloaded += OnUnloaded;
            }
        }

        protected virtual T CreateNewView()
        {
            var view = new T {DataContext = this};

            view.Loaded += OnLoaded;
            view.Unloaded += OnUnloaded;

            return view;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            OnLoad();
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            OnUnload();
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnUnload()
        {
        }
    }
}