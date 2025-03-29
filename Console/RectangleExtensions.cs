using System.Drawing;

namespace SpriteSplitter;

/// <summary>
/// Provides extension methods for working with Rectangle objects.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Combines overlapping rectangles into larger rectangles.
    /// This implementation handles multiple groups of overlapping rectangles.
    /// </summary>
    /// <param name="rectangles">The collection of rectangles to combine</param>
    /// <returns>A collection of combined rectangles</returns>
    public static IEnumerable<Rectangle> CombineOverlappingRectangles(this IEnumerable<Rectangle> rectangles)
    {
        // Sort the rectangles by their x and y coordinates
        rectangles = rectangles.OrderBy(r => r.Y).ThenBy(r => r.X);

        // Combine any overlapping rectangles
        List<Rectangle> combined = new List<Rectangle>();

        Rectangle current = rectangles.First();
        foreach (Rectangle rectangle in rectangles.Skip(1))
        {
            if (rectangle.IntersectsWith(current))
            {
                current = Rectangle.Union(current, rectangle);
            }
            else
            {
                combined.Add(current);
                current = rectangle;
            }
        }
        combined.Add(current);

        return combined;
    }
}
