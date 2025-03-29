using System.Drawing;
using System.Drawing.Imaging;

namespace SpriteSplitter;

/// <summary>
/// Provides utility methods for working with bitmap masks.
/// </summary>
public static class BitmapMaskUtility
{
    /// <summary>
    /// Writes a boolean mask to a bitmap file, with optional pre-save transformations.
    /// </summary>
    /// <param name="array">The boolean mask array</param>
    /// <param name="outputPath">The path to save the bitmap to</param>
    /// <param name="beforeSave">Optional action to perform on the bitmap before saving</param>
    public static void WriteMaskToBitmap(bool[,] array, string outputPath, Action<Bitmap>? beforeSave = default)
    {
        // Create a new bitmap with the same dimensions as the array
        int width = array.GetLength(1);
        int height = array.GetLength(0);
        Bitmap image = new(width, height);

        // Set the pixel colors based on the values in the array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = array[y, x] ? Color.White : Color.Black;
                image.SetPixel(x, y, color);
            }
        }

        // Execute any transforms
        beforeSave?.Invoke(image);

        // Save the bitmap to the file system
        image.Save(outputPath, ImageFormat.Bmp);
    }
}
