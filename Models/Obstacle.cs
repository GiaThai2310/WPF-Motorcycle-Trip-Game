using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Models;

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
                ObstacleType.Pothole   => new Rect(X + 4, GameConstants.GroundY - 14, Width - 8, 20),
                ObstacleType.Rock      => new Rect(X + 5, Y + 5, Width - 10, Height - 7),
                ObstacleType.Ufo       => new Rect(X + 5, Y + 5, Width - 10, Height - 10),
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

            case ObstacleType.Ufo:
                Width = 60;
                Height = 30;
                Y = 210; // Hovers at Y=210. Bottom bounds = 240. Grounded bike top bounds = 260.
                break;

            case ObstacleType.Rock:
            default:
                Width = 50;
                Height = 42;
                Y = GameConstants.GroundY - Height;
                break;
        }

        VisualElement = CreateVisual();

        Canvas.SetLeft(VisualElement, X);
        Canvas.SetTop(VisualElement, Y);

        ApplyAnimation();
    }

    // Applies subtle composition-based WPF animations to pedestrian and cone obstacles.
    private void ApplyAnimation()
    {
        if (VisualElement is not FrameworkElement element) return;

        switch (Type)
        {
            case ObstacleType.Pedestrian:
                // Applies walking bob animation simulating pedestrian step movement.
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
                // Applies gentle wobble animation to traffic cone variants.
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

    // Updates left canvas position of visual element.
    public void UpdatePosition(double newX)
    {
        X = newX;
        Canvas.SetLeft(VisualElement, X);
    }

    // Randomly constructs visual presentation using asset images or custom vector variants.
    private UIElement CreateVisual()
    {
        switch (Type)
        {
            case ObstacleType.Pedestrian:
                // Randomly selects 1 of 4 pedestrian variants, prioritizing newly generated image assets over code fallbacks.
                int pedVariant = SharedRandom.Next(4);
                if (pedVariant == 0) return TryLoadAsset("pedestrian.png") ?? BuildFptStudentVisual();
                if (pedVariant == 1) return TryLoadAsset("fpt_student.png") ?? BuildFptStudentVisual();
                if (pedVariant == 2) return TryLoadAsset("ronaldo.png") ?? BuildRonaldoVisual();
                return TryLoadAsset("messi.png") ?? BuildMessiVisual();

            case ObstacleType.Rock:
                // Randomly selects 1 of 4 rock visual render variants: PNG asset, Boulder, Brick Pile, or Traffic Cone.
                int rockVariant = SharedRandom.Next(4);
                if (rockVariant == 0)
                {
                    UIElement? pngVisual = TryLoadAsset("rock.png");
                    if (pngVisual != null) return pngVisual;
                }
                return (rockVariant % 3) switch
                {
                    0 => BuildRockVisual(),
                    1 => BuildBrickPileVisual(),
                    _ => BuildTrafficConeVisual()
                };

            case ObstacleType.Ufo:
                return BuildUfoVisual();

            case ObstacleType.Pothole:
            default:
                // Attempts loading pothole PNG asset first, falling back to embedded pothole vector visual.
                return TryLoadAsset("pothole.png") ?? BuildPotholeVisual();
        }
    }

    // Attempts loading a PNG bitmap asset from application pack resources.
    private UIElement? TryLoadAsset(string assetName)
    {
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
                Stretch = Stretch.Fill
            };
        }
        catch
        {
            return null;
        }
    }

    // ========================================================================
    // PEDESTRIAN VARIANT 1: FPT University Student (Áo cam FPT đeo balo)
    // ========================================================================
    private UIElement BuildFptStudentVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };
        RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(255, 219, 172));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(40, 30, 20));
        SolidColorBrush fptOrange = new SolidColorBrush(Color.FromRgb(243, 112, 33));
        SolidColorBrush fptOrangeDark = new SolidColorBrush(Color.FromRgb(200, 85, 20));
        SolidColorBrush pants = new SolidColorBrush(Color.FromRgb(40, 45, 55));
        SolidColorBrush backpackBrush = new SolidColorBrush(Color.FromRgb(50, 50, 55));
        SolidColorBrush shoe = new SolidColorBrush(Color.FromRgb(240, 240, 240));

        double cx = Width / 2.0;

        // Pixel-art Hair block
        Rectangle hair = new Rectangle { Width = 16, Height = 8, Fill = hairBrush };
        Canvas.SetLeft(hair, cx - 8);
        Canvas.SetTop(hair, 0);
        canvas.Children.Add(hair);

        // Pixel-art Head block
        Rectangle head = new Rectangle { Width = 14, Height = 14, Fill = skin };
        Canvas.SetLeft(head, cx - 7);
        Canvas.SetTop(head, 4);
        canvas.Children.Add(head);

        // Pixel-art Neck
        Rectangle neck = new Rectangle { Width = 4, Height = 3, Fill = skin };
        Canvas.SetLeft(neck, cx - 2);
        Canvas.SetTop(neck, 18);
        canvas.Children.Add(neck);

        // FPT Orange Polo Shirt (pixel block)
        Rectangle torso = new Rectangle { Width = 18, Height = 22, Fill = fptOrange };
        Canvas.SetLeft(torso, cx - 9);
        Canvas.SetTop(torso, 21);
        canvas.Children.Add(torso);

        // Dark collar trim (pixel block)
        Rectangle collar = new Rectangle { Width = 8, Height = 3, Fill = fptOrangeDark };
        Canvas.SetLeft(collar, cx - 4);
        Canvas.SetTop(collar, 21);
        canvas.Children.Add(collar);

        // "FPT" pixel badge text
        TextBlock fptLabel = new TextBlock
        {
            Text = "FPT",
            FontSize = 7, FontWeight = FontWeights.ExtraBold,
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Segoe UI")
        };
        Canvas.SetLeft(fptLabel, cx - 7);
        Canvas.SetTop(fptLabel, 26);
        canvas.Children.Add(fptLabel);

        // Backpack (pixel block)
        Rectangle bag = new Rectangle { Width = 8, Height = 18, Fill = backpackBrush };
        Canvas.SetLeft(bag, cx + 6);
        Canvas.SetTop(bag, 23);
        canvas.Children.Add(bag);

        // Left arm (pixel block)
        Rectangle leftArm = new Rectangle { Width = 4, Height = 16, Fill = skin };
        Canvas.SetLeft(leftArm, cx - 13);
        Canvas.SetTop(leftArm, 23);
        canvas.Children.Add(leftArm);

        // Right arm (pixel block)
        Rectangle rightArm = new Rectangle { Width = 4, Height = 16, Fill = skin };
        Canvas.SetLeft(rightArm, cx + 9);
        Canvas.SetTop(rightArm, 23);
        canvas.Children.Add(rightArm);

        // Pixel-art Pants (legs blocks)
        Rectangle leftLeg = new Rectangle { Width = 6, Height = 22, Fill = pants };
        Canvas.SetLeft(leftLeg, cx - 8);
        Canvas.SetTop(leftLeg, 43);
        canvas.Children.Add(leftLeg);

        Rectangle rightLeg = new Rectangle { Width = 6, Height = 22, Fill = pants };
        Canvas.SetLeft(rightLeg, cx + 2);
        Canvas.SetTop(rightLeg, 43);
        canvas.Children.Add(rightLeg);

        // Pixel-art Sneakers
        Rectangle leftShoe = new Rectangle { Width = 8, Height = 5, Fill = shoe };
        Canvas.SetLeft(leftShoe, cx - 10);
        Canvas.SetTop(leftShoe, Height - 5);
        canvas.Children.Add(leftShoe);

        Rectangle rightShoe = new Rectangle { Width = 8, Height = 5, Fill = shoe };
        Canvas.SetLeft(rightShoe, cx + 2);
        Canvas.SetTop(rightShoe, Height - 5);
        canvas.Children.Add(rightShoe);

        return canvas;
    }

    private UIElement BuildUfoVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };
        RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

        SolidColorBrush metalBase = new SolidColorBrush(Color.FromRgb(150, 160, 170));
        SolidColorBrush metalDark = new SolidColorBrush(Color.FromRgb(100, 110, 120));
        SolidColorBrush glass = new SolidColorBrush(Color.FromRgb(120, 220, 255));
        SolidColorBrush lightGreen = new SolidColorBrush(Color.FromRgb(0, 255, 100));

        // Glass Dome (pixel blocks)
        Rectangle dome = new Rectangle { Width = 24, Height = 12, Fill = glass };
        Canvas.SetLeft(dome, 18);
        Canvas.SetTop(dome, 0);
        canvas.Children.Add(dome);

        Rectangle domeTop = new Rectangle { Width = 16, Height = 4, Fill = glass };
        Canvas.SetLeft(domeTop, 22);
        Canvas.SetTop(domeTop, -4);
        canvas.Children.Add(domeTop);

        // Alien Silhouette inside dome
        Rectangle alien = new Rectangle { Width = 8, Height = 8, Fill = new SolidColorBrush(Color.FromRgb(50, 150, 50)) };
        Canvas.SetLeft(alien, 26);
        Canvas.SetTop(alien, 4);
        canvas.Children.Add(alien);

        // Saucer Metal Body (pixel blocks)
        Rectangle saucerTop = new Rectangle { Width = 40, Height = 6, Fill = metalBase };
        Canvas.SetLeft(saucerTop, 10);
        Canvas.SetTop(saucerTop, 12);
        canvas.Children.Add(saucerTop);

        Rectangle saucerMid = new Rectangle { Width = 56, Height = 8, Fill = metalDark };
        Canvas.SetLeft(saucerMid, 2);
        Canvas.SetTop(saucerMid, 18);
        canvas.Children.Add(saucerMid);

        Rectangle saucerBot = new Rectangle { Width = 32, Height = 4, Fill = metalBase };
        Canvas.SetLeft(saucerBot, 14);
        Canvas.SetTop(saucerBot, 26);
        canvas.Children.Add(saucerBot);

        // UFO Lights (green glowing pixel blocks)
        for (int i = 0; i < 4; i++)
        {
            Rectangle light = new Rectangle { Width = 4, Height = 4, Fill = lightGreen };
            Canvas.SetLeft(light, 8 + (i * 14));
            Canvas.SetTop(light, 20);
            canvas.Children.Add(light);
        }

        return canvas;
    }

    // ========================================================================
    // PEDESTRIAN VARIANT 2: Cristiano Ronaldo — SIUUU celebration
    // Arms spread wide behind, legs apart, head back, #7 Portugal jersey
    // ========================================================================
    private UIElement BuildRonaldoVisual()
    {
        Canvas canvas = new Canvas { Width = Width, Height = Height };
        RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(210, 170, 130));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(30, 25, 18));
        SolidColorBrush jersey = new SolidColorBrush(Color.FromRgb(0, 100, 60));
        SolidColorBrush shorts = new SolidColorBrush(Color.FromRgb(0, 85, 50));
        SolidColorBrush bootsBrush = new SolidColorBrush(Color.FromRgb(25, 25, 25));

        double cx = Width / 2.0;

        // "SIUUU!" pixel banner label
        TextBlock siuLabel = new TextBlock
        {
            Text = "SIUUU!",
            FontSize = 8, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            FontFamily = new FontFamily("Segoe UI")
        };
        Canvas.SetLeft(siuLabel, cx - 14);
        Canvas.SetTop(siuLabel, 0);
        canvas.Children.Add(siuLabel);

        // Pixel-art Hair block
        Rectangle hair = new Rectangle { Width = 14, Height = 6, Fill = hairBrush };
        Canvas.SetLeft(hair, cx - 7);
        Canvas.SetTop(hair, 10);
        canvas.Children.Add(hair);

        // Pixel-art Head block
        Rectangle head = new Rectangle { Width = 14, Height = 14, Fill = skin };
        Canvas.SetLeft(head, cx - 7);
        Canvas.SetTop(head, 14);
        canvas.Children.Add(head);

        // Pixel-art Portugal #7 Jersey
        Rectangle torso = new Rectangle { Width = 18, Height = 20, Fill = jersey };
        Canvas.SetLeft(torso, cx - 9);
        Canvas.SetTop(torso, 28);
        canvas.Children.Add(torso);

        // #7 gold number on jersey
        TextBlock number = new TextBlock
        {
            Text = "7", FontSize = 8, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0))
        };
        Canvas.SetLeft(number, cx - 3);
        Canvas.SetTop(number, 32);
        canvas.Children.Add(number);

        // Outstretched SIUUU pixel arms (left & right blocks)
        Rectangle leftArm = new Rectangle { Width = 10, Height = 4, Fill = skin };
        Canvas.SetLeft(leftArm, cx - 18);
        Canvas.SetTop(leftArm, 32);
        canvas.Children.Add(leftArm);

        Rectangle rightArm = new Rectangle { Width = 10, Height = 4, Fill = skin };
        Canvas.SetLeft(rightArm, cx + 8);
        Canvas.SetTop(rightArm, 32);
        canvas.Children.Add(rightArm);

        // Pixel Shorts
        Rectangle shortsRect = new Rectangle { Width = 16, Height = 8, Fill = shorts };
        Canvas.SetLeft(shortsRect, cx - 8);
        Canvas.SetTop(shortsRect, 48);
        canvas.Children.Add(shortsRect);

        // Pixel Legs (apart SIUUU stance)
        Rectangle leftLeg = new Rectangle { Width = 6, Height = 18, Fill = skin };
        Canvas.SetLeft(leftLeg, cx - 12);
        Canvas.SetTop(leftLeg, 54);
        canvas.Children.Add(leftLeg);

        Rectangle rightLeg = new Rectangle { Width = 6, Height = 18, Fill = skin };
        Canvas.SetLeft(rightLeg, cx + 6);
        Canvas.SetTop(rightLeg, 54);
        canvas.Children.Add(rightLeg);

        // Pixel Boots
        Rectangle leftBoot = new Rectangle { Width = 8, Height = 5, Fill = bootsBrush };
        Canvas.SetLeft(leftBoot, cx - 14);
        Canvas.SetTop(leftBoot, Height - 5);
        canvas.Children.Add(leftBoot);

        Rectangle rightBoot = new Rectangle { Width = 8, Height = 5, Fill = bootsBrush };
        Canvas.SetLeft(rightBoot, cx + 6);
        Canvas.SetTop(rightBoot, Height - 5);
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
        RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

        SolidColorBrush skin = new SolidColorBrush(Color.FromRgb(230, 190, 150));
        SolidColorBrush hairBrush = new SolidColorBrush(Color.FromRgb(35, 28, 20));
        SolidColorBrush beardBrush = new SolidColorBrush(Color.FromRgb(60, 50, 40));
        SolidColorBrush jerseyBlue = new SolidColorBrush(Color.FromRgb(108, 172, 228));
        SolidColorBrush jerseyWhite = new SolidColorBrush(Color.FromRgb(245, 245, 250));
        SolidColorBrush shortsDark = new SolidColorBrush(Color.FromRgb(20, 20, 45));
        SolidColorBrush bootsBrush = new SolidColorBrush(Color.FromRgb(25, 25, 25));

        double cx = Width / 2.0;

        // "G.O.A.T" pixel banner label
        TextBlock goatLabel = new TextBlock
        {
            Text = "G.O.A.T",
            FontSize = 8, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            FontFamily = new FontFamily("Segoe UI")
        };
        Canvas.SetLeft(goatLabel, cx - 15);
        Canvas.SetTop(goatLabel, 0);
        canvas.Children.Add(goatLabel);

        // Pixel Hair block
        Rectangle hair = new Rectangle { Width = 16, Height = 8, Fill = hairBrush };
        Canvas.SetLeft(hair, cx - 8);
        Canvas.SetTop(hair, 10);
        canvas.Children.Add(hair);

        // Pixel Head block
        Rectangle head = new Rectangle { Width = 14, Height = 14, Fill = skin };
        Canvas.SetLeft(head, cx - 7);
        Canvas.SetTop(head, 14);
        canvas.Children.Add(head);

        // Pixel Beard block
        Rectangle beard = new Rectangle { Width = 10, Height = 5, Fill = beardBrush };
        Canvas.SetLeft(beard, cx - 5);
        Canvas.SetTop(beard, 23);
        canvas.Children.Add(beard);

        // Argentina Jersey (blue base + white stripe block)
        Rectangle torsoBlue = new Rectangle { Width = 18, Height = 20, Fill = jerseyBlue };
        Canvas.SetLeft(torsoBlue, cx - 9);
        Canvas.SetTop(torsoBlue, 28);
        canvas.Children.Add(torsoBlue);

        Rectangle whiteStripe = new Rectangle { Width = 6, Height = 20, Fill = jerseyWhite };
        Canvas.SetLeft(whiteStripe, cx - 3);
        Canvas.SetTop(whiteStripe, 28);
        canvas.Children.Add(whiteStripe);

        // #10 number
        TextBlock number = new TextBlock
        {
            Text = "10", FontSize = 8, FontWeight = FontWeights.ExtraBold,
            Foreground = new SolidColorBrush(Color.FromRgb(20, 20, 80))
        };
        Canvas.SetLeft(number, cx - 5);
        Canvas.SetTop(number, 32);
        canvas.Children.Add(number);

        // Pointing Up Arms (pixel blocks)
        Rectangle leftArm = new Rectangle { Width = 4, Height = 14, Fill = skin };
        Canvas.SetLeft(leftArm, cx - 12);
        Canvas.SetTop(leftArm, 16);
        canvas.Children.Add(leftArm);

        Rectangle rightArm = new Rectangle { Width = 4, Height = 14, Fill = skin };
        Canvas.SetLeft(rightArm, cx + 8);
        Canvas.SetTop(rightArm, 16);
        canvas.Children.Add(rightArm);

        // Shorts
        Rectangle shortsRect = new Rectangle { Width = 16, Height = 8, Fill = shortsDark };
        Canvas.SetLeft(shortsRect, cx - 8);
        Canvas.SetTop(shortsRect, 48);
        canvas.Children.Add(shortsRect);

        // Pixel Legs
        Rectangle leftLeg = new Rectangle { Width = 6, Height = 18, Fill = skin };
        Canvas.SetLeft(leftLeg, cx - 7);
        Canvas.SetTop(leftLeg, 54);
        canvas.Children.Add(leftLeg);

        Rectangle rightLeg = new Rectangle { Width = 6, Height = 18, Fill = skin };
        Canvas.SetLeft(rightLeg, cx + 1);
        Canvas.SetTop(rightLeg, 54);
        canvas.Children.Add(rightLeg);

        // Boots
        Rectangle leftBoot = new Rectangle { Width = 8, Height = 5, Fill = bootsBrush };
        Canvas.SetLeft(leftBoot, cx - 9);
        Canvas.SetTop(leftBoot, Height - 5);
        canvas.Children.Add(leftBoot);

        Rectangle rightBoot = new Rectangle { Width = 8, Height = 5, Fill = bootsBrush };
        Canvas.SetLeft(rightBoot, cx + 1);
        Canvas.SetTop(rightBoot, Height - 5);
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
