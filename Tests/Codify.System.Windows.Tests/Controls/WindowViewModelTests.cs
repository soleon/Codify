using System.Runtime.ExceptionServices;
using System.Windows;
using Codify.System.Windows.Controls;

namespace Codify.System.Windows.Tests.Controls;

public class WindowViewModelTests
{
    [Fact]
    public void ViewLazilyCreatesWindowWithDataContext()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingWindowViewModel();

            var window = viewModel.View;

            Assert.Same(window, viewModel.View);
            Assert.Same(viewModel, window.DataContext);
        });
    }

    [Fact]
    public void ClosedWindowUnloadsAndClearsCurrentView()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingWindowViewModel();
            var closedWindow = viewModel.View;

            closedWindow.RaiseClosed();

            var replacementWindow = viewModel.View;
            Assert.Equal(1, viewModel.UnloadCount);
            Assert.NotSame(closedWindow, replacementWindow);
            Assert.Same(viewModel, replacementWindow.DataContext);
        });
    }

    [Fact]
    public void ClosingReplacedWindowDoesNotClearCurrentView()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingWindowViewModel();
            var replacedWindow = viewModel.View;
            var currentWindow = new TestWindow();

            viewModel.View = currentWindow;
            replacedWindow.RaiseClosed();

            Assert.Same(currentWindow, viewModel.View);
            Assert.Equal(0, viewModel.UnloadCount);
        });
    }

    [Fact]
    public void ShowAssignsOwnerAndDisplaysWindow()
    {
        RunOnStaThread(() =>
        {
            var owner = new TestWindow();
            var viewModel = new TrackingWindowViewModel();
            var window = viewModel.View;

            owner.Show();
            viewModel.Show(owner);

            Assert.Same(owner, window.Owner);
            Assert.True(window.IsVisible);

            window.Close();
            owner.Close();
        });
    }

    [Fact]
    public void HideHidesCurrentWindow()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingWindowViewModel();
            var window = viewModel.View;

            viewModel.Show();
            viewModel.Hide();

            Assert.False(window.IsVisible);

            window.Close();
        });
    }

    [Fact]
    public void CloseClosesCurrentWindowAndClearsView()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new TrackingWindowViewModel();
            var closedWindow = viewModel.View;

            viewModel.Show();
            viewModel.Close();

            Assert.NotSame(closedWindow, viewModel.View);
        });
    }

    [Fact]
    public void ShowDialogAssignsOwnerAndReturnsDialogResult()
    {
        RunOnStaThread(() =>
        {
            var owner = new TestWindow();
            var viewModel = new TrackingWindowViewModel();
            var window = viewModel.View;
            window.Loaded += (_, _) =>
            {
                window.Dispatcher.BeginInvoke(
                    () => viewModel.Close(true),
                    global::System.Windows.Threading.DispatcherPriority.Background);
            };

            owner.Show();
            var result = viewModel.ShowDialog(owner);

            Assert.True(result);
            Assert.Same(owner, window.Owner);

            owner.Close();
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

    private sealed class TestWindow : Window
    {
        public void RaiseClosed()
        {
            OnClosed(EventArgs.Empty);
        }
    }

    private sealed class TrackingWindowViewModel : WindowViewModel<TestWindow>
    {
        public int UnloadCount { get; private set; }

        protected override void OnUnload()
        {
            UnloadCount++;
        }
    }
}
