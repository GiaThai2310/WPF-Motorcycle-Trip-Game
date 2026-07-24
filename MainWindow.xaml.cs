using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Motorcycle_Trip_Game.Controllers;
using WPF_Motorcycle_Trip_Game.Core;
using WPF_Motorcycle_Trip_Game.Managers;

namespace WPF_Motorcycle_Trip_Game;

public partial class MainWindow : Window
{
    private readonly GameEngine _gameEngine;

    public MainWindow()
    {
        InitializeComponent();

        // Inject a fallback procedural vector drawing if the actual image hasn't been added yet,
        // so the game is still playable for testing.
        if (BikeImage.Source == null)
        {
            BikeImage.Source = BuildBikeFallbackImage();
        }

        _gameEngine = new GameEngine(
            new BikeController(BikeImage),
            new ObstacleManager(GameCanvas),
            new ScoreManager(),
            new VisualManager(ScoreText, MessageText, RestartButton));
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
            case Key.Up:
                _gameEngine.Jump();
                e.Handled = true;
                break;

            case Key.R:
                _gameEngine.Restart();
                e.Handled = true;
                break;
        }
    }

    private void OnRestartClick(object sender, RoutedEventArgs e)
    {
        _gameEngine.Restart();
        Keyboard.Focus(this);
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _gameEngine.Dispose();
    }

    // -------------------------------------------------------------------------
    // Fallback procedural motorcycle drawing. 
    // This creates a static vector image of a motorcycle at startup.
    // This whole method and the call in the constructor can be deleted once 
    // the final motorcycle graphic is integrated.
    // -------------------------------------------------------------------------
    private static ImageSource BuildBikeFallbackImage()
    {
        var group = new DrawingGroup();

        using (DrawingContext dc = group.Open())
        {
            // ── Brushes / Pens ───────────────────────────────────────────────
            var wheelFill   = new SolidColorBrush(Color.FromRgb(35,  35,  35));
            var wheelRim    = new SolidColorBrush(Color.FromRgb(180, 180, 185));
            var frameFill   = new SolidColorBrush(Color.FromRgb(55,  55,  62));
            var tankFill    = new SolidColorBrush(Color.FromRgb(210, 55,  40));   // red tank
            var forkPen     = new Pen(new SolidColorBrush(Color.FromRgb(100, 100, 110)), 3);
            var exhaustPen  = new Pen(new SolidColorBrush(Color.FromRgb(180, 145, 55)), 2.5);
            var handlePen   = new Pen(new SolidColorBrush(Color.FromRgb(130, 130, 140)), 2.5);
            var skinBrush   = new SolidColorBrush(Color.FromRgb(255, 210, 170));
            var hairBrush   = new SolidColorBrush(Color.FromRgb(45,  28,  18));
            var riderJacket = new SolidColorBrush(Color.FromRgb(30,  50,  130));  // dark blue
            var helmetFill  = new SolidColorBrush(Color.FromRgb(25,  25,  80));
            var passFill    = new SolidColorBrush(Color.FromRgb(225, 150, 185));  // pink top
            var visorFill   = new SolidColorBrush(Color.FromArgb(170, 140, 190, 255));

            var framePen  = new Pen(new SolidColorBrush(Color.FromRgb(80, 80, 92)), 1);
            var tankPen   = new Pen(new SolidColorBrush(Color.FromRgb(155, 30, 20)), 1);
            var outlinePen = new Pen(new SolidColorBrush(Color.FromRgb(20, 20, 20)), 1.2);

            // ── Wheels ───────────────────────────────────────────────────────
            // Left (rear) wheel — centre (22, 63), r=14
            dc.DrawEllipse(wheelFill, outlinePen, new Point(22, 63), 14, 14);
            dc.DrawEllipse(wheelRim,  null,        new Point(22, 63),  4,  4);

            // Right (front) wheel — centre (97, 63), r=13
            dc.DrawEllipse(wheelFill, outlinePen, new Point(97, 63), 13, 13);
            dc.DrawEllipse(wheelRim,  null,        new Point(97, 63),  4,  4);

            // ── Frame / chassis ──────────────────────────────────────────────
            var frameGeom = new StreamGeometry();
            using (StreamGeometryContext ctx = frameGeom.Open())
            {
                ctx.BeginFigure(new Point(22, 63), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(32, 46), isStroked: true, isSmoothJoin: false);
                ctx.LineTo(new Point(52, 38), isStroked: true, isSmoothJoin: false);
                ctx.LineTo(new Point(84, 40), isStroked: true, isSmoothJoin: false);
                ctx.LineTo(new Point(97, 63), isStroked: true, isSmoothJoin: false);
                ctx.LineTo(new Point(72, 63), isStroked: true, isSmoothJoin: false);
                ctx.LineTo(new Point(38, 63), isStroked: true, isSmoothJoin: false);
            }
            dc.DrawGeometry(frameFill, framePen, frameGeom);

            // Fuel tank / body fairing
            dc.DrawRoundedRectangle(tankFill, tankPen, new Rect(38, 33, 46, 17), 3, 3);

            // Front fork
            dc.DrawLine(forkPen, new Point(84, 40), new Point(97, 57));

            // Handlebars
            dc.DrawLine(handlePen, new Point(79, 32), new Point(96, 31));

            // Exhaust pipe (rear-bottom)
            dc.DrawLine(exhaustPen, new Point(32, 56), new Point(22, 65));

            // ── Passenger (rear, pink top) ────────────────────────────────────
            // Passenger body
            dc.DrawRoundedRectangle(passFill, outlinePen, new Rect(41, 15, 14, 23), 2, 2);

            // Passenger head (skin)
            dc.DrawEllipse(skinBrush, outlinePen, new Point(48, 9), 7, 7);

            // Passenger hair (long, over shoulders)
            dc.DrawEllipse(hairBrush, null, new Point(48, 5), 7, 5);
            dc.DrawRoundedRectangle(hairBrush, null, new Rect(43, 9, 3, 9), 1, 1); // left side hair
            dc.DrawRoundedRectangle(hairBrush, null, new Rect(50, 9, 3, 7), 1, 1); // right side hair

            // Passenger arms reaching forward (holding rider's waist)
            dc.DrawLine(new Pen(skinBrush, 2.5), new Point(55, 22), new Point(63, 24));
            dc.DrawLine(new Pen(skinBrush, 2.5), new Point(55, 28), new Point(63, 30));

            // ── Rider (front, dark blue jacket, helmet) ──────────────────────
            // Rider body / jacket
            dc.DrawRoundedRectangle(riderJacket, outlinePen, new Rect(56, 12, 17, 26), 2, 2);

            // Rider helmet
            dc.DrawEllipse(helmetFill, outlinePen, new Point(64, 7), 9, 8);

            // Helmet visor (blue tint rectangle)
            dc.DrawRoundedRectangle(visorFill, null, new Rect(58, 3, 12, 6), 2, 2);

            // Rider arm to handlebar
            dc.DrawLine(
                new Pen(riderJacket, 4),
                new Point(73, 20), new Point(85, 30));
        }

        var image = new DrawingImage(group);
        image.Freeze(); // Make immutable — safe to share across threads/frames.
        return image;
    }
}
