using System.Runtime.ExceptionServices;
using System.Windows;
using Codify.System.Windows.Controls;

namespace Codify.System.Windows.Tests.Controls;

public class ViewModelTests
{
    [Fact]
    public void ViewLazilyCreatesViewWithDataContextAndEventHandlers()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingViewModel();

            var view = viewModel.View;
            view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, view));
            view.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent, view));

            Assert.Same(view, viewModel.View);
            Assert.Same(viewModel, view.DataContext);
            Assert.Equal(1, viewModel.LoadCount);
            Assert.Equal(1, viewModel.UnloadCount);
        });
    }

    [Fact]
    public void SettingViewRaisesPropertyChangedAndMovesEventHandlersToNewView()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingViewModel();
            var original = viewModel.View;
            var replacement = new TestElement();
            var propertyNames = new List<string?>();
            viewModel.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);

            viewModel.View = replacement;
            original.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, original));
            original.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent, original));
            replacement.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, replacement));
            replacement.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent, replacement));

            Assert.Same(replacement, viewModel.View);
            Assert.Equal(["View"], propertyNames);
            Assert.Equal(1, viewModel.LoadCount);
            Assert.Equal(1, viewModel.UnloadCount);
        });
    }

    [Fact]
    public void SettingViewAttachesViewModelAsReplacementDataContext()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingViewModel();
            var replacement = new TestElement();

            viewModel.View = replacement;

            Assert.Same(viewModel, replacement.DataContext);
        });
    }

    [Fact]
    public void SettingSameViewDoesNotRaisePropertyChanged()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingViewModel();
            var view = viewModel.View;
            var propertyNames = new List<string?>();
            viewModel.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);

            viewModel.View = view;

            Assert.Empty(propertyNames);
        });
    }

    private static void RunOnStaThread(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private sealed class TestElement : FrameworkElement;

    private sealed class TrackingViewModel : ViewModel<TestElement>
    {
        public int LoadCount { get; private set; }

        public int UnloadCount { get; private set; }

        protected override void OnLoad()
        {
            LoadCount++;
        }

        protected override void OnUnload()
        {
            UnloadCount++;
        }
    }
}
