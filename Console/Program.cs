﻿using SpriteSplitter.Tools;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// Configure
const double defaultThreshold = 11;
const int minimumSpriteSize = 20;
const string defaultFile = @".\sheets\test.png";

// Read in the image file
string filePath = args.FirstOrDefault() ?? defaultFile;
var spriteSheet = new Bitmap(filePath);
var threshold = double.TryParse(args.Skip(1).FirstOrDefault(), out double t) ? t : defaultThreshold;
var targetX = int.TryParse(args.Skip(2).FirstOrDefault(), out int x) ? x : 3;
var targetY = int.TryParse(args.Skip(3).FirstOrDefault(), out int y) ? y : 3;

// Provides an easy way to store output data for this process.
var outputFolder = new Folder("sprites", filePath, threshold, minimumSpriteSize);

// Compares the current color with the key color, false for transparent.
var AreSimilarEnough = (Color target, Color sample) => ColorDistance(target, sample) <= threshold;

// Get the alpha channel mask
var mask = GetSpriteMask(spriteSheet, (targetX, targetY), AreSimilarEnough);

// Find the sprites bounding boxes
var bounds = GetSpriteBoundingBoxes(mask).ToList();

// Save the BoundingBoxes
outputFolder.WriteJson("boudingboxes.json", bounds, 
    transformWith: s => new { s.X, s.Y, s.Width, s.Height });

// Save the mask
WriteMaskToBitmap(mask, 
    beforeSave: bmp => bounds.ForEach(box => DrawRectangle(bmp, box, Color.Red)));

Console.WriteLine($"Found {bounds.Count} sprites");
foreach (var rectangle in bounds)
{
    var file = outputFolder.WriteTo("sprite.png", file =>
    {
        using var sprite = CopyBitmapRectangleWithAlpha(spriteSheet, mask, rectangle);
        sprite.Save(file, ImageFormat.Png);
    });

    Console.WriteLine($"Saved {rectangle} to {file}");
}

outputFolder.CompressFolder();

static bool[,] GetSpriteMask(Bitmap image, (int X, int Y) targetColorPosition, Func<Color, Color, bool> similar)
{
    bool[,] map = new bool[image.Height, image.Width];

    // Get the color at position (X, Y) in the image
    Color targetColor = image.GetPixel(targetColorPosition.X, targetColorPosition.Y);
    Console.WriteLine($"Found target color {targetColor.R},{targetColor.G},{targetColor.B}");

    // Create a boolean array based on whether each pixel matches the target color
    for (int y = 0; y < image.Height; y++)
    {
        for (int x = 0; x < image.Width; x++)
        {
            Color pixelColor = image.GetPixel(x, y);
            map[y, x] = !similar(targetColor, pixelColor);
        }
    }

    return map;
}

static double ColorDistance(Color color1, Color color2)
{
    int dr = color1.R - color2.R;
    int dg = color1.G - color2.G;
    int db = color1.B - color2.B;
    return Math.Sqrt(dr * dr + dg * dg + db * db);
}

static List<Rectangle> GetSpriteBoundingBoxes(bool[,] map)
{
    // Create a new array with the same dimensions as the original array
    bool[,] search = new bool[map.GetLength(0), map.GetLength(1)];

    // Copy the elements of the original array to the new array
    Array.Copy(map, search, map.Length);

    // Collect the bounding boxes
    List<Rectangle> bounds = new();

    // Iterate through each cell in the search
    for (int y = 0; y < search.GetLength(0); y++)
    {
        for (int x = 0; x < search.GetLength(1); x++)
        {
            // If the cell is a "true" island, find its bounds and add them to the list
            if (search[y, x])
            {
                Rectangle bound = FindSpriteBounds(search, y, x);
                if (bound.Width > minimumSpriteSize && bound.Height > minimumSpriteSize)
                {
                    bounds.Add(bound);
                }
            }
        }
    }

    return bounds;
}

static Rectangle FindSpriteBounds(bool[,] map, int startY, int startX)
{
    // DFS depth-first-search for the edge of the sprite.

    int minX = startX;
    int maxX = startX;
    int minY = startY;
    int maxY = startY;

    // Mark the starting cell as visited
    map[startY, startX] = false;

    // Add the starting cell to a queue of cells to visit
    Queue<int> xQueue = new Queue<int>();
    Queue<int> yQueue = new Queue<int>();
    xQueue.Enqueue(startX);
    yQueue.Enqueue(startY);

    // Keep expanding the bounds while there are cells to visit
    while (xQueue.Count > 0)
    {
        int x = xQueue.Dequeue();
        int y = yQueue.Dequeue();

        // Update the min/max x and y values based on the current cell
        minX = Math.Min(minX, x);
        maxX = Math.Max(maxX, x);
        minY = Math.Min(minY, y);
        maxY = Math.Max(maxY, y);

        // Add all adjacent "true" cells to the queue
        if (y > 0 && map[y - 1, x])
        {
            map[y - 1, x] = false;
            xQueue.Enqueue(x);
            yQueue.Enqueue(y - 1);
        }
        if (y < map.GetLength(0) - 1 && map[y + 1, x])
        {
            map[y + 1, x] = false;
            xQueue.Enqueue(x);
            yQueue.Enqueue(y + 1);
        }
        if (x > 0 && map[y, x - 1])
        {
            map[y, x - 1] = false;
            xQueue.Enqueue(x - 1);
            yQueue.Enqueue(y);
        }
        if (x < map.GetLength(1) - 1 && map[y, x + 1])
        {
            map[y, x + 1] = false;
            xQueue.Enqueue(x + 1);
            yQueue.Enqueue(y);
        }
    }

    // Return a rectangle representing the bounds of the island
    return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
}

void WriteMaskToBitmap(bool[,] array, Action<Bitmap>? beforeSave = default)
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

    // Execute an transforms
    beforeSave?.Invoke(image);

    // Save the bitmap to the file system
    outputFolder.WriteTo("000-MaskOverlay.bmp", file =>
    {
        image.Save(file, ImageFormat.Bmp);
    });
}

static void DrawRectangle(Bitmap image, Rectangle bound, Color color)
{
    // Set the pixel colors for the top and bottom edges of the square
    for (int i = bound.X; i < bound.X + bound.Width; i++)
    {
        image.SetPixel(i, bound.Y, color);
        image.SetPixel(i, bound.Y + bound.Height - 1, color);
    }

    // Set the pixel colors for the left and right edges of the square
    for (int i = bound.Y; i < bound.Y + bound.Height; i++)
    {
        image.SetPixel(bound.X, i, color);
        image.SetPixel(bound.X + bound.Width - 1, i, color);
    }
}

static Bitmap CopyBitmapRectangleWithAlpha(Bitmap source, bool[,] mask, Rectangle rectangle)
{
    // We expect midjourney to give us Format24bppRgb PNG data.
    if (source.PixelFormat != PixelFormat.Format24bppRgb)
    {
        throw new InvalidOperationException($"Unexpected pixel format '{source.PixelFormat}' is not '{PixelFormat.Format24bppRgb}'.");
    }

    // Create a new bitmap with the same pixel format as the source
    Bitmap destination = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);

    // Lock the source and destination bitmaps for data manipulation
    BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
    BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, destination.PixelFormat);

    // Create byte arrays for the source and destination bitmaps
    int sourceStride = sourceData.Stride;
    int destinationStride = destinationData.Stride;
    int sourcePixelSize = Image.GetPixelFormatSize(source.PixelFormat) / 8;
    int destinationPixelSize = Image.GetPixelFormatSize(destination.PixelFormat) / 8;

    byte[] sourceArray = new byte[sourceStride * source.Height];
    byte[] destinationArray = new byte[destinationStride * destination.Height];

    // Copy the data from the source bitmap to the source array
    Marshal.Copy(sourceData.Scan0, sourceArray, 0, sourceArray.Length);

    // Copy the data from the source array to the destination array, applying the mask
    int sourceOffset = sourceStride * rectangle.Y + rectangle.X * sourcePixelSize;
    int destinationOffset = 0;
    for (int y = 0; y < rectangle.Height; y++)
    {
        // Copy a single line of pixels from the source array to the destination array
        for (int x = 0; x < rectangle.Width; x++)
        {
            // The masked or unmasked pixel conversion happens below.
            if (mask[rectangle.Y + y, rectangle.X + x])
            {
                // Copy the sprite pixel with no transparency
                destinationArray[destinationOffset + x * destinationPixelSize + 3] = 255;
                destinationArray[destinationOffset + x * destinationPixelSize + 2] = sourceArray[(sourceOffset + x * sourcePixelSize) + 2];
                destinationArray[destinationOffset + x * destinationPixelSize + 1] = sourceArray[(sourceOffset + x * sourcePixelSize) + 1];
                destinationArray[destinationOffset + x * destinationPixelSize + 0] = sourceArray[(sourceOffset + x * sourcePixelSize) + 0];
            }
            else
            {
                // Copy an transparent pixel
                destinationArray[destinationOffset + x * destinationPixelSize + 3] = 0; // Alpha
                destinationArray[destinationOffset + x * destinationPixelSize + 2] = 0; // RED
                destinationArray[destinationOffset + x * destinationPixelSize + 1] = 0; // GREEN
                destinationArray[destinationOffset + x * destinationPixelSize + 0] = 0; // BLUE
            }
        }

        // Advance to the next line in both arrays
        sourceOffset += sourceStride;
        destinationOffset += destinationStride;
    }

    // Copy the data from the destination array to the destination bitmap
    Marshal.Copy(destinationArray, 0, destinationData.Scan0, destinationArray.Length);

    // Unlock the source and destination bitmaps
    source.UnlockBits(sourceData);
    destination.UnlockBits(destinationData);

    return destination;
}