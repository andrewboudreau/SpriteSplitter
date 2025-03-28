using System.Drawing;

namespace SpriteSplitter
{
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
            var rectList = rectangles.ToList();
            if (rectList.Count == 0)
                return rectList;

            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < rectList.Count; i++)
                {
                    for (int j = i + 1; j < rectList.Count; j++)
                    {
                        if (rectList[i].IntersectsWith(rectList[j]))
                        {
                            // Combine the two rectangles
                            rectList[i] = Rectangle.Union(rectList[i], rectList[j]);
                            rectList.RemoveAt(j);
                            changed = true;
                            break;
                        }
                    }
                    if (changed) break;
                }
            } while (changed);

            return rectList;
        }
    }
}
