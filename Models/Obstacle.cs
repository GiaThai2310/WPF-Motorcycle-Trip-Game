using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Models;

// Owned by: Vinh
// Defines individual obstacle dimensions, visual elements, animations, and tight hitboxes.
public sealed class Obstacle
{
    private static readonly Random SharedRandom = new();

    public ObstacleType Type { get; }
    public double X { get; private set; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }
    public UIElement VisualElement { get; }

    public Rect Bounds
    {
        get
        {
            return Type switch
            {
                ObstacleType.Pedestrian => new Rect(X + 6, Y + 5, Width - 12, Height - 7),
                ObstacleType.Pothole   => new Rect(X + 4, Y + 2, Width - 8, Height - 4),
                ObstacleType.Rock      => new Rect(X + 5, Y + 5, Width - 10, Height - 7),
                _                      => new Rect(X, Y, Width, Height)
            };
        }
    }

    public Obstacle(ObstacleType type, double startX)
    {
        Type = type;
        X = startX;

        switch (type)
        {
            case ObstacleType.Pedestrian:
                Width = 46;
                Height = 72;
                Y = GameConstants.GroundY - Height; // Feet touch the ground
                break;

            case ObstacleType.Pothole:
                Width = 65;
                Height = 18;
                Y = GameConstants.GroundY - 4; // Embedded IN the road surface, mostly below ground
                break;

            case ObstacleType.Rock:
            default:
                Width = 50;
                Height = 42;
                Y = GameConstants.GroundY - Height;
                break;
        }

        VisualElement = TryLoadAsset() ?? CreateFallbackVisual();

        Canvas.SetLeft(VisualElement, X);
        Canvas.SetTop(VisualElement, Y);

        ApplyAnimation();
    }

    /// <summary>
    /// Applies subtle WPF Storyboard animations to make obstacles feel alive.
    /// Uses WPF's built-in composition engine — does NOT create a DispatcherTimer.
    /// </summary>
    private void ApplyAnimation()
    {
        if (VisualElement is not FrameworkElement element) return;

        switch (Type)
        {
            case ObstacleType.Pedestrian:
                // Walking bob: gentle up-down oscillation (simulates walking motion)
                TranslateTransform walkTransform = new TranslateTransform();
                element.RenderTransform = walkTransform;

                DoubleAnimation walkBob = new DoubleAnimation
                {
                    From = 0,
                    To = -3,
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                walkTransform.BeginAnimation(TranslateTransform.YProperty, walkBob);
                break;

            case ObstacleType.Rock:
                // Only animate traffic cone (wobble) — rocks and bricks stay static
                // We detect cone by checking if the visual contains a Polygon (cone shape)
                if (element is Canvas rockCanvas && rockCanvas.Children.Count > 0
                    && rockCanvas.Children[0] is Polygon)
                {
                    RotateTransform wobble = new RotateTransform(0, Width / 2, Height);
                    element.RenderTransform = wobble;

                    DoubleAnimation wobbleAnim = new DoubleAnimation
                    {
                        From = -2,
                        To = 2,
                        Duration = TimeSpan.FromMilliseconds(400),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    wobble.BeginAnimation(RotateTransform.AngleProperty, wobbleAnim);
                }
                break;
        }
    }

    public void UpdatePosition(double newX)
    {
        X = newX;
        Canvas.SetLeft(VisualElement, X);
    }

    private UIElement? TryLoadAsset()
    {
        string assetName = Type switch
        {
            ObstacleType.Pedestrian => "pedestrian.png",
            ObstacleType.Pothole   => "pothole.png",
            ObstacleType.Rock      => "rock.png",
            _                      => ""
        };

        try
        {
            string uri = $"pack://application:,,,/Assets/{assetName}";
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            if (bitmap.PixelWidth == 0) return null;

            return new Image
            {
                Source = bitmap,
                Width = Width,
                Height = Height,
                Stretch = Stretch.Uniform
            };
        }
        catch
        {
            return null;
        }
    }

    private UIElement CreateFallbackVisual()
    {
        return Type switch
        {
            ObstacleType.Pedestrian => SharedRandom.Next(3) switch
            {
                0 => BuildFptStudentVisual(),
                1 => BuildRonaldoSiuuuVisual(),
                _ => BuildMessiVisual()
            },
            ObstacleType.Pothole => BuildPotholeVisual(),
            ObstacleType.Rock => SharedRandom.Next(3) switch
            {
                0 => BuildRockVisual(),
                1 => BuildBrickPileVisual(),
                _ => BuildTrafficConeVisual()
            },
            _ => BuildRockVisual()
        };
    }

    // ========================================================================
    // PEDESTRIAN VARIANT 1: FPT University Student (Áo cam FPT đeo balo)
    // ========================================================================
    private UIElement BuildFptStudentVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(255, 219, 172));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(40, 30, 20));
        SolidColorBrush fptOrange = new SolidColorBrush(Color.FromRgb(243, 112, 33));
        SolidColorBrush fptOrangeDark = new SolidColorBrush(Color.FromRgb(200, 85, 20));
        SolidColorBrush pants = new SolidColorBrush(Color.FromRgb(40, 45, 55));
        SolidColorBrush backpackBrush = new SolidColorBrush(Color.FromRgb(50, 50, 55));
        SolidColorBrush shoe = new SolidColorBrush(Color.FromRgb(35, 35, 35));

        double cx = Width / 2.0;

        // Hair
        Ellipse hair = new Ellipse { Width = 16, Height = 10, Fill = hairBrush };
        Canvas.SetLeft(hair, cx - 8);
        Canvas.SetTop(hair, 0);
        canvas.Children.Add(hair);

        // Head
        Ellipse head = new Ellipse
        {
            Width = 14, Height = 14, Fill = skin,
            Stroke = new SolidColorBrush(Color.FromRgb(200, 170, 130)), StrokeThickness = 0.5
        };
        Canvas.SetLeft(head, cx - 7);
        Canvas.SetTop(head, 2);
        canvas.Children.Add(head);

        // Neck
        Rectangle neck = new Rectangle { Width = 4, Height = 4, Fill = skin };
        Canvas.SetLeft(neck, cx - 2);
        Canvas.SetTop(neck, 15);
        canvas.Children.Add(neck);

        // FPT Orange Polo Shirt
        Rectangle torso = new Rectangle
        {
            Width = 18, Height = 20, Fill = fptOrange,
            Stroke = fptOrangeDark, StrokeThickness = 0.8,
            RadiusX = 2, RadiusY = 2
        };
        Canvas.SetLeft(torso, cx - 9);
        Canvas.SetTop(torso, 18);
        canvas.Children.Add(torso);

        // "FPT" text on shirt
        TextBlock fptLabel = new TextBlock
        {
            Text = "FPT",
            FontSize = 6, FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        Canvas.SetLeft(fptLabel, cx - 7);
        Canvas.SetTop(fptLabel, 23);
        canvas.Children.Add(fptLabel);

        // Polo collar
        Polygon collar = new Polygon
        {
            Points = new PointCollection
            {
                new Point(cx - 4, 18), new Point(cx, 21), new Point(cx + 4, 18)
            },
            Fill = new SolidColorBrush(Color.FromRgb(255, 140, 60)),
            Stroke = fptOrangeDark, StrokeThickness = 0.5
        };
        canvas.Children.Add(collar);

        // Backpack
        Rectangle bag = new Rectangle
        {
            Width = 10, Height = 16, Fill = backpackBrush,
            Stroke = new SolidColorBrush(Color.FromRgb(30, 30, 35)),
            StrokeThickness = 0.5, RadiusX = 2, RadiusY = 2
        };
        Canvas.SetLeft(bag, cx + 6);
        Canvas.SetTop(bag, 20);
        canvas.Children.Add(bag);

        // Backpack strap
        canvas.Children.Add(new Line
        {
            X1 = cx + 6, Y1 = 21, X2 = cx + 3, Y2 = 19,
            Stroke = backpackBrush, StrokeThickness = 1.5
        });

        // Left arm (swinging back)
        canvas.Children.Add(new Line
        {
            X1 = cx - 8, Y1 = 20, X2 = cx - 13, Y2 = 35,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Right arm (swinging forward)
        canvas.Children.Add(new Line
        {
            X1 = cx + 8, Y1 = 20, X2 = cx + 5, Y2 = 36,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Left leg (stepping forward)
        canvas.Children.Add(new Line
        {
            X1 = cx - 4, Y1 = 38, X2 = cx - 10, Y2 = 61,
            Stroke = pants, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Right leg (stepping back)
        canvas.Children.Add(new Line
        {
            X1 = cx + 4, Y1 = 38, X2 = cx + 8, Y2 = 60,
            Stroke = pants, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Left shoe — touching bottom
        Rectangle leftShoe = new Rectangle { Width = 9, Height = 5, Fill = shoe, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(leftShoe, cx - 14);
        Canvas.SetTop(leftShoe, Height - 6);
        canvas.Children.Add(leftShoe);

        // Right shoe — touching bottom
        Rectangle rightShoe = new Rectangle { Width = 9, Height = 5, Fill = shoe, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(rightShoe, cx + 5);
        Canvas.SetTop(rightShoe, Height - 7);
        canvas.Children.Add(rightShoe);

        return canvas;
    }

    // ========================================================================
    // PEDESTRIAN VARIANT 2: Cristiano Ronaldo — SIUUU celebration
    // Arms spread wide behind, legs apart, head back, #7 Portugal jersey
    // ========================================================================
    private UIElement BuildRonaldoSiuuuVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(210, 170, 130));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(30, 25, 18));
        SolidColorBrush jersey = new SolidColorBrush(Color.FromRgb(0, 100, 60));
        SolidColorBrush jerseyAccent = new SolidColorBrush(Color.FromRgb(220, 30, 30));
        SolidColorBrush shorts = new SolidColorBrush(Color.FromRgb(0, 85, 50));
        SolidColorBrush socks = new SolidColorBrush(Color.FromRgb(0, 100, 60));
        SolidColorBrush bootsBrush = new SolidColorBrush(Color.FromRgb(25, 25, 25));

        double cx = Width / 2.0;

        // "SIUUU!" label above head
        TextBlock siuLabel = new TextBlock
        {
            Text = "SIUUU!",
            FontSize = 7, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            FontStyle = FontStyles.Italic
        };
        Canvas.SetLeft(siuLabel, cx - 13);
        Canvas.SetTop(siuLabel, 0);
        canvas.Children.Add(siuLabel);

        // Hair (styled up)
        Rectangle hairTop = new Rectangle { Width = 14, Height = 6, Fill = hairBrush, RadiusX = 3, RadiusY = 3 };
        Canvas.SetLeft(hairTop, cx - 7);
        Canvas.SetTop(hairTop, 10);
        canvas.Children.Add(hairTop);

        // Head (tilted back)
        Ellipse head = new Ellipse
        {
            Width = 13, Height = 13, Fill = skin,
            Stroke = new SolidColorBrush(Color.FromRgb(180, 140, 100)), StrokeThickness = 0.5
        };
        Canvas.SetLeft(head, cx - 6.5);
        Canvas.SetTop(head, 13);
        canvas.Children.Add(head);

        // Mouth open (screaming SIUUU)
        Ellipse mouth = new Ellipse
        {
            Width = 4, Height = 3,
            Fill = new SolidColorBrush(Color.FromRgb(150, 50, 50))
        };
        Canvas.SetLeft(mouth, cx - 2);
        Canvas.SetTop(mouth, 21);
        canvas.Children.Add(mouth);

        // Neck
        Rectangle neck = new Rectangle { Width = 5, Height = 4, Fill = skin };
        Canvas.SetLeft(neck, cx - 2.5);
        Canvas.SetTop(neck, 25);
        canvas.Children.Add(neck);

        // Jersey (Portugal green #7)
        Rectangle torso = new Rectangle
        {
            Width = 18, Height = 18, Fill = jersey,
            Stroke = new SolidColorBrush(Color.FromRgb(0, 70, 40)),
            StrokeThickness = 0.8, RadiusX = 2, RadiusY = 2
        };
        Canvas.SetLeft(torso, cx - 9);
        Canvas.SetTop(torso, 28);
        canvas.Children.Add(torso);

        // #7 gold number
        TextBlock number = new TextBlock
        {
            Text = "7", FontSize = 9, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0))
        };
        Canvas.SetLeft(number, cx - 3);
        Canvas.SetTop(number, 30);
        canvas.Children.Add(number);

        // Red collar accents
        canvas.Children.Add(new Line { X1 = cx - 5, Y1 = 28, X2 = cx, Y2 = 30, Stroke = jerseyAccent, StrokeThickness = 1.5 });
        canvas.Children.Add(new Line { X1 = cx + 5, Y1 = 28, X2 = cx, Y2 = 30, Stroke = jerseyAccent, StrokeThickness = 1.5 });

        // SIUUU Arms spread wide behind
        canvas.Children.Add(new Line
        {
            X1 = cx - 9, Y1 = 31, X2 = cx - 21, Y2 = 40,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });
        // Left fingers
        canvas.Children.Add(new Line { X1 = cx - 21, Y1 = 40, X2 = cx - 23, Y2 = 38, Stroke = skin, StrokeThickness = 1.5 });
        canvas.Children.Add(new Line { X1 = cx - 21, Y1 = 40, X2 = cx - 23, Y2 = 41, Stroke = skin, StrokeThickness = 1.5 });

        canvas.Children.Add(new Line
        {
            X1 = cx + 9, Y1 = 31, X2 = cx + 21, Y2 = 40,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });
        // Right fingers
        canvas.Children.Add(new Line { X1 = cx + 21, Y1 = 40, X2 = cx + 23, Y2 = 38, Stroke = skin, StrokeThickness = 1.5 });
        canvas.Children.Add(new Line { X1 = cx + 21, Y1 = 40, X2 = cx + 23, Y2 = 41, Stroke = skin, StrokeThickness = 1.5 });

        // Shorts
        Rectangle shortsRect = new Rectangle { Width = 16, Height = 8, Fill = shorts, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(shortsRect, cx - 8);
        Canvas.SetTop(shortsRect, 46);
        canvas.Children.Add(shortsRect);

        // Left leg (apart — SIUUU landing)
        canvas.Children.Add(new Line
        {
            X1 = cx - 5, Y1 = 54, X2 = cx - 12, Y2 = Height - 6,
            Stroke = socks, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Right leg (apart — SIUUU landing)
        canvas.Children.Add(new Line
        {
            X1 = cx + 5, Y1 = 54, X2 = cx + 12, Y2 = Height - 6,
            Stroke = socks, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Left boot — touching bottom of canvas = touching GroundY
        Rectangle leftBoot = new Rectangle { Width = 9, Height = 5, Fill = bootsBrush, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(leftBoot, cx - 16);
        Canvas.SetTop(leftBoot, Height - 6);
        canvas.Children.Add(leftBoot);

        // Right boot — touching bottom
        Rectangle rightBoot = new Rectangle { Width = 9, Height = 5, Fill = bootsBrush, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(rightBoot, cx + 8);
        Canvas.SetTop(rightBoot, Height - 6);
        canvas.Children.Add(rightBoot);

        return canvas;
    }

    // ========================================================================
    // PEDESTRIAN VARIANT 3: Lionel Messi — Pointing to the sky celebration
    // Argentina #10 jersey, both hands pointing up, calm GOATed stance
    // ========================================================================
    private UIElement BuildMessiVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(230, 190, 150));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(35, 28, 20));
        SolidColorBrush beardBrush = new SolidColorBrush(Color.FromRgb(60, 50, 40));
        SolidColorBrush jerseyBlue = new SolidColorBrush(Color.FromRgb(108, 172, 228));  // Argentina sky blue
        SolidColorBrush jerseyWhite = new SolidColorBrush(Color.FromRgb(245, 245, 250)); // Argentina white stripe
        SolidColorBrush shortsDark = new SolidColorBrush(Color.FromRgb(20, 20, 45));
        SolidColorBrush socksBrush = new SolidColorBrush(Color.FromRgb(240, 240, 245));
        SolidColorBrush bootsBrush = new SolidColorBrush(Color.FromRgb(25, 25, 25));

        double cx = Width / 2.0;

        // "G.O.A.T" label above head
        TextBlock goatLabel = new TextBlock
        {
            Text = "G.O.A.T",
            FontSize = 6.5, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            FontStyle = FontStyles.Italic
        };
        Canvas.SetLeft(goatLabel, cx - 14);
        Canvas.SetTop(goatLabel, 0);
        canvas.Children.Add(goatLabel);

        // Hair (Messi's longer messy style)
        Ellipse hair = new Ellipse { Width = 17, Height = 12, Fill = hairBrush };
        Canvas.SetLeft(hair, cx - 8.5);
        Canvas.SetTop(hair, 8);
        canvas.Children.Add(hair);

        // Head
        Ellipse head = new Ellipse
        {
            Width = 14, Height = 14, Fill = skin,
            Stroke = new SolidColorBrush(Color.FromRgb(190, 155, 120)), StrokeThickness = 0.5
        };
        Canvas.SetLeft(head, cx - 7);
        Canvas.SetTop(head, 12);
        canvas.Children.Add(head);

        // Beard
        Ellipse beard = new Ellipse
        {
            Width = 10, Height = 6,
            Fill = beardBrush
        };
        Canvas.SetLeft(beard, cx - 5);
        Canvas.SetTop(beard, 21);
        canvas.Children.Add(beard);

        // Neck
        Rectangle neck = new Rectangle { Width = 4, Height = 4, Fill = skin };
        Canvas.SetLeft(neck, cx - 2);
        Canvas.SetTop(neck, 25);
        canvas.Children.Add(neck);

        // Argentina jersey — sky blue with white center stripe
        Rectangle torsoBlue = new Rectangle
        {
            Width = 18, Height = 19, Fill = jerseyBlue,
            Stroke = new SolidColorBrush(Color.FromRgb(80, 140, 200)),
            StrokeThickness = 0.8, RadiusX = 2, RadiusY = 2
        };
        Canvas.SetLeft(torsoBlue, cx - 9);
        Canvas.SetTop(torsoBlue, 28);
        canvas.Children.Add(torsoBlue);

        // White vertical stripe (center of jersey)
        Rectangle whiteStripe = new Rectangle
        {
            Width = 6, Height = 19, Fill = jerseyWhite
        };
        Canvas.SetLeft(whiteStripe, cx - 3);
        Canvas.SetTop(whiteStripe, 28);
        canvas.Children.Add(whiteStripe);

        // #10 on jersey
        TextBlock number = new TextBlock
        {
            Text = "10", FontSize = 8, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(20, 20, 80))
        };
        Canvas.SetLeft(number, cx - 5);
        Canvas.SetTop(number, 32);
        canvas.Children.Add(number);

        // Both arms POINTING UP to the sky (Messi celebration)
        // Left arm up
        canvas.Children.Add(new Line
        {
            X1 = cx - 8, Y1 = 30, X2 = cx - 12, Y2 = 15,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });
        // Left index finger pointing up
        canvas.Children.Add(new Line
        {
            X1 = cx - 12, Y1 = 15, X2 = cx - 12, Y2 = 10,
            Stroke = skin, StrokeThickness = 2,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Right arm up
        canvas.Children.Add(new Line
        {
            X1 = cx + 8, Y1 = 30, X2 = cx + 12, Y2 = 15,
            Stroke = skin, StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });
        // Right index finger pointing up
        canvas.Children.Add(new Line
        {
            X1 = cx + 12, Y1 = 15, X2 = cx + 12, Y2 = 10,
            Stroke = skin, StrokeThickness = 2,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Shorts
        Rectangle shortsRect = new Rectangle { Width = 16, Height = 8, Fill = shortsDark, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(shortsRect, cx - 8);
        Canvas.SetTop(shortsRect, 47);
        canvas.Children.Add(shortsRect);

        // Left leg (standing straight, calm pose)
        canvas.Children.Add(new Line
        {
            X1 = cx - 4, Y1 = 55, X2 = cx - 5, Y2 = Height - 7,
            Stroke = socksBrush, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Right leg (slightly apart)
        canvas.Children.Add(new Line
        {
            X1 = cx + 4, Y1 = 55, X2 = cx + 6, Y2 = Height - 7,
            Stroke = socksBrush, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // Left boot — touching bottom
        Rectangle leftBoot = new Rectangle { Width = 9, Height = 5, Fill = bootsBrush, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(leftBoot, cx - 9);
        Canvas.SetTop(leftBoot, Height - 6);
        canvas.Children.Add(leftBoot);

        // Right boot — touching bottom
        Rectangle rightBoot = new Rectangle { Width = 9, Height = 5, Fill = bootsBrush, RadiusX = 2, RadiusY = 2 };
        Canvas.SetLeft(rightBoot, cx + 2);
        Canvas.SetTop(rightBoot, Height - 6);
        canvas.Children.Add(rightBoot);

        return canvas;
    }

    // ========================================================================
    // POTHOLE: Deep hole embedded IN the road surface, below GroundY
    // ========================================================================
    private UIElement BuildPotholeVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        // Outer asphalt crack ring
        Ellipse outerCrack = new Ellipse
        {
            Width = Width, Height = Height,
            Fill = new SolidColorBrush(Color.FromRgb(80, 75, 70)),
            Stroke = new SolidColorBrush(Color.FromRgb(50, 48, 45)),
            StrokeThickness = 1.5,
            StrokeDashArray = new DoubleCollection { 3, 2 }
        };
        canvas.Children.Add(outerCrack);

        // Inner dark hole (depth illusion)
        Ellipse innerHole = new Ellipse
        {
            Width = Width * 0.7, Height = Height * 0.6,
            Fill = new SolidColorBrush(Color.FromRgb(15, 12, 10))
        };
        Canvas.SetLeft(innerHole, Width * 0.15);
        Canvas.SetTop(innerHole, Height * 0.2);
        canvas.Children.Add(innerHole);

        // Highlight edge
        Ellipse highlight = new Ellipse
        {
            Width = Width * 0.3, Height = Height * 0.25,
            Fill = new SolidColorBrush(Color.FromArgb(60, 180, 175, 165))
        };
        Canvas.SetLeft(highlight, Width * 0.12);
        Canvas.SetTop(highlight, Height * 0.1);
        canvas.Children.Add(highlight);

        // Crack lines
        SolidColorBrush crackBrush = new SolidColorBrush(Color.FromRgb(45, 42, 38));
        canvas.Children.Add(new Line { X1 = 5, Y1 = Height / 2, X2 = 0, Y2 = 2, Stroke = crackBrush, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = Width - 8, Y1 = Height / 2, X2 = Width, Y2 = 3, Stroke = crackBrush, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = Width / 2, Y1 = Height - 2, X2 = Width * 0.3, Y2 = Height, Stroke = crackBrush, StrokeThickness = 0.8 });

        return canvas;
    }

    // ========================================================================
    // ROCK: Craggy boulder with shading, highlights, surface cracks
    // ========================================================================
    private UIElement BuildRockVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        Polygon rockBody = new Polygon
        {
            Points = new PointCollection
            {
                new Point(Width * 0.1, Height * 0.85),
                new Point(Width * 0.0, Height * 0.55),
                new Point(Width * 0.15, Height * 0.2),
                new Point(Width * 0.4, Height * 0.05),
                new Point(Width * 0.7, Height * 0.0),
                new Point(Width * 0.9, Height * 0.15),
                new Point(Width * 1.0, Height * 0.5),
                new Point(Width * 0.95, Height * 0.85),
                new Point(Width * 0.5, Height * 1.0)
            },
            Fill = new SolidColorBrush(Color.FromRgb(130, 125, 115)),
            Stroke = new SolidColorBrush(Color.FromRgb(90, 85, 78)),
            StrokeThickness = 1.5
        };
        canvas.Children.Add(rockBody);

        Polygon shadow = new Polygon
        {
            Points = new PointCollection
            {
                new Point(Width * 0.5, Height * 1.0),
                new Point(Width * 0.95, Height * 0.85),
                new Point(Width * 1.0, Height * 0.5),
                new Point(Width * 0.9, Height * 0.15),
                new Point(Width * 0.65, Height * 0.45),
                new Point(Width * 0.55, Height * 0.7)
            },
            Fill = new SolidColorBrush(Color.FromArgb(70, 40, 35, 30))
        };
        canvas.Children.Add(shadow);

        Polygon highlightPoly = new Polygon
        {
            Points = new PointCollection
            {
                new Point(Width * 0.15, Height * 0.2),
                new Point(Width * 0.4, Height * 0.05),
                new Point(Width * 0.7, Height * 0.0),
                new Point(Width * 0.5, Height * 0.3),
                new Point(Width * 0.25, Height * 0.35)
            },
            Fill = new SolidColorBrush(Color.FromArgb(50, 200, 195, 185))
        };
        canvas.Children.Add(highlightPoly);

        SolidColorBrush crackBrush = new SolidColorBrush(Color.FromRgb(85, 80, 72));
        canvas.Children.Add(new Line { X1 = Width * 0.3, Y1 = Height * 0.3, X2 = Width * 0.55, Y2 = Height * 0.6, Stroke = crackBrush, StrokeThickness = 0.8 });
        canvas.Children.Add(new Line { X1 = Width * 0.6, Y1 = Height * 0.2, X2 = Width * 0.7, Y2 = Height * 0.55, Stroke = crackBrush, StrokeThickness = 0.7 });

        Ellipse pebble = new Ellipse { Width = 6, Height = 4, Fill = new SolidColorBrush(Color.FromRgb(110, 105, 95)) };
        Canvas.SetLeft(pebble, Width * 0.2);
        Canvas.SetTop(pebble, Height * 0.6);
        canvas.Children.Add(pebble);

        return canvas;
    }

    // ========================================================================
    // ROCK VARIANT 2: Đống gạch đỏ (Red Brick Pile)
    // ========================================================================
    private UIElement BuildBrickPileVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        SolidColorBrush brick = new SolidColorBrush(Color.FromRgb(180, 70, 50));
        SolidColorBrush brickDark = new SolidColorBrush(Color.FromRgb(140, 50, 35));
        SolidColorBrush mortar = new SolidColorBrush(Color.FromRgb(190, 180, 165));

        // Bottom row — 3 bricks
        double bw = 16; double bh = 10;
        double baseY = Height - bh;

        Rectangle b1 = new Rectangle { Width = bw, Height = bh, Fill = brick, Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b1, 2); Canvas.SetTop(b1, baseY);
        canvas.Children.Add(b1);

        Rectangle b2 = new Rectangle { Width = bw, Height = bh, Fill = brickDark, Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b2, 17); Canvas.SetTop(b2, baseY);
        canvas.Children.Add(b2);

        Rectangle b3 = new Rectangle { Width = bw, Height = bh, Fill = brick, Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b3, 32); Canvas.SetTop(b3, baseY);
        canvas.Children.Add(b3);

        // Middle row — 2 bricks offset
        double midY = baseY - bh + 1;
        Rectangle b4 = new Rectangle { Width = bw, Height = bh, Fill = brickDark, Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b4, 9); Canvas.SetTop(b4, midY);
        canvas.Children.Add(b4);

        Rectangle b5 = new Rectangle { Width = bw, Height = bh, Fill = brick, Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b5, 24); Canvas.SetTop(b5, midY);
        canvas.Children.Add(b5);

        // Top row — 1 brick
        double topY = midY - bh + 1;
        Rectangle b6 = new Rectangle { Width = bw, Height = bh, Fill = new SolidColorBrush(Color.FromRgb(165, 60, 42)), Stroke = mortar, StrokeThickness = 0.8, RadiusX = 1, RadiusY = 1 };
        Canvas.SetLeft(b6, 16); Canvas.SetTop(b6, topY);
        canvas.Children.Add(b6);

        // Scattered crumble pieces
        Ellipse piece1 = new Ellipse { Width = 5, Height = 4, Fill = brickDark };
        Canvas.SetLeft(piece1, 0); Canvas.SetTop(piece1, Height - 5);
        canvas.Children.Add(piece1);

        Ellipse piece2 = new Ellipse { Width = 4, Height = 3, Fill = brick };
        Canvas.SetLeft(piece2, Width - 6); Canvas.SetTop(piece2, Height - 4);
        canvas.Children.Add(piece2);

        return canvas;
    }

    // ========================================================================
    // ROCK VARIANT 3: Cọc tiêu giao thông (Traffic Cone with stripes)
    // ========================================================================
    private UIElement BuildTrafficConeVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };

        SolidColorBrush coneOrange = new SolidColorBrush(Color.FromRgb(255, 100, 0));
        SolidColorBrush coneWhite = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        SolidColorBrush coneDark = new SolidColorBrush(Color.FromRgb(200, 70, 0));
        SolidColorBrush baseBrush = new SolidColorBrush(Color.FromRgb(55, 55, 60));

        double cx = Width / 2.0;

        // Cone tip (top)
        Polygon coneBody = new Polygon
        {
            Points = new PointCollection
            {
                new Point(cx, 2),
                new Point(cx - 14, Height - 8),
                new Point(cx + 14, Height - 8)
            },
            Fill = coneOrange,
            Stroke = coneDark,
            StrokeThickness = 1
        };
        canvas.Children.Add(coneBody);

        // White reflective stripe — upper
        Polygon stripe1 = new Polygon
        {
            Points = new PointCollection
            {
                new Point(cx - 5, Height * 0.35),
                new Point(cx + 5, Height * 0.35),
                new Point(cx + 7, Height * 0.48),
                new Point(cx - 7, Height * 0.48)
            },
            Fill = coneWhite
        };
        canvas.Children.Add(stripe1);

        // White reflective stripe — lower
        Polygon stripe2 = new Polygon
        {
            Points = new PointCollection
            {
                new Point(cx - 9, Height * 0.58),
                new Point(cx + 9, Height * 0.58),
                new Point(cx + 11, Height * 0.71),
                new Point(cx - 11, Height * 0.71)
            },
            Fill = coneWhite
        };
        canvas.Children.Add(stripe2);

        // Square base
        Rectangle coneBase = new Rectangle
        {
            Width = Width * 0.75, Height = 7,
            Fill = baseBrush,
            Stroke = new SolidColorBrush(Color.FromRgb(35, 35, 40)),
            StrokeThickness = 1,
            RadiusX = 1, RadiusY = 1
        };
        Canvas.SetLeft(coneBase, (Width - Width * 0.75) / 2);
        Canvas.SetTop(coneBase, Height - 8);
        canvas.Children.Add(coneBase);

        return canvas;
    }
}
