using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Codify.Extensions;

namespace Codify.Windows.Interactivity
{
    public class KeySequencer : Behavior<UIElement>
    {
        public const char AttributeSeparator = ',';
        private ushort _currentIndex;
        private Timer _timer;
        private Key[] _keySequence;

        public event Action KeySequenceCompleted;

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(KeySequencer));


        public KeyCollection KeySequence
        {
            get { return (KeyCollection)GetValue(KeySequenceProperty); }
            set { SetValue(KeySequenceProperty, value); }
        }

        public static readonly DependencyProperty KeySequenceProperty = DependencyProperty.Register(
            "KeySequence",
            typeof(KeyCollection),
            typeof(KeySequencer),
            new PropertyMetadata(default(KeyCollection), (s, e) => ((KeySequencer)s).OnKeySequenceChanged((KeyCollection)e.NewValue)));

        private void OnKeySequenceChanged(KeyCollection newValue)
        {
            _keySequence = newValue.ProcessIfNotNull(v => v.ToArray());
            Reset();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
        }

        private void AssociatedObjectOnKeyDown(object sender, KeyEventArgs args)
        {
            if (_keySequence.IsNullOrEmpty())
            {
                _currentIndex = 0;
            }
            else if (_currentIndex == 0)
            {
                var exptectedKey = _keySequence[0];
                if (args.Key != exptectedKey) return;
                if (Duration > TimeSpan.Zero)
                    _timer = new Timer(_ => Reset(), null, Duration, TimeSpan.FromMilliseconds(-1));
                _currentIndex++;
            }
            else if (_currentIndex == _keySequence.Length - 1)
            {
                KeySequenceCompleted.ExecuteIfNotNull();
                Reset();
            }
            else if (_currentIndex < _keySequence.Length)
            {
                var expectedKey = _keySequence[_currentIndex];
                if (args.Key == expectedKey) _currentIndex++;
                else Reset();
            }
        }

        private void Reset()
        {
            _currentIndex = 0;
            if (_timer == null) return;
            _timer.Dispose();
            _timer = null;
        }

        [TypeConverter(typeof(KeyConverter))]
        public class KeyCollection : List<Key> { }

        public class KeyConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var keyString = value as string;
                if (keyString == null) return null;
                var collection = new KeyCollection();
                if (keyString.Contains(AttributeSeparator))
                    collection.AddRange(keyString.Split(AttributeSeparator).Select(kstr =>
                    {
                        Key testKey;
                        if (Enum.TryParse(kstr.ToString(CultureInfo.InvariantCulture), out testKey))
                            return testKey;
                        throw new NotSupportedException("Convertring " + kstr + " to " + typeof(Key) + " is not supported.");
                    }));
                else
                    collection.AddRange(keyString.Select(c =>
                    {
                        Key testKey;
                        if (Enum.TryParse(c.ToString(CultureInfo.InvariantCulture), out testKey))
                            return testKey;
                        throw new NotSupportedException("Convertring " + keyString + " to key sequence is not supported.");
                    }));

                return collection;
            }
        }
    }
}