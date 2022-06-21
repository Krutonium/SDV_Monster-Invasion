using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using StardewValley.Monsters;
using StardewValley.Characters;
using StardewValley.Network;

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

        public List<GameLocation> hasMobs = new List<GameLocation>();
        public List<Monster> existingMobs = new List<Monster>();
        
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.Warped += PlayerOnWarped;
        }

        private void PlayerOnWarped(object sender, WarpedEventArgs e)
        {
            Monitor.Log($"Warped to {e.NewLocation}");
            if (!hasMobs.Contains(e.NewLocation))
            {
                hasMobs.Add(e.NewLocation);
                var Tiles = e.NewLocation.map.GetLayer("Back");
                int Width = 0;
                int Height = 0;
                Random rand = new Random();
                while (Width != Tiles.LayerWidth)
                {
                    while (Height != Tiles.LayerHeight)
                    {
                        //if (Tiles.Tiles[Width, Height].TileIndexProperties.ContainsKey("Diggable"))
                        //{
                            //if (rand.Next(0, 9) == 4)
                            {
                                SpawnMonster(new GreenSlime(new Vector2(Height, Width)));
                                //Nothing is Spawned
                            }
                        //}

                        Height += 1;
                    }

                    Width += 1;
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            
            if (e.Button == SButton.End)
            {
                SpawnMonster(new GreenSlime(Game1.player.position));
                //Works fine
            }
        }

        public void SpawnMonster(Monster monster)
        {
            Game1.currentLocation.characters.Add(monster);
            this.Monitor.Log($"Spawned slime");
        }
    }
}