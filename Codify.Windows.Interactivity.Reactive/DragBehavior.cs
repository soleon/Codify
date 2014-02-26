using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Codify.Extensions;
using Codify.Windows.Extensions;

namespace Codify.Windows.Interactivity.Reactive
{
    public class DragBehavior : Behavior<UIElement>
    {
        public delegate void DragDropBehaviorEventHandler(object sender, DragEventArgs e);

        public event DragDropBehaviorEventHandler DragStarted;
        public event DragDropBehaviorEventHandler PreviewDragStarted;
        public event DragDropBehaviorEventHandler DragCompleted;

        private IDisposable _mouseEnterEventSubscription, _dragStartEventSubscription;

        public TransformGroup DragCueTransform
        {
            get { return (TransformGroup) GetValue(DragCueTransformProperty); }
            set { SetValue(DragCueTransformProperty, value); }
        }

        public static readonly DependencyProperty DragCueTransformProperty = DependencyProperty.Register("DragCueTransform", typeof (TransformGroup), typeof (DragBehavior));

        public bool StaticDragHintCue
        {
            get { return (bool) GetValue(StaticDragHintCueProperty); }
            set { SetValue(StaticDragHintCueProperty, value); }
        }

        public static readonly DependencyProperty StaticDragHintCueProperty = DependencyProperty.Register("StaticDragHintCue", typeof (bool), typeof (DragBehavior));

        public bool AllowDrag
        {
            get { return (bool) GetValue(AllowDragProperty); }
            set { SetValue(AllowDragProperty, value); }
        }

        public static readonly DependencyProperty AllowDragProperty = DependencyProperty.Register("AllowDrag", typeof (bool), typeof (DragBehavior), new PropertyMetadata(true));

        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects) GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        public static readonly DependencyProperty AllowedEffectsProperty = DependencyProperty.Register("AllowedEffects", typeof (DragDropEffects), typeof (DragBehavior));

        public object DragDataObject
        {
            get { return GetValue(DragDataObjectProperty); }
            set { SetValue(DragDataObjectProperty, value); }
        }

        public static readonly DependencyProperty DragDataObjectProperty = DependencyProperty.Register("DragDataObject", typeof (object), typeof (DragBehavior));

        public double Opacity
        {
            get { return (double) GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register("Opacity", typeof (double), typeof (DragBehavior), new PropertyMetadata(0.5D));


        public int Count
        {
            get { return (int) GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof (int), typeof (DragBehavior));


        protected override void OnAttached()
        {
            base.OnAttached();

            var associatedObject = AssociatedObject;
            _mouseEnterEventSubscription =
                Observable.FromEventPattern<MouseEventArgs>(
                    handler => associatedObject.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(handler), true),
                    handler => associatedObject.RemoveHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(handler)))
                    .Where(pattern => AllowDrag).Take(1).Subscribe(pattern =>
                    {
                        // Initialize whatever is needed the first time the mouse is entering the area of the associated object.
                        UIElement rootUIElement = null;
                        AdornerLayer adornerLayer = null;
                        IObservable<Point> mouseDragOverPositions = null;

                        // Setup queries for basic event sequences.
                        var mouseDownPositions =
                            Observable.FromEventPattern<MouseButtonEventArgs>(
                                handler => associatedObject.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(handler), true),
                                handler => associatedObject.RemoveHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(handler)))
                                .Select(eventPattern =>
                                {
                                    associatedObject.CaptureMouse();
                                    return eventPattern.EventArgs.GetPosition(associatedObject);
                                });
                        var mouseUpPositions =
                            Observable.FromEventPattern<MouseButtonEventArgs>(
                                handler => associatedObject.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(handler), true),
                                handler => associatedObject.RemoveHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(handler)))
                                .Select(eventPattern =>
                                {
                                    associatedObject.ReleaseMouseCapture();
                                    return eventPattern.EventArgs.GetPosition(associatedObject);
                                });
                        var mouseMovePositions =
                            Observable.FromEventPattern<MouseEventArgs>(
                                handler => associatedObject.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(handler), true),
                                handler => associatedObject.RemoveHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(handler)))
                                .Select(eventPattern => eventPattern.EventArgs.GetPosition(associatedObject));

                        // Advance query to detect drag start event sequence with accidental drag detection.
                        var dragStartPositions = mouseDownPositions.SelectMany(mouseDownPosition =>
                            mouseMovePositions
                                .Where(mouseMovePosition => Math.Abs(mouseMovePosition.X - mouseDownPosition.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(mouseMovePosition.Y - mouseDownPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                                .StartWith(mouseDownPosition)
                                .TakeUntil(mouseUpPositions)
                                .SkipUntil(mouseMovePositions)
                                .Take(1));

                        // Subscribe to the drag start event sequence.
                        _dragStartEventSubscription = dragStartPositions.Subscribe(startMousePosition =>
                        {
                            associatedObject.ReleaseMouseCapture();

                            // Notify preview drag start.
                            var previewDragStartedArgs = new DragEventArgs {Source = associatedObject};
                            if (PreviewDragStarted != null) PreviewDragStarted(this, previewDragStartedArgs);
                            if (previewDragStartedArgs.Handled) return;

                            if (rootUIElement == null || adornerLayer == null)
                            {
                                // Initialize drag adornment if it has not happened before.
                                // This initialization is not needed if drag is cancelled.

                                // Try finding the root visual element that hosts the associated object.
                                rootUIElement = VisualTreeExtension.FindRootUIElement(associatedObject);
                                if (rootUIElement == null)
                                    throw new Exception("Unable to find the root UI element of the attached object.");

                                // Allow drop on root element.
                                rootUIElement.AllowDrop = true;

                                // Try finding the top most adorner layer.
                                // Note there could be multiple adorner layers at different level of visual tree,
                                // to ensure visibility of the adorner at all tiems, use the top most adorner layer.
                                adornerLayer = VisualTreeExtension.FindRootAdornerLayer(rootUIElement);
                                if (adornerLayer == null)
                                    throw new Exception("Unable to get the adorner layer of the root UI element.");

                                mouseDragOverPositions = Observable.FromEventPattern<System.Windows.DragEventArgs>(
                                    handler => rootUIElement.AddHandler(UIElement.PreviewDragOverEvent, new DragEventHandler(handler), true),
                                    handler => rootUIElement.RemoveHandler(UIElement.PreviewDragOverEvent, new DragEventHandler(handler)))
                                    .Select(eventPattern => eventPattern.EventArgs.GetPosition(adornerLayer));
                            }

                            // Setup adorner.
                            var dragAdorner = new DragAdorner(associatedObject, adornerLayer, Count, StaticDragHintCue, DragCueTransform);
                            dragAdorner.SetBinding(DragAdorner.OpacityProperty, new Binding("Opacity") {Source = this, Mode = BindingMode.OneWay});

                            adornerLayer.Add(dragAdorner);

                            // Notify drag start.
                            var dragStartedEventArgs = new DragEventArgs {Source = associatedObject};
                            if (DragStarted != null) DragStarted(this, dragStartedEventArgs);
                            if (dragStartedEventArgs.Handled) return;

                            DragDropEffects resultDragDropEffects;

                            var dataObject = new DataObject();
                            dataObject.SetData(associatedObject);

                            if (DragDataObject != null)
                                dataObject.SetData(DragDataObject);

                            // Subscribe to drag over events of the root element only when the drag and drop action is in place.
                            // The "using" block ensures the subscription is disposed right after the drag and drop action.
                            using (mouseDragOverPositions.Subscribe(currentMousePosition => dragAdorner.SetPosition(currentMousePosition.X - startMousePosition.X, currentMousePosition.Y - startMousePosition.Y)))
                                resultDragDropEffects = DragDrop.DoDragDrop(associatedObject, dataObject, AllowedEffects);

                            adornerLayer.Remove(dragAdorner);

                            // Notify drag complete.
                            DragCompleted.UseIfNotNull(handler => handler(this, new DragEventArgs {Effects = resultDragDropEffects, Source = associatedObject}));
                        });
                    });
        }

        protected override void OnDetaching()
        {
            _mouseEnterEventSubscription.UseIfNotNull(subscription => subscription.Dispose());
            _dragStartEventSubscription.UseIfNotNull(subscription => subscription.Dispose());
            base.OnDetaching();
        }

        private sealed class DragAdorner : Adorner
        {
            private readonly AdornerLayer _adornerLayer;
            private readonly UIElement _adornerContainer;
            private readonly TransformGroup _adornerTransform;
            private readonly TranslateTransform _adornerPositionTransform;

            internal new static readonly DependencyProperty OpacityProperty = DependencyProperty.Register("Opacity", typeof (double), typeof (DragAdorner));

            internal DragAdorner(UIElement adornedElement, AdornerLayer adornerLayer, int count, bool generateStaticCue, TransformGroup adornerTransform)
                : base(adornedElement)
            {
                _adornerPositionTransform = new TranslateTransform();
                FrameworkElement adornerElement;
                _adornerLayer = adornerLayer;
                _adornerTransform = adornerTransform;

                if (count > 1)
                    _adornerContainer = new Grid
                    {
                        Children =
                        {
                            (adornerElement = new Rectangle
                            {
                                Width = adornedElement.RenderSize.Width,
                                Height = adornedElement.RenderSize.Height,
                                Fill = new VisualBrush(adornedElement)
                            }),
                            new Border
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                BorderBrush = new SolidColorBrush(Colors.White),
                                BorderThickness = new Thickness(2),
                                Background = new SolidColorBrush(new Color {A = 255, G = 114, B = 188}),
                                CornerRadius = new CornerRadius(3),
                                Padding = new Thickness(3, 0, 3, 0),
                                Margin = new Thickness(-10, -10, 0, 0),
                                Effect = new DropShadowEffect
                                {
                                    ShadowDepth = 0,
                                    Color = new Color {A = 255, R = 201, G = 201, B = 201}
                                },
                                Child = new TextBlock
                                {
                                    Foreground = new SolidColorBrush(Colors.White),
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 10,
                                    Text = count > 99 ? "99+" : count.ToString(CultureInfo.InvariantCulture),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    LineHeight = 14
                                }
                            }
                        }
                    };
                else
                    _adornerContainer = adornerElement = new Rectangle
                    {
                        Width = adornedElement.RenderSize.Width,
                        Height = adornedElement.RenderSize.Height,
                        Fill = generateStaticCue ? GetPreviewBrush(adornedElement) : new VisualBrush(adornedElement)
                    };

                adornerElement.SetBinding(UIElement.OpacityProperty, new Binding("Opacity") {Source = this, Mode = BindingMode.OneWay});
            }

            private static Brush GetPreviewBrush(UIElement element)
            {
                var bitmap = new RenderTargetBitmap(((int) element.RenderSize.Width) + 1, (int) element.RenderSize.Height, 96, 96, PixelFormats.Default);
                bitmap.Render(element);
                return new ImageBrush(bitmap);
            }

            protected override Size MeasureOverride(Size constraint)
            {
                _adornerContainer.Measure(constraint);
                return _adornerContainer.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _adornerContainer.Arrange(new Rect(finalSize));
                return finalSize;
            }

            protected override Visual GetVisualChild(int index)
            {
                return _adornerContainer;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            public void SetPosition(double x, double y)
            {
                _adornerPositionTransform.X = x;
                _adornerPositionTransform.Y = y;
                _adornerLayer.Update(AdornedElement);
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
            {
                if (_adornerTransform == null)
                    return _adornerPositionTransform;

                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(_adornerTransform);
                transformGroup.Children.Add(_adornerPositionTransform);
                return transformGroup;
            }
        }

        public class DragEventArgs : EventArgs
        {
            public bool Handled;
            public object Source;
            public DragDropEffects Effects;
        }
    }
}