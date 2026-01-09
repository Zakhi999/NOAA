using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NOAA.Controls;

/// <summary>
/// A mock-up similar to the info bar in WinUI3.
/// </summary>
public partial class MessageBar : UserControl
{
    bool _isShown = false;
    System.Windows.Threading.DispatcherTimer? _hideTimer = null;

    #region [Dependency Properties]
    /// <summary>
    ///   Define our text value property
    /// </summary>
    public static readonly DependencyProperty BarTextProperty = DependencyProperty.Register(
        nameof(BarText),
        typeof(string),
        typeof(MessageBar), new PropertyMetadata(OnTextValueChanged));
    public string BarText
    {
        get { return (string)this.GetValue(BarTextProperty); }
        set { this.SetValue(BarTextProperty, value); }
    }
    static void OnTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null && e.NewValue != null)
        {
            //mbc.ShowHideMessage(e.NewValue as string, mbc.TimeoutMS > 0 ? true : false);
            mbc.ShowMessage(e.NewValue as string, mbc.TimeoutMS > 0 ? true : false);
        }
    }

    /// <summary>
    ///   Define our width property
    /// </summary>
    public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register(
        nameof(BarWidth),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(800d, OnBarWidthChanged));
    public double BarWidth
    {
        get { return (double)this.GetValue(BarWidthProperty); }
        set { this.SetValue(BarWidthProperty, value); }
    }
    static void OnBarWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null)
        {
            mbc.DesignerPane.Width = (double)e.NewValue;
        }
    }

    /// <summary>
    ///   Define our text size property
    /// </summary>
    public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register(
        nameof(TextSize),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(14d, OnTextSizeChanged));
    public double TextSize
    {
        get { return (double)this.GetValue(TextSizeProperty); }
        set { this.SetValue(TextSizeProperty, value); }
    }
    static void OnTextSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null && e.NewValue != null)
        {
            mbc.Message.FontSize = (double)e.NewValue;
        }
    }

    /// <summary>
    ///   Define our text alignment property
    /// </summary>
    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
        nameof(TextAlignment),
        typeof(HorizontalAlignment),
        typeof(MessageBar),
        new PropertyMetadata(HorizontalAlignment.Left, OnTextAlignmentChanged));
    public HorizontalAlignment TextAlignment
    {
        get { return (HorizontalAlignment)this.GetValue(TextAlignmentProperty); }
        set { this.SetValue(TextAlignmentProperty, value); }
    }
    static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null && e.NewValue != null)
        {
            mbc.Message.HorizontalAlignment = (HorizontalAlignment)e.NewValue;
        }
    }

    /// <summary>
    ///   Define our corner radius property
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(3d, OnCornerRadiusChanged));
    public double CornerRadius
    {
        get { return (double)this.GetValue(CornerRadiusProperty); }
        set { this.SetValue(CornerRadiusProperty, value); }
    }
    static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null && e.NewValue != null)
        {
            mbc.MessageBorder.CornerRadius = new CornerRadius((double)e.NewValue);
        }
    }

    /// <summary>
    ///   Define our foreground brush property
    /// </summary>
    public static readonly DependencyProperty ForegroundBrushProperty = DependencyProperty.Register(
        nameof(ForegroundBrush),
        typeof(Brush),
        typeof(MessageBar),
        new PropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke)));
    public Brush ForegroundBrush
    {
        get { return (Brush)this.GetValue(ForegroundBrushProperty); }
        set { this.SetValue(ForegroundBrushProperty, value); }
    }

    /// <summary>
    ///   Define our background brush property
    /// </summary>
    public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register(
        nameof(BackgroundBrush),
        typeof(Brush),
        typeof(MessageBar),
        new PropertyMetadata(new SolidColorBrush(Colors.Black)));
    public Brush BackgroundBrush
    {
        get { return (Brush)this.GetValue(BackgroundBrushProperty); }
        set { this.SetValue(BackgroundBrushProperty, value); }
    }

    /// <summary>
    ///   Define our border brush property
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
        nameof(BorderBrush),
        typeof(Brush),
        typeof(MessageBar),
        new PropertyMetadata(new SolidColorBrush(Colors.Gray)));
    public Brush BorderBrush
    {
        get { return (Brush)this.GetValue(BorderBrushProperty); }
        set { this.SetValue(BorderBrushProperty, value); }
    }

    /// <summary>
    ///   Define our border thickness property
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
        nameof(BorderThickness),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(2d, OnBorderThicknessChanged));
    public double BorderThickness
    {
        get { return (double)this.GetValue(BorderThicknessProperty); }
        set { this.SetValue(BorderThicknessProperty, value); }
    }
    static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mbc = d as MessageBar;
        if (mbc != null && e.NewValue != null)
        {
            mbc.MessageBorder.BorderThickness = new Thickness((double)e.NewValue);
        }
    }

    /// <summary>
    ///   Define our speed property.
    /// </summary>
    public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register(
        nameof(AnimationSpeed),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(500d));
    public double AnimationSpeed
    {
        get { return (double)this.GetValue(AnimationSpeedProperty); }
        set { this.SetValue(AnimationSpeedProperty, value); }
    }

    /// <summary>
    ///   Define our expansion property.
    /// </summary>
    /// <remarks>
    ///   The control must have a height value to render properly.
    /// </remarks>
    public static readonly DependencyProperty ExpandAmountProperty = DependencyProperty.Register(
        nameof(ExpandAmount),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(50d));
    public double ExpandAmount
    {
        get { return (double)this.GetValue(ExpandAmountProperty); }
        set { this.SetValue(ExpandAmountProperty, value); }
    }

    /// <summary>
    ///   Define our timeout property.
    /// </summary>
    /// <remarks>
    ///   If this value is zero, the message will show for an indefinite amount of time.
    /// </remarks>
    public static readonly DependencyProperty TimeoutMSProperty = DependencyProperty.Register(
        nameof(TimeoutMS),
        typeof(double),
        typeof(MessageBar),
        new PropertyMetadata(5000d));
    public double TimeoutMS
    {
        get { return (double)this.GetValue(TimeoutMSProperty); }
        set { this.SetValue(TimeoutMSProperty, value); }
    }
    #endregion

    public MessageBar()
    {
        InitializeComponent();
        this.Loaded += (s, e) => { this.Height = ExpandAmount; };
    }

    #region [Animated TextBlock]
    /// <summary>
    ///   Our animation logic for the message bar.
    /// </summary>
    public void ShowMessage(string msg, bool autoHide = true)
    {
        if (string.IsNullOrEmpty(msg))
            return;

        //Message.Text = msg;

        if (!_isShown) // reveal
        {
            _isShown = !_isShown;
            if (DesignerPane.ActualWidth != double.NaN && DesignerPane.ActualWidth > 3)
                MessagePane.Width = DesignerPane.ActualWidth - 3;
            MessagePane.Height = ExpandAmount;
            var yLocation = DesignerPane.ActualHeight;
            //MessagePane.Margin = new Thickness(3, 0, 0, 0);
            MessagePane.SetValue(Canvas.LeftProperty, DesignerPane.GetValue(Canvas.LeftProperty));
            MessagePane.SetValue(Canvas.TopProperty, yLocation);
            var dbGrow = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                FillBehavior = FillBehavior.HoldEnd,
                To = yLocation - ExpandAmount,
                EasingFunction = new SineEase()
            };
            dbGrow.Completed += (s, e) =>
            {
                if (autoHide)
                {
                    // Check for existing timers
                    if (_hideTimer != null)
                    {
                        _hideTimer.Stop();
                        _hideTimer = null;
                    }

                    _hideTimer = new System.Windows.Threading.DispatcherTimer();
                    _hideTimer.Interval = TimeSpan.FromMilliseconds(TimeoutMS);
                    _hideTimer.Tick += delegate
                    {
                        _hideTimer?.Stop();
                        if (_isShown) // Popup might be gone when timer completes, so check it again.
                        {
                            _isShown = !_isShown;
                            var dbShrink = new DoubleAnimation
                            {
                                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                                FillBehavior = FillBehavior.HoldEnd,
                                To = DesignerPane.ActualHeight,
                                From = DesignerPane.ActualHeight - ExpandAmount,
                                EasingFunction = new SineEase()
                            };
                            MessagePane.BeginAnimation(Canvas.TopProperty, dbShrink);
                        }
                        else
                        {
                            Debug.WriteLine($"[WARNING] message is already gone");
                        }
                    };
                    _hideTimer?.Start();
                }
                else
                {
                    Debug.WriteLine($"[INFO] auto-hide is disabled");
                }
            };
            // Start animation
            MessagePane.BeginAnimation(Canvas.TopProperty, dbGrow);
        }
        else // already shown
        {
            if (autoHide)
            {   // Check for existing timers
                if (_hideTimer != null)
                {
                    _hideTimer.Stop();
                    _hideTimer = null;
                }

                _hideTimer = new System.Windows.Threading.DispatcherTimer();
                _hideTimer.Interval = TimeSpan.FromMilliseconds(TimeoutMS);
                _hideTimer.Tick += delegate
                {
                    _hideTimer?.Stop();
                    if (_isShown) // Popup might be gone when timer completes, so check it again.
                    {
                        _isShown = !_isShown;
                        var dbShrink = new DoubleAnimation
                        {
                            Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                            FillBehavior = FillBehavior.HoldEnd,
                            To = DesignerPane.ActualHeight,
                            From = DesignerPane.ActualHeight - ExpandAmount,
                            EasingFunction = new SineEase()
                        };
                        MessagePane.BeginAnimation(Canvas.TopProperty, dbShrink);
                    }
                    else
                    {
                        Debug.WriteLine($"[WARNING] message is already gone");
                    }
                };
                _hideTimer?.Start();
            }
        }
    }

    /// <summary>
    ///   Similar to <see cref="ShowMessage(string, bool)"/>, but each call 
    ///   will hide or show the message bar depending on it's current state.
    /// </summary>
    public void ToggleMessage(string msg, bool autoHide = true)
    {
        if (string.IsNullOrEmpty(msg))
            return;

        //Message.Text = msg;

        if (!_isShown) // reveal
        {
            _isShown = !_isShown;
            if (DesignerPane.ActualWidth != double.NaN && DesignerPane.ActualWidth > 3)
                MessagePane.Width = DesignerPane.ActualWidth - 3;
            MessagePane.Height = ExpandAmount;
            var yLocation = DesignerPane.ActualHeight;
            //MessagePane.Margin = new Thickness(3, 0, 0, 0);
            MessagePane.SetValue(Canvas.LeftProperty, DesignerPane.GetValue(Canvas.LeftProperty));
            MessagePane.SetValue(Canvas.TopProperty, yLocation);
            var dbGrow = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                FillBehavior = FillBehavior.HoldEnd,
                To = yLocation - ExpandAmount,
                EasingFunction = new SineEase()
            };
            dbGrow.Completed += (s, e) =>
            {
                if (autoHide)
                {   // Check for existing timers
                    if (_hideTimer != null)
                    {
                        _hideTimer.Stop();
                        _hideTimer = null;
                    }

                    _hideTimer = new System.Windows.Threading.DispatcherTimer();
                    _hideTimer.Interval = TimeSpan.FromMilliseconds(TimeoutMS);
                    _hideTimer.Tick += delegate
                    {
                        _hideTimer?.Stop();
                        if (_isShown) // Popup might be gone when timer completes, so check it again.
                        {
                            _isShown = !_isShown;
                            var dbShrink = new DoubleAnimation
                            {
                                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                                FillBehavior = FillBehavior.HoldEnd,
                                To = DesignerPane.ActualHeight,
                                From = DesignerPane.ActualHeight - ExpandAmount,
                                EasingFunction = new SineEase()
                            };
                            MessagePane.BeginAnimation(Canvas.TopProperty, dbShrink);
                        }
                        else
                        {
                            Debug.WriteLine($"[WARNING] message is already gone");
                        }
                    };
                    _hideTimer?.Start();
                }
                else
                {
                    Debug.WriteLine($"[INFO] auto-hide is disabled");
                }
            };
            // Start animation
            MessagePane.BeginAnimation(Canvas.TopProperty, dbGrow);
        }
        else // hide
        {
            _isShown = !_isShown;
            var dbShrink = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                FillBehavior = FillBehavior.HoldEnd,
                To = DesignerPane.ActualHeight,
                From = DesignerPane.ActualHeight - ExpandAmount,
                EasingFunction = new SineEase()
            };
            // Start animation
            MessagePane.BeginAnimation(Canvas.TopProperty, dbShrink);
        }
    }
    #endregion
}
