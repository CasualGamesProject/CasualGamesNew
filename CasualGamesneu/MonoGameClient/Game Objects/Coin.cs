using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CommonDataItems;
using Engine.Engines;
using Microsoft.Xna.Framework.Input;
using Microsoft.AspNet.SignalR.Client;

namespace Sprites
{
    public class Coin :DrawableGameComponent
    {

        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;
        public Color tint = Color.White;
        CoinData Coindata;


        public Coin(Game game,CoinData cData, Texture2D imageTexture, Point pos) : base(game)
        {
            Coindata = cData;
            game.Components.Add(this);
            Image = imageTexture;
            Position = pos;
            BoundingRect = new Rectangle((int)Position.X, Position.Y, Image.Width, Image.Height);

        }

        public override void Update(GameTime gameTime)
        {


            //if (Coin has been collected by player)
            //{
    //        Visible = false;
            //}

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            //Get SpriteBatch to draw
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            if (sp == null) return; // stops errors
            //Draws if image exists and Isn't visible (Collected)
            if (Image != null && Visible)
            {
                sp.Begin();
                sp.Draw(Image, BoundingRect, tint);
                sp.End();
            }

            base.Draw(gameTime);
        }


    }
}
