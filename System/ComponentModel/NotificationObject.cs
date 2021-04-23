using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Codify.System.ComponentModel
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetValue<T>(ref T source, T value, string propertyName, params string[] propertyNames)
        {
            if (!SetValue(ref source, value, propertyName))
            {
                return false;
            }

            if (propertyNames == null)
            {
                return true;
            }

            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }

            return true;
        }

        protected bool SetValue<T>(ref T source, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(source, value))
            {
                return false;
            }

            source = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}