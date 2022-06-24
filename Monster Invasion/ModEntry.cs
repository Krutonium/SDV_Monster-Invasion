using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using StardewValley.Monsters;

namespace Monster_Invasion
{
    // Plan:
    // When the player loads a new area 
    // if it's 
    // A) On the Whitelist
    // B) After 10 PM
    // Spawn Enemies around.
    // This adds danger (but also possibly reward)
    // for staying up late.
    
    public class ModEntry : Mod
    {
        public List<String> overrideAreas = new();
        public List<Monster> existingMobs = new();
        private ModConfig config = new ModConfig();
        public override void Entry(IModHelper helper)
        {
            GetOverrideAreas();
            config = Helper.ReadConfig<ModConfig>();
            
#if DEBUG
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
#endif
            helper.Events.Player.Warped += PlayerOnWarped;
            helper.Events.GameLoop.DayEnding += GameLoopOnDayEnding;
            helper.Events.GameLoop.TimeChanged += GameLoopOnTimeChanged;
            helper.Events.GameLoop.DayStarted += GameLoopOnDayStarted;
        }

        private void GameLoopOnDayStarted(object sender, DayStartedEventArgs e)
        {
            GeneratedToday = false;
        }

        private bool GeneratedToday = false;
        private void GameLoopOnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (e.NewTime <= config.StartTime) return;
            if (GeneratedToday == true) return;
            GeneratedToday = true;
            ClearArea();
            mkMobs(Game1.currentLocation);
        }

        private void GameLoopOnDayEnding(object sender, DayEndingEventArgs e)
        {
            ClearArea();
        }

        class ModConfig
        {
            public int StartTime = 2400;
            public bool ScaleWithCombatSkill = false;
            public int SpawnChancePerTile = 100;
            public string Explaination =
                "SpawnChancePerTile is not how you think - A bigger number will *decrease* the number of mobs being spawned" +
                " - At a setting of 1, it will spawn a mob on every single valid tile." +
                " This is very laggy. Recommend 100 for a balanced experience, 50 for a more intense one, and 25 for a fight for your life.";
        }
        
        private void ClearArea()
        {
            foreach (var mob in existingMobs)
            {
                mob.Health = 0;
            }
            existingMobs.Clear();
        }
        
        private void GetOverrideAreas()
        {
            overrideAreas.Add("Example override area");
        }

        private void PlayerOnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (Game1.timeOfDay <= config.StartTime) return;
            if (!e.OldLocation.farmers.Any())
            {
                //Only clear the map if there's no farmers left on the map.
                ClearArea();
            }
            mkMobs(e.NewLocation);
        }


        private void mkMobs(GameLocation map)
        { 
            if (map.IsOutdoors || overrideAreas.Contains(map.Name))
            {
                var Tiles = map.map.GetLayer("Back");
                int Width = 0;
                int Height = 0;
                Random rand = new Random();
                int MobCount = 0;
                while (Width != Tiles.LayerWidth)
                {
                    while (Height != Tiles.LayerHeight)
                    {
                        var position = new Vector2(Height * Game1.tileSize, Width * Game1.tileSize);
                        if (map.isTileLocationTotallyClearAndPlaceableIgnoreFloors(new Vector2(Height, Width)))
                        {
                            if (rand.Next(0, config.SpawnChancePerTile) == 0)
                            {
                                MobCount += 1;
                                var _color = new Color(rand.Next(0,255), rand.Next(0,255), rand.Next(0,255));
                                var Mob = new Monster();
                                var MobType = rand.Next(1, 5);
                                switch(MobType)
                                {
                                    case 1:
                                        Mob = new RockGolem(position: position);
                                        break;
                                    case 2:
                                        Mob = new Skeleton(position: position);
                                        break;
                                    case 3:
                                        Mob = new GreenSlime(position: position, color: _color);
                                        break;
                                    case 4:
                                        Mob = new DustSpirit(position: position, false);
                                        break;
                                }

                                int Max = 50;
                                if (config.ScaleWithCombatSkill)
                                {
                                    Max = Game1.player.CombatLevel * 100;
                                }
                                Mob.MaxHealth = rand.Next(0, rand.Next(1, Max));
                                Mob.Health = Mob.MaxHealth;
                                existingMobs.Add(Mob);
                                SpawnMonster(Mob, map);
                            }
                        }

                        Height += 1;
                    }

                    Height = 0;
                    Width += 1;
                }
                Monitor.Log($"Spawned {MobCount} Mobs at {map.Name}", LogLevel.Debug);
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            
            if (e.Button == SButton.End)
            {
                ClearArea();
            }
        }

        private void SpawnMonster(Monster monster, GameLocation location)
        {
            location.characters.Add(monster);
        }
    }
}