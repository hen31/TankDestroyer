using System.Numerics;
using TankDestroyer.API;

namespace TankDestroyer.Engine;

public class World : IWorld
{
    public string Name { get; set; }
    public Tile[] Tiles { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Vector2[] SpawnPoints { get; set; } = Array.Empty<Vector2>();
    public List<Message> Messages { get; set; } = [];

    public ITile GetTile(int x, int y)
    {
        return Tiles[(y * Width) + x];
    }

    public static World GenerateRandom(int height, int width)
    {
        var tiles = new Tile[height * width];
        Random random = new Random();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile tile = new();
                tiles[(y * width) + x] = tile;
                tile.X = x;
                tile.Y = y;
                if (y < 100 && x == 0)
                {
                    tile.TileType = TileType.Sand;
                }
                else
                {
                    tile.TileType = (TileType)random.Next(0, 5);
                }
            }
        }

        return new World()
        {
            Tiles = tiles,
            Width = width,
            Height = height,
            SpawnPoints = [new Vector2(10, 10), new Vector2(0, height - 7)]
        };
    }

    public static World LoadFromFile(string filePath)
    {
        string text = File.ReadAllText(filePath);
        World world = new World();
        string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var infoLineParts = lines[0].Split(',');
        world.Name = infoLineParts[0];

        world.Height = int.Parse(infoLineParts[1]);
        world.Width = int.Parse(infoLineParts[2]);

        List<Vector2> spawnPoints = new List<Vector2>();
        for (int i = 3; i < infoLineParts.Length; i++)
        {
            var parts = infoLineParts[i].Split(';');
            spawnPoints.Add(new Vector2(int.Parse(parts[0]), int.Parse(parts[1])));
        }

        List<Tile> tiles = new List<Tile>();
        for (int y = 1; y < lines.Length; y++)
        {
            var line = lines[y].Trim();
            var numbers = line.Split(',');
            for (int x = 0; x < numbers.Length; x++)
            {
                var number = numbers[x];
                tiles.Add(new Tile()
                {
                    X = x,
                    Y = y - 1,
                    TileType = (TileType)int.Parse(number)
                });
            }
        }

        world.Tiles = tiles.ToArray();
        world.SpawnPoints = spawnPoints.ToArray();
        return world;
    }
}
