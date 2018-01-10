using Microsoft.Xna.Framework;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonDataItems;
using System;
using Sprites;
using GameComponentNS;
using System.Collections.Generic;


namespace MonoGameClient
{
    
    public class Game1 : Game
    {
        
        public List<Coin> AvaiableCoins = new List<Coin>();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        
        string connectionMessage = string.Empty;


        HubConnection serverConnection;
        IHubProxy proxy;

        public bool Connected { get; private set; }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

       
        protected override void Initialize()
        {
            serverConnection = new HubConnection("http://localhost:50983/");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();

            Action<PlayerData> joined = clientJoined;
            proxy.On<PlayerData>("Joined", joined);

            Action<List<PlayerData>> currentPlayers = clientPlayers;
            proxy.On<List<PlayerData>>("CurrentPlayers", currentPlayers);

            Action<string, Position> otherMove = clientOtherMoved;
            proxy.On<string, Position>("OtherMove", otherMove);


            //Action<List<CoinData>>("UpdateCoin", Update);

            base.Initialize();
        }

        private void clientOtherMoved(string playerID, Position newPos)
        {
            // iterate over all the other player components 
            // and check to see the type and the right id
            foreach (var player in Components)
            {
                if (player.GetType() == typeof(OtherPlayerSprite)
                    && ((OtherPlayerSprite)player).pData.playerID == playerID)
                {
                    OtherPlayerSprite p = ((OtherPlayerSprite)player);
                    p.pData.playerPosition = newPos;
                    p.Position = new Point(p.pData.playerPosition.X, p.pData.playerPosition.Y);
                    break; // break out of loop as only one player position is being updated
                           // and we have found it
                }
            }
        }


        // Only called when the client joins a game
        private void clientPlayers(List<PlayerData> otherPlayers)
        {
            foreach (PlayerData player in otherPlayers)
            {
                // Create an other player sprites in this client after
                new OtherPlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                                        new Point(player.playerPosition.X, player.playerPosition.Y));
                connectionMessage = player.playerID + " delivered ";
            }
        }

        private void clientJoined(PlayerData otherPlayerData)
        {
            // Create an other player sprite
            //use points in game to make movement smoother
            new OtherPlayerSprite(this, otherPlayerData, Content.Load<Texture2D>(otherPlayerData.imageName),
                                    new Point(otherPlayerData.playerPosition.X, otherPlayerData.playerPosition.Y));
            new FadeText(this, Vector2.Zero, otherPlayerData.GamerTag + "Has joined the game");
        }


        /// <summary>
        /// ////////////
        /// </summary>


        private void ServerConnection_StateChanged(StateChange State)
        {
            switch (State.NewState)
            {
                case ConnectionState.Connected:
                    connectionMessage = "Connected......";
                    Connected = true;
                    startGame();
                    break;
                case ConnectionState.Disconnected:
                    connectionMessage = "Disconnected.....";
                    if (State.OldState == ConnectionState.Connected)
                        connectionMessage = "Lost Connection....";
                    Connected = false;
                    break;
                case ConnectionState.Connecting:
                    connectionMessage = "Connecting.....";
                    Connected = false;
                    break;

            }
        }

        private void startGame()
        {
            // Continue on and subscribe to the incoming messages joined, currentPlayers, otherMove messages

            // Immediate Pattern
            proxy.Invoke<PlayerData>("Join")
                .ContinueWith( // This is an inline delegate pattern that processes the message 
                               // returned from the async Invoke Call
                        (p) => { // Wtih p do 
                            if (p.Result == null)
                                connectionMessage = "No player Data returned";
                            else
                            {
                                CreatePlayer(p.Result);
                                // Here we'll want to create our game player using the image name in the PlayerData 
                                // Player Data packet to choose the image for the player
                                // We'll use a simple sprite player for the purposes of demonstration 

                            }

                        });

            proxy.Invoke<CoinData>("CoinsCreate").ContinueWith
                (// This is an inline delegate pattern that processes the message 
                 // returned from the async Invoke Call
                (c) =>
                {
                    if (c.Result == null)
                    {
                        connectionMessage = "No Coin Data returned  ";

                    }
                    else
                    {
                        GenerateCoin(c.Result);
                    }
                }

                );

        }

        private void LeaveGame()
        {

            proxy.Invoke<PlayerData>("LeftGame");
        }

     

        private void GenerateCoin(CoinData coin)
        {
            new Coin(this, coin, Content.Load<Texture2D>(coin.imageName),
                new Point(coin.coinPos.X, coin.coinPos.Y));

        }


        private void CreatePlayer(PlayerData player)
        {
            new SimplePlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                new Point(player.playerPosition.X, player.playerPosition.Y));

            new FadeText(this, Vector2.Zero, " Welcome " + player.GamerTag + " you are playing as " + player.imageName);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService<SpriteBatch>(spriteBatch);
            font = Content.Load<SpriteFont>("Message");
            Services.AddService<SpriteFont>(font);

        }

        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);
        }

       
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);

            foreach (var item in Components)
            {
                if (item.GetType() == typeof(Coin))
                   {

                }
            }
         
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
