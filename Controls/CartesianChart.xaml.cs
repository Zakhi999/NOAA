using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NOAA.Controls;

public partial class CartesianChart : UserControl
{
    #region [Properties]
    bool _constantTooltip = false;
    bool _animating = false;
    const double HitThreshold = 20.0; // in pixels
    long _minX; long _maxX; double _maxY; // control-wide scope

    public List<ChartSeries> Series
    {
        get => (List<ChartSeries>)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
        nameof(Series), 
        typeof(List<ChartSeries>),
        typeof(CartesianChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    DoubleAnimation? _fadeOutTooltip = null;
    #endregion

    public CartesianChart()
    {
        InitializeComponent();
        if (_fadeOutTooltip == null)
        {
            _fadeOutTooltip = new DoubleAnimation
            {
                From = PART_Tooltip.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            // We need to set visibility AFTER fade-out completes
            _fadeOutTooltip.Completed += (_, _) =>
            {
                if (PART_Tooltip.Visibility == Visibility.Visible)
                    PART_Tooltip.Visibility = Visibility.Collapsed;

                _animating = false;
            };
        }
        #region [Configure events]
        Loaded += (_, _) => Redraw();
        SizeChanged += (_, _) => Redraw();
        if (_constantTooltip)
            MouseMove += OnMouseMoveConstant;
        else
            MouseMove += OnMouseMove;
        MouseLeave += OnMouseLeave;
        #endregion
    }

    #region [Chart Methods]
    public void Redraw()
    {
        PART_Canvas.Children.Clear();
        if (Series == null || Series.Count == 0) 
            return;

        var points = Series[0].Points;
        if (points.Count < 2) 
            return;

        if (!_constantTooltip)
        {
            // Store scaling values for PlotX/PlotY
            _minX = points.Min(p => p.Time.Ticks);
            _maxX = points.Max(p => p.Time.Ticks);
            _maxY = points.Max(p => p.Value);
            if (_maxY <= 0)
                _maxY = 1; // avoid divide-by-zero
        }

        DrawGridlines(points);
        DrawAxes();
        DrawSeries();

        // Ensure dot is on top (z-order fix)
        PART_Canvas.Children.Remove(PART_HighlightDot);
        PART_Canvas.Children.Add(PART_HighlightDot);

        // Ensure tooltip is on top (z-order fix)
        PART_Canvas.Children.Remove(PART_Tooltip);
        PART_Canvas.Children.Add(PART_Tooltip);
    }

    void DrawAxes()
    {
        var g = new DrawingVisual();
        var dc = g.RenderOpen();
        var pen = new Pen(Brushes.Gray, 1);
        dc.DrawLine(pen, new Point(40, ActualHeight - 30), new Point(ActualWidth - 10, ActualHeight - 30)); // X axis
        dc.DrawLine(pen, new Point(40, 10), new Point(40, ActualHeight - 30)); // Y axis
        dc.Close();
        PART_Canvas.Children.Add(new VisualHost(g));
    }

    void DrawSeries()
    {
        bool _smoothJoin = true;
        foreach (var series in Series)
        {
            if (series.Points.Count < 2) 
                continue;

            var g = new DrawingVisual();
            var dc = g.RenderOpen();

            var minX = series.Points.Min(p => p.Time.Ticks);
            var maxX = series.Points.Max(p => p.Time.Ticks);
            var minY = 0.0;
            var maxY = series.Points.Max(p => p.Value);
            if (maxY <= 0) // Avoid divide-by-zero
                maxY = 1;

            double PlotX(DateTime t) => 40 + (ActualWidth - 50) * ((t.Ticks - minX) / (double)(maxX - minX));
            double PlotY(double v) => (ActualHeight - 30) - (ActualHeight - 40) * (v / maxY);

            // Easier than PathGeometry for our use case
            var geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                var first = series.Points[0];
                ctx.BeginFigure(new Point(PlotX(first.Time), PlotY(first.Value)), false, false);

                foreach (var p in series.Points.Skip(1))
                    ctx.LineTo(new Point(PlotX(p.Time), PlotY(p.Value)), true, _smoothJoin);
            }

            dc.DrawGeometry(null, new Pen(series.Stroke, series.StrokeThickness), geo);
            dc.Close();

            PART_Canvas.Children.Add(new VisualHost(g));
        }
    }

    void DrawGridlines(List<ChartPoint> points)
    {
        var g = new DrawingVisual();
        var dc = g.RenderOpen();

        // Add padding for labels
        double left = 40;
        double bottom = ActualHeight - 30;
        double top = 10;
        double right = ActualWidth - 10;

        long minX = points.Min(p => p.Time.Ticks);
        long maxX = points.Max(p => p.Time.Ticks);
        double minY = 0.0;
        double maxY = points.Max(p => p.Value);
        if (maxY <= 0) // Avoid divide-by-zero
            maxY = 1;

        // Choose gridline spacing/resolution
        // NOTE: If steps are too large then there won't be room for rendering label texts (or will overlap)
        int xSteps = 6;
        int ySteps = 6;

        Pen? gridPen;
        if (Series.Count > 0)
            gridPen = new Pen(Series[0].GridPen, Series[0].GridThickness);
        else
            gridPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1);

        // Horizontal gridlines + Y labels
        for (int i = 0; i <= ySteps; i++)
        {
            double yVal = (maxY / ySteps) * i;
            double y = bottom - (bottom - top) * (yVal / maxY);

            dc.DrawLine(gridPen, new Point(left, y), new Point(right, y));

            var label = new FormattedText(
                $"{yVal:0.0}",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                12,
                Brushes.LightGray,
                1.25);

            dc.DrawText(label, new Point(5, y - 8));
        }

        // Vertical gridlines + X labels
        for (int i = 0; i <= xSteps; i++)
        {
            long tick = minX + (long)((maxX - minX) * (i / (double)xSteps));
            DateTime t = new DateTime(tick);

            double x = left + (right - left) * (i / (double)xSteps);

            dc.DrawLine(gridPen, new Point(x, top), new Point(x, bottom));

            var label = new FormattedText(
                t.ToString("MM/dd\nHH:mm"),
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                11,
                Brushes.LightGray,
                1.25);

            dc.DrawText(label, new Point(x - 25, bottom + 5));
        }

        dc.Close();
        PART_Canvas.Children.Add(new VisualHost(g));
    }
    #endregion

    #region [Mouse Events]
    /// <summary>
    /// Tooltip is only visible if within HitThreshold
    /// </summary>
    void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (Series == null || Series.Count == 0)
            return;

        var pos = e.GetPosition(PART_Canvas);
        var series = Series[0];
        if (series.Points.Count < 2)
            return;

        // Find the closest point in X
        var closest = series.Points.OrderBy(p => Math.Abs(PlotX_Scoped(p.Time) - pos.X)).First();
        if (closest == null)
            return;

        // Compute the Y position of the line at that point
        double lineY = PlotY_Scoped(closest.Value);

        // Distance from mouse to line
        double dy = Math.Abs(lineY - pos.Y);

        if (dy <= HitThreshold)
        {
            // Position highlight dot
            double x = PlotX_Scoped(closest.Time);
            double y = PlotY_Scoped(closest.Value);
            Canvas.SetLeft(PART_HighlightDot, x - PART_HighlightDot.Width / 2);
            Canvas.SetTop(PART_HighlightDot, y - PART_HighlightDot.Height / 2);
            if (PART_HighlightDot.Visibility != Visibility.Visible)
            {
                PART_HighlightDot.Visibility = Visibility.Visible;
                StartDotPulse();
            }

            #region [Show tooltip]
            //Canvas.SetLeft(PART_Tooltip, pos.X + 20);
            //Canvas.SetTop(PART_Tooltip, pos.Y - 10);
            double tooltipWidth = PART_Tooltip.ActualWidth;
            double tooltipHeight = PART_Tooltip.ActualHeight;
            double offsetX = 20;// Default offset is tooltip on the right
            if (pos.X + tooltipWidth + offsetX > ActualWidth) // If tooltip overflows the right, flip it to the left
                offsetX = -tooltipWidth - 10;
            Canvas.SetLeft(PART_Tooltip, pos.X + offsetX);
            Canvas.SetTop(PART_Tooltip, pos.Y - tooltipHeight + 16);
            
            // TODO: Add date/time/uom formatting to ChartSeries model
            //PART_TooltipText.Text = $"{closest.Time:t}\n{closest.Value:0.00} in";
            PART_TooltipText.Text = $"{closest.Time.ToString("MM/dd h:mm tt")}\n{closest.Value:0.00} inches";
            
            if (PART_Tooltip.Visibility != Visibility.Visible)
            {
                PART_Tooltip.Visibility = Visibility.Visible;
                PART_Tooltip.Opacity = 0.0;
                FadeInTooltip();
            }
            #endregion
        }
        else
        {
            // Hide tooltip/dot when not near the line
            if (PART_HighlightDot.Visibility == Visibility.Visible)
            {
                StopDotPulse();
                PART_HighlightDot.Visibility = Visibility.Collapsed;
            }
            if (PART_Tooltip.Visibility == Visibility.Visible)
            {
                if (!_animating) // prevent double-trigger
                    FadeOutTooltip();
            }
        }
    }

    /// <summary>
    /// Tooltip is always visible
    /// </summary>
    void OnMouseMoveConstant(object sender, MouseEventArgs e)
    {
        if (Series == null || Series.Count == 0)
            return;

        var pos = e.GetPosition(this);
        var series = Series[0];

        var closest = series.Points
            .OrderBy(p => Math.Abs((40 + (ActualWidth - 50) *
                ((p.Time.Ticks - series.Points.Min(x => x.Time.Ticks)) /
                (double)(series.Points.Max(x => x.Time.Ticks) - series.Points.Min(x => x.Time.Ticks))) - pos.X)))
            .FirstOrDefault();

        if (closest == null)
            return;

        PART_Tooltip.Visibility = Visibility.Hidden;

        PART_TooltipText.Text = $"{closest.Time:t}\n{closest.Value:0.00} in";

        #region [Centering Tooltip]
        //double x = Math.Min(pos.X + 10, ActualWidth - PART_Tooltip.ActualWidth - 10);
        //double y = Math.Min(pos.Y - 10, ActualHeight - PART_Tooltip.ActualHeight - 10);
        //Canvas.SetLeft(PART_Tooltip, x);
        //Canvas.SetTop(PART_Tooltip, y);
        //Debug.WriteLine($"{closest.Time:t}\n{closest.Value:0.00} in ({x},{y})");
        //Panel.SetZIndex(PART_Tooltip, 9999);
        #endregion

        //Canvas.SetLeft(PART_Tooltip, pos.X + 10);
        //Canvas.SetTop(PART_Tooltip, pos.Y - 10);
        //Debug.WriteLine($"{closest.Time:t}\n{closest.Value:0.00} in ({pos.X},{pos.Y})");
        var posc = e.GetPosition(PART_Canvas);
        Canvas.SetLeft(PART_Tooltip, posc.X + 10);
        Canvas.SetTop(PART_Tooltip, posc.Y - 10);


        PART_Tooltip.Visibility = Visibility.Visible;
    }

    void OnMouseLeave(object sender, MouseEventArgs e)
    {
        StopDotPulse();
        FadeOutTooltip();
        PART_HighlightDot.Visibility = Visibility.Collapsed;
    }
    #endregion

    #region [Hit threshold helpers]
    double PlotX_Scoped(DateTime t)
    {
        long minX = _minX;
        long maxX = _maxX;
        return 40 + (ActualWidth - 50) * ((t.Ticks - minX) / (double)(maxX - minX));
    }

    double PlotY_Scoped(double v)
    {
        double maxY = _maxY;
        if (maxY <= 0) { maxY = 1; } // Avoid divide-by-zero
        return (ActualHeight - 30) - (ActualHeight - 40) * (v / maxY);
    }
    #endregion

    #region [Animation helpers]
    void StartDotPulse()
    {
        var anim = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromSeconds(0.6),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        PART_HighlightDot.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    void StopDotPulse()
    {
        PART_HighlightDot.BeginAnimation(UIElement.OpacityProperty, null);
        PART_HighlightDot.Opacity = 1.0;
    }

    void FadeInTooltip()
    {
        var anim = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.6),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        PART_Tooltip.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    void FadeOutTooltip()
    {
        //var anim = new DoubleAnimation
        //{
        //    From = PART_Tooltip.Opacity,
        //    To = 0.0,
        //    Duration = TimeSpan.FromSeconds(0.6),
        //    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        //};
        if (_fadeOutTooltip == null || _animating)
            return;
        
        _animating = true;
        PART_Tooltip.BeginAnimation(UIElement.OpacityProperty, _fadeOutTooltip);
    }
    #endregion
}

/// <summary>
/// We need a <see cref="VisualHost"/> class since WPF does not let us add <br/>
/// a <see cref="System.Windows.Media.DrawingVisual"/> directly into a Canvas/Panel/Grid. <br/>
/// A <see cref="System.Windows.Media.DrawingVisual"/>  is a visual, not a <see cref="System.Windows.UIElement"/>, <br/>
/// and Panels/Canvas/Grid only accept <see cref="System.Windows.UIElement"/> as <br/>
/// children, so our <see cref="VisualHost"/> acts as a bridge.
/// </summary>
public class VisualHost : FrameworkElement
{
    /* [Benefits of our VisualHost class]
    1. It wraps a DrawingVisual inside a FrameworkElement.
        - A FrameworkElement can be added to a Canvas, Grid, etc; a DrawingVisual cannot.
    2. It exposes the visual to WPF’s visual tree.
        - WPF needs a parent/child relationship to render visuals. DrawingVisual alone cannot be placed in the visual tree.
    3. It allows our chart to render hundreds of lines efficiently.
        - DrawingVisual is extremely fast because it bypasses layout, hit‑testing, and measurement. But because it’s not a UIElement, we need a host.
    4. It keeps our chart control lightweight.
        - Instead of creating hundreds of shapes, we draw everything into a single DrawingVisual (much faster).
    */
    readonly Visual _visual;
    public VisualHost(Visual visual) => _visual = visual;
    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _visual;
}
