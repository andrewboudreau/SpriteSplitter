using SpriteSplitter.Tools;

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
var bounds = GetSpriteBoundingBoxes(mask).CombineOverlappingRectangles().ToList();

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

