using Microsoft.Maui.Graphics;

namespace ResponderApp
{
    public class GradientTextDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            string text = "BAYANIHAN";
            float fontSize = 32;

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontSize = fontSize;

            var gradient = new LinearGradientPaint
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new[]
                {
                    new PaintGradientStop(0f, Color.FromArgb("#34e89e")),
                    new PaintGradientStop(1f, Color.FromArgb("#0f3443"))
                }
            };

            canvas.SaveState();
            canvas.SetFillPaint(gradient, dirtyRect);

            canvas.DrawString(
                text,
                dirtyRect.X,
                dirtyRect.Y,
                dirtyRect.Width,
                dirtyRect.Height,
                HorizontalAlignment.Center,
                VerticalAlignment.Center
            );

            canvas.RestoreState();
        }
    }
}
