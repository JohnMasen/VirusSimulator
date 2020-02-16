using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using VirusSimulator.Core;
using System.Linq;
using SixLabors.Primitives;
using System.IO;

namespace VirusSimulator.Image
{
    public class ImagePositionLoader
    {
        Image<Gray8> img;
        List<(int score, Vector2 position)> points;
        public ImagePositionLoader(string path, int mapSize)
        {
            img = SixLabors.ImageSharp.Image.Load<Gray8>(path);
            img.Mutate(op =>
            {
                op.Resize(mapSize, mapSize);
            });
            points = new List<(int score, Vector2 position)>(img.Width * img.Height);
            for (int y = 0; y < img.Height; y++)
            {
                var row = img.GetPixelRowSpan(y);
                for (int x = 0; x < img.Width; x++)
                {
                    Gray8 point = row[x];
                    points.Add((point.PackedValue, new Vector2(x, y)));
                }
            }

        }

        public IEnumerable<Vector2> GetRandomPoints(int count)
        {
            return (from item in points
                    orderby (item.score * 255 + Helper.RandomInt(255)) descending
                    select item.position).Take(count);
        }

        public void TestOutput(string path, int size, int points)
        {
            Image<Bgra32> output = new Image<Bgra32>(size, size);
            output.Mutate(op =>
            {
                op.Fill(Color.Black);
                foreach (var item in GetRandomPoints(points))
                {
                    op.Draw(Color.White, 5, new RectangleF(item, new SizeF(5, 5)));
                }
            });
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                output.SaveAsBmp(fs);
            }
        }

    }
}
