using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGameClient
{
    class Tile
    {
        //it's protected, so texture is only used in this class
        protected Texture2D texture;

        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            
            get { return rectangle; }
            protected set { rectangle = value; }          
        }

        //Used to share the content loader(Tile1, Tile2 etc)
        private static ContentManager content;
        public static ContentManager Content
        {
            protected get { return content; }
            set { content = value; }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, Color.White);
        }
    }

    // used to give the ability of having multiple tiles(name each tile Tile1, Tile2 etc)
    class CollisionTiles : Tile
    {
        public CollisionTiles(int i, Rectangle newRectangle)
        {
            texture = Content.Load<Texture2D>("Tile" + i);
            this.Rectangle = newRectangle;
        }
    }
}
