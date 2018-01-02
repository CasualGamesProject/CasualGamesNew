﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CommonDataItems;
using Engine.Engines;
using Microsoft.Xna.Framework.Input;
using Microsoft.AspNet.SignalR.Client;

namespace Sprites
{
    public class Coin : DrawableGameComponent
    {
        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;
        public Color tint = Color.White;

        public Coin(Game game, Texture2D spriteimage,Point pos) : base(game)
        {
            Image = spriteimage;
            Position = pos;
            BoundingRect = new Rectangle((int)Position.X, Position.Y, Image.Width, Image.Height);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
            if (sb == null) return;
            if (Image != null && Visible)
            {
                sb.Begin();
                sb.Draw(Image, BoundingRect, tint);
                sb.End();
            }

            base.Draw(gameTime);
        }


    }
}
