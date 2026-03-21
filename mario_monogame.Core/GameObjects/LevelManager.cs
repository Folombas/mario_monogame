using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Тип биома уровня.
    /// </summary>
    public enum BiomeType
    {
        Grassland,
        Castle,
        Candy,
        Ice,
        Mushroom,
        Mountain,
        Forest,
        Industrial,
        Country,
        Desert,
        Snow,
        Cave
    }

    /// <summary>
    /// Данные уровня.
    /// </summary>
    public class LevelData
    {
        public int LevelNumber { get; set; }
        public string Name { get; set; }
        public BiomeType Biome { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 PlayerStart { get; set; }
        public Vector2 FlagPosition { get; set; }
        public string BackgroundPath { get; set; }
        public string[] ParallaxLayers { get; set; }
        public string MusicPath { get; set; }
        public string[] Layout { get; set; }
        public List<EnemySpawn> EnemySpawns { get; set; }
        public List<ItemSpawn> ItemSpawns { get; set; }
        public string IntroText { get; set; }
        public string OutroText { get; set; }
        public Color SkyColor { get; set; }

        public LevelData()
        {
            EnemySpawns = new List<EnemySpawn>();
            ItemSpawns = new List<ItemSpawn>();
            ParallaxLayers = new string[0];
            SkyColor = new Color(100, 149, 237);
        }
    }

    public struct EnemySpawn
    {
        public string Type;
        public Vector2 Position;
        public string Variant;
    }

    public struct ItemSpawn
    {
        public string Type;
        public Vector2 Position;
        public string Variant;
    }

    /// <summary>
    /// Менеджер уровней с поддержкой множества биомов.
    /// </summary>
    public class LevelManager
    {
        private Dictionary<int, LevelData> _levels;
        private int _currentLevelIndex;

        public LevelManager()
        {
            _levels = new Dictionary<int, LevelData>();
            _currentLevelIndex = 1;
            CreateAllLevels();
        }

        private void CreateAllLevels()
        {
            // Уровень 1: Зелёные луга (Base)
            _levels[1] = new LevelData
            {
                LevelNumber = 1,
                Name = "Green Hills",
                Biome = BiomeType.Grassland,
                Width = 100,
                Height = 20,
                BackgroundPath = "Sprites/Backgrounds/blue_grass",
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(4600, 450),
                Layout = CreateGrasslandLevel(),
                SkyColor = new Color(100, 149, 237),
                IntroText = "Добро пожаловать в Зелёные Луга!",
                OutroText = "Первый уровень пройден!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Goomba", Position = new Vector2(400, 550) },
                    new EnemySpawn { Type = "Goomba", Position = new Vector2(700, 550) },
                    new EnemySpawn { Type = "Slime", Position = new Vector2(550, 550), Variant = "Green" },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(200, 450) },
                    new ItemSpawn { Type = "Coin", Position = new Vector2(250, 450) },
                    new ItemSpawn { Type = "Mushroom", Position = new Vector2(600, 300) },
                }
            };

            // Уровень 2: Замок и конфеты (Complete Pack)
            _levels[2] = new LevelData
            {
                LevelNumber = 2,
                Name = "Candy Castle",
                Biome = BiomeType.Candy,
                Width = 120,
                Height = 20,
                BackgroundPath = "Level2/bg_castle",
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(5600, 450),
                Layout = CreateCastleLevel(),
                SkyColor = new Color(180, 100, 200),
                IntroText = "Сладкий замок полон опасностей...",
                OutroText = "Сладкая победа!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Goomba", Position = new Vector2(500, 550) },
                    new EnemySpawn { Type = "Slime", Position = new Vector2(800, 550), Variant = "Purple" },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(400, 400) },
                    new ItemSpawn { Type = "Gem", Position = new Vector2(700, 350), Variant = "Red" },
                }
            };

            // Уровень 3: Горы (Parallax Mountains)
            _levels[3] = new LevelData
            {
                LevelNumber = 3,
                Name = "Mountain Peaks",
                Biome = BiomeType.Mountain,
                Width = 130,
                Height = 20,
                BackgroundPath = "",
                ParallaxLayers = new[]
                {
                    "Level3/Backgrounds/parallax-mountain-bg",
                    "Level3/Backgrounds/parallax-mountain-montain-far",
                    "Level3/Backgrounds/parallax-mountain-mountains",
                    "Level3/Backgrounds/parallax-mountain-trees",
                    "Level3/Backgrounds/parallax-mountain-foreground-trees"
                },
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(6000, 450),
                Layout = CreateMountainLevel(),
                SkyColor = new Color(80, 120, 180),
                IntroText = "Высокие горы ждут тебя!",
                OutroText = "Вершина покорена!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Slime", Position = new Vector2(600, 550), Variant = "Blue" },
                    new EnemySpawn { Type = "Mouse", Position = new Vector2(900, 550) },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(500, 400) },
                    new ItemSpawn { Type = "Star", Position = new Vector2(1000, 300) },
                }
            };

            // Уровень 4: Лес (Parallax Forest)
            _levels[4] = new LevelData
            {
                LevelNumber = 4,
                Name = "Dark Forest",
                Biome = BiomeType.Forest,
                Width = 140,
                Height = 20,
                BackgroundPath = "",
                ParallaxLayers = new[]
                {
                    "Level4/Backgrounds/parallax-forest-back-trees",
                    "Level4/Backgrounds/parallax-forest-middle-trees",
                    "Level4/Backgrounds/parallax-forest-lights",
                    "Level4/Backgrounds/parallax-forest-front-trees"
                },
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(6500, 450),
                Layout = CreateForestLevel(),
                SkyColor = new Color(60, 100, 60),
                IntroText = "Тёмный лес таит секреты...",
                OutroText = "Лес пройден!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Fly", Position = new Vector2(500, 300) },
                    new EnemySpawn { Type = "Slime", Position = new Vector2(800, 550), Variant = "Green" },
                    new EnemySpawn { Type = "Worm", Position = new Vector2(1100, 550), Variant = "Pink" },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(600, 400) },
                    new ItemSpawn { Type = "Gem", Position = new Vector2(900, 350), Variant = "Green" },
                }
            };

            // Уровень 5: Индустриальная зона (Parallax Industrial)
            _levels[5] = new LevelData
            {
                LevelNumber = 5,
                Name = "Industrial Zone",
                Biome = BiomeType.Industrial,
                Width = 150,
                Height = 20,
                BackgroundPath = "",
                ParallaxLayers = new[]
                {
                    "Level5/Backgrounds/parallax-industrial-bg",
                    "Level5/Backgrounds/parallax-industrial-buildings",
                    "Level5/Backgrounds/parallax-industrial-foreground"
                },
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(7000, 450),
                Layout = CreateIndustrialLevel(),
                SkyColor = new Color(80, 80, 100),
                IntroText = "Индустриальная зона опасна!",
                OutroText = "Зона зачищена!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Saw", Position = new Vector2(700, 400) },
                    new EnemySpawn { Type = "Slime", Position = new Vector2(1000, 550), Variant = "Purple" },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(800, 400) },
                    new ItemSpawn { Type = "Mushroom", Position = new Vector2(1200, 350) },
                }
            };

            // Уровень 6: Деревня (Country)
            _levels[6] = new LevelData
            {
                LevelNumber = 6,
                Name = "Country Village",
                Biome = BiomeType.Country,
                Width = 140,
                Height = 20,
                BackgroundPath = "Level6/country_bg",
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(6500, 450),
                Layout = CreateCountryLevel(),
                SkyColor = new Color(150, 180, 220),
                IntroText = "Спокойная деревня...",
                OutroText = "Деревня спасена!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Goomba", Position = new Vector2(600, 550) },
                    new EnemySpawn { Type = "Bee", Position = new Vector2(900, 350) },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(700, 400) },
                    new ItemSpawn { Type = "Star", Position = new Vector2(1100, 300) },
                }
            };

            // Уровень 7: Финальный босс
            _levels[7] = new LevelData
            {
                LevelNumber = 7,
                Name = "Final Boss",
                Biome = BiomeType.Castle,
                Width = 100,
                Height = 20,
                BackgroundPath = "Level2/bg_castle",
                PlayerStart = new Vector2(100, 400),
                FlagPosition = new Vector2(4500, 450),
                Layout = CreateBossLevel(),
                SkyColor = new Color(100, 50, 50),
                IntroText = "Босс ждёт тебя!",
                OutroText = "ПОБЕДА! Ты легенда!",
                EnemySpawns = new List<EnemySpawn>
                {
                    new EnemySpawn { Type = "Goomba", Position = new Vector2(500, 550) },
                    new EnemySpawn { Type = "Slime", Position = new Vector2(800, 550), Variant = "Purple" },
                    new EnemySpawn { Type = "Boss", Position = new Vector2(4000, 450) },
                },
                ItemSpawns = new List<ItemSpawn>
                {
                    new ItemSpawn { Type = "Coin", Position = new Vector2(600, 400) },
                    new ItemSpawn { Type = "Mushroom", Position = new Vector2(900, 350) },
                    new ItemSpawn { Type = "Star", Position = new Vector2(1500, 300) },
                }
            };
        }

        public LevelData GetLevel(int levelNumber)
        {
            return _levels.ContainsKey(levelNumber) ? _levels[levelNumber] : null;
        }

        public LevelData CurrentLevel => GetLevel(_currentLevelIndex);

        public void GoToLevel(int levelNumber)
        {
            if (_levels.ContainsKey(levelNumber))
            {
                _currentLevelIndex = levelNumber;
            }
        }

        public void NextLevel()
        {
            _currentLevelIndex++;
        }

        public bool HasNextLevel() => _levels.ContainsKey(_currentLevelIndex + 1);
        public int TotalLevels => _levels.Count;

        private string[] CreateGrasslandLevel() => CreateBaseLayout(100, 'G', 'D', 'S');
        private string[] CreateCastleLevel() => CreateBaseLayout(120, 'B', 'S', 'S');
        private string[] CreateMountainLevel() => CreateBaseLayout(130, 'S', 'S', 'S');
        private string[] CreateForestLevel() => CreateBaseLayout(140, 'G', 'D', 'S');
        private string[] CreateIndustrialLevel() => CreateBaseLayout(150, 'B', 'S', 'S');
        private string[] CreateCountryLevel() => CreateBaseLayout(140, 'G', 'D', 'S');
        private string[] CreateBossLevel() => CreateBaseLayout(100, 'B', 'S', 'S');

        private string[] CreateBaseLayout(int width, char top, char mid, char bot)
        {
            var layout = new string[20];
            for (int y = 0; y < 20; y++)
            {
                if (y < 15)
                    layout[y] = new string(' ', width);
                else if (y == 15)
                    layout[y] = new string(top, width);
                else if (y < 19)
                    layout[y] = new string(mid, width);
                else
                    layout[y] = new string(bot, width);
            }

            // Добавляем платформы
            AddPlatform(layout, 15, 11, 4, 'B');
            AddPlatform(layout, 30, 8, 3, '?');
            AddPlatform(layout, 50, 11, 5, 'B');
            AddPlatform(layout, 70, 7, 4, 'B');

            // Трубы
            AddPipe(layout, 25, 13, 2);
            AddPipe(layout, 60, 13, 3);

            // Шипы
            AddSpikes(layout, 40, 14, 3);
            AddSpikes(layout, 80, 14, 4);

            return layout;
        }

        private void AddPlatform(string[] layout, int x, int y, int length, char tile)
        {
            for (int i = 0; i < length && y < layout.Length && x + i < width; i++)
            {
                if (x + i < layout[y].Length)
                {
                    char[] row = layout[y].ToCharArray();
                    row[x + i] = tile;
                    layout[y] = new string(row);
                }
            }
        }

        private void AddPipe(string[] layout, int x, int y, int height)
        {
            for (int i = 0; i < height && y + i < layout.Length; i++)
            {
                for (int w = 0; w < 2 && x + w < layout[y + i].Length; w++)
                {
                    char[] row = layout[y + i].ToCharArray();
                    row[x + w] = 'P';
                    layout[y + i] = new string(row);
                }
            }
        }

        private void AddSpikes(string[] layout, int x, int y, int count)
        {
            for (int i = 0; i < count && y < layout.Length && x + i < layout[y].Length; i++)
            {
                char[] row = layout[y].ToCharArray();
                row[x + i] = '^';
                layout[y] = new string(row);
            }
        }

        private int width => 150;
    }
}
