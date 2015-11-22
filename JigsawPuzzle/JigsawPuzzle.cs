using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace JigsawPuzzleEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Jigsaw Puzzle")]
    public class JigsawPuzzleEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Jigsaw Puzzle";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(JigsawPuzzleEffectPlugin), "JigsawPuzzle.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public JigsawPuzzleEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Amount1, 1, 0.2, 10));
            props.Add(new Int32Property(PropertyNames.Amount2, 2, 1, 10));
            props.Add(new BooleanProperty(PropertyNames.Amount3, false));
            props.Add(new Int32Property(PropertyNames.Amount4, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount5, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>();

            propRules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.Amount4, PropertyNames.Amount3, false));

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Scale");
            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.UpDownIncrement, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Line Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Line Color");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.Description, "Transparent");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlType(PropertyNames.Amount4, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, "Offset");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            Rectangle selection5 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource5 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection5));
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.StaticImageUnderlay, imageResource5);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<DoubleProperty>(PropertyNames.Amount1).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value;
            Amount3 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount3).Value;
            Amount4 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount4).Value);
            Amount5 = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Amount5).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);


            Bitmap puzzleBitmap = new Bitmap((int)(100 * Amount1), (int)(100 * Amount1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics puzzleGraphics = Graphics.FromImage(puzzleBitmap);
            puzzleGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen puzzlePen = new Pen(Amount4, Amount2);

            // Create points that define the horizontal curve.
            Point horPoint0 = new Point((int)(-50 * Amount1), (int)(16 * Amount1));
            Point horPoint1 = new Point((int)(0 * Amount1), (int)(16 * Amount1));
            Point horPoint2 = new Point((int)(13 * Amount1), (int)(28 * Amount1));
            Point horPoint3 = new Point((int)(9 * Amount1), (int)(50 * Amount1));

            Point horPoint4 = new Point((int)(49 * Amount1), (int)(49 * Amount1));

            Point horPoint5 = new Point((int)(90 * Amount1), (int)(49 * Amount1));
            Point horPoint6 = new Point((int)(86 * Amount1), (int)(71 * Amount1));
            Point horPoint7 = new Point((int)(99 * Amount1), (int)(83 * Amount1));
            Point horPoint8 = new Point((int)(149 * Amount1), (int)(83 * Amount1));

            Point[] horCurvePoints = { horPoint0, horPoint1, horPoint2, horPoint3, horPoint4, horPoint5, horPoint6, horPoint7, horPoint8 };

            // Draw the horizontal curve to graphics object.
            puzzleGraphics.DrawCurve(puzzlePen, horCurvePoints);


            // Create points that define the vertical curve.
            Point verPoint0 = new Point((int)(83 * Amount1), (int)(-50 * Amount1));
            Point verPoint1 = new Point((int)(83 * Amount1), (int)(0 * Amount1));
            Point verPoint2 = new Point((int)(73 * Amount1), (int)(13 * Amount1));
            Point verPoint3 = new Point((int)(49 * Amount1), (int)(9 * Amount1));

            Point verPoint4 = new Point((int)(50 * Amount1), (int)(50 * Amount1));

            Point verPoint5 = new Point((int)(50 * Amount1), (int)(90 * Amount1));
            Point verPoint6 = new Point((int)(28 * Amount1), (int)(86 * Amount1));
            Point verPoint7 = new Point((int)(16 * Amount1), (int)(99 * Amount1));
            Point verPoint8 = new Point((int)(16 * Amount1), (int)(149 * Amount1));

            Point[] verCurvePoints = { verPoint0, verPoint1, verPoint2, verPoint3, verPoint4, verPoint5, verPoint6, verPoint7, verPoint8 };

            // Draw the vertical curve to graphics object.
            puzzleGraphics.DrawCurve(puzzlePen, verCurvePoints);

            puzzlePen.Dispose();

            puzzleSurface = Surface.CopyFromBitmap(puzzleBitmap);
            puzzleBitmap.Dispose();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        #region User Entered Code
        // Name: Jigsaw Puzzle
        // Submenu: Render
        // Author: toe_head2001
        // Title: 
        // Desc: Generates a jigsaw puzzle outline
        // Keywords: 
        // URL: http://www.getpaint.net/redirect/plugins.html
        // Help:
        #region UICode
        double Amount1 = 1; // [0.2,10] Scale
        int Amount2 = 2; // [1,10] Line Width
        bool Amount3 = false; // [0,1] Transparent
        ColorBgra Amount4 = ColorBgra.FromBgr(0, 0, 0); // Line Color
        Pair<double, double> Amount5 = Pair.Create(0.0, 0.0); // Offset
        #endregion

        private BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        private Surface puzzleSurface;

        private static ColorBgra GetBilinearSampleMirrored(Surface tile, float x, float y)
        {
            int width = tile.Width;
            int num = (int)Math.Floor(x / width);
            x -= num * width;
            int height = tile.Height;
            int num2 = (int)Math.Floor(y / height);
            y -= num2 * height;

            if (num2 % 2 != 0)
            {
                y = height - (y + 1);
            }
            if (num % 2 != 0)
            {
                x = width - (x + 1);
            }

            return tile.GetBilinearSampleClamped(x, y);
        }

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

            ColorBgra sourcePixel, puzzlePixel, finalPixel;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    int offsetX = (int)(x + selection.Width * Amount5.First / 2);
                    int offsetY = (int)(y + selection.Height * Amount5.Second / 2);

                    puzzlePixel = GetBilinearSampleMirrored(puzzleSurface, offsetX - selection.Left, offsetY - selection.Top);

                    sourcePixel = src[x, y];

                    if (Amount3)
                    {
                        sourcePixel.A = Int32Util.ClampToByte(255 - puzzlePixel.A);
                        finalPixel = sourcePixel;
                    }
                    else
                    {
                        finalPixel = normalOp.Apply(sourcePixel, puzzlePixel);
                    }

                    dst[x, y] = finalPixel;
                }
            }
        }
        #endregion
    }
}