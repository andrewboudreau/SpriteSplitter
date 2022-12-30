using SpriteSplitter;
using System.Drawing;
using System.Drawing.Imaging;

const double defaultThreshold = 11;
const int defaultMinSpriteSize = 20;
const string defaultFile = "test.png";

string session = DateTime.Now.Ticks.ToString()[5..^1];
int imageId = 1;

// Read in the image file
string filePath = args.FirstOrDefault() ?? defaultFile;
var image = new Bitmap(filePath);
var threshold = double.TryParse(args.Skip(1).FirstOrDefault(), out double t) ? t : defaultThreshold;
var targetX = int.TryParse(args.Skip(2).FirstOrDefault(), out int x) ? x : 6;
var targetY = int.TryParse(args.Skip(3).FirstOrDefault(), out int y) ? y : 6;

var mask = GetSpriteMask(image, (targetX, targetY), (key, actual) =>
{
    var distance = ColorDistance(key, actual);
    return distance <= threshold;
});

var bounds = GetSpriteBoundingBoxes(mask)
   //.CombineOverlappingRectangles()
   .ToList();

mask = GetSpriteMask(image, (targetX, targetY), (key, actual) =>
{
    var distance = ColorDistance(key, actual);
    return distance <= threshold;
});

WriteToBitmap(mask, bmp =>
{
    foreach (var b in bounds)
    {
        DrawSquare(bmp, b, Color.Red);
    }
});

Console.WriteLine($"Found {bounds.Count} sprites");

foreach (var bound in bounds)
{
    Console.WriteLine($"Found {bound}");
}

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

static List<Rectangle> GetSpriteBoundingBoxes(bool[,] map)
{
    var mutable = new bool[,](map);
    List<Rectangle> bounds = new();

    // Iterate through each cell in the map
    for (int y = 0; y < map.GetLength(0); y++)
    {
        for (int x = 0; x < map.GetLength(1); x++)
        {
            // If the cell is a "true" island, find its bounds and add them to the list
            if (map[y, x])
            {
                Rectangle bound = FindSpriteBounds(map, y, x);
                if (bound.Width > defaultMinSpriteSize && bound.Height > defaultMinSpriteSize)
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

static bool[,] ExampleMatrix()
{
    #region disable_format
    bool[,] map = {
        { true, true , false, false, false },
        { true, true , false, false, true  },
        { true, false, false, true , true  },
        { true, false, false, true , false },
        { true, false, true , true , false }
    };
    #endregion

    #region disable_format
    bool[,] map2 = {
        { true , true , false, false, false },
        { false, true , false, false, true  },
        { true , false, false, true , true  },
        { false, false, false, false, false },
        { true , false, true , false, false }
    };
    #endregion
    return map;
}

void WriteToBitmap(bool[,] array, Action<Bitmap>? transform = default)
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
            Color color = array[y, x] ? Color.Black : Color.White;
            image.SetPixel(x, y, color);
        }
    }

    // Execute an transforms
    transform?.Invoke(image);

    string outputFileName = $"{session}-{imageId++}.bmp";

    // Save the bitmap to the file system
    image.Save(outputFileName, ImageFormat.Bmp);
}

static double ColorDistance(Color color1, Color color2)
{
    int dr = color1.R - color2.R;
    int dg = color1.G - color2.G;
    int db = color1.B - color2.B;
    return Math.Sqrt(dr * dr + dg * dg + db * db);
}

static void DrawSquare(Bitmap image, Rectangle bound, Color color)
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
