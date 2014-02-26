using System;
using System.ComponentModel;
using Codify.Extensions;

namespace Codify.Entities
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises the property changed notification for the specified property and optionally marks this instance as changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises the property changed notification for the specified properties.
        /// </summary>
        /// <param name="propertyNames">The name of the properties to notify change.</param>
        protected virtual void RaisePropertyChanged(params string[] propertyNames)
        {
            propertyNames.ForEach(RaisePropertyChanged);
        }

        /// <summary>
        ///     Sets the value of the target object reference only if the value is different than the target reference value.
        /// </summary>
        /// <typeparam name="T">Type of the target and value.</typeparam>
        /// <param name="target">The reference of the target whoes value will be updated.</param>
        /// <param name="value">The value to update the target.</param>
        /// <param name="propertyNames">The property names to specify which properties to notify this change.</param>
        /// <returns><c>true</c> if the new value is different from the old value, otherwise <c>false</c>.</returns>
        protected virtual bool SetValue<T>(ref T target, T value, params string[] propertyNames)
        {
            if (Equals(target, value)) return false;
            target = value;
            RaisePropertyChanged(propertyNames);
            return true;
        }
    }
}