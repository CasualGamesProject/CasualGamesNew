using Microsoft.Xna.Framework;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonDataItems;
using System;
using Sprites;
using GameComponentNS;
using System.Collections.Generic;
using Engine.Engines;
using Microsoft.Xna.Framework.Media;


namespace MonoGameClient
{
    
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D coinImage;

        Map map;//t

       

        string connectionMessage = string.Empty;

        Texture2D background;

        //For game boundries 
        Rectangle gameView;
        HubConnection serverConnection;
        IHubProxy proxy;

        //Coin collectable collections
        List<Coin> DisplayCoins = new List<Coin>(); // Client's Coins to be displayed
        List<CoinData> testlist = new List<CoinData>(); // used to grab info from server

        //Used for identifying the client's player
        PlayerData clientUserData;


        public bool Connected { get; private set; }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = false;

            //fixed screen resolution (can sometimes stretch sprites)
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.ApplyChanges();

        }


        protected override void Initialize()
        {

            new InputEngine(this);

            //Our Azure server can be swapped with local host (http://s00162322gameserver.azurewebsites.net)
          

            //Peter's Server Connection
       //     serverConnection = new HubConnection("https://s00162137gameserver.azurewebsites.net");

            //hosting locally 
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

            //gets called by proxy to send leftmessage
            Action<PlayerData> left = LeaveGame;
            proxy.On<PlayerData>("Left", left);
            

            Services.AddService<IHubProxy>(proxy);

            map = new Map();
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
            // Create another player sprite
            //use points in game to make movement smoother
            new OtherPlayerSprite(this, otherPlayerData, Content.Load<Texture2D>(otherPlayerData.imageName),
                                    new Point(otherPlayerData.playerPosition.X, otherPlayerData.playerPosition.Y));
            new FadeText(this, Vector2.Zero, otherPlayerData.GamerTag + "Has joined the game");
        }   

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
        }

        private void CreatePlayer(PlayerData player)
        {
            new SimplePlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                new Point(player.playerPosition.X, player.playerPosition.Y));
            clientUserData = player;
            new FadeText(this, Vector2.Zero, " Welcome " + player.GamerTag + " you are playing as " + player.imageName);

            //To display coins on Load
            GenerateCoin();
        }
     
        private void LeaveGame(PlayerData playdata)
        {
          //Displays message when player leaves
            new FadeText(this, Vector2.Zero, playdata.GamerTag + " has left the game");

        }

        private void GenerateCoin()
        {
            //Calls server for CoinList data
            proxy.Invoke<List<CoinData>>("CDATA")
                .ContinueWith(
                (c) =>
                {
                    if (c.Result != null)
                    {
                        testlist = c.Result; // is equal t server's coin collection

                        //create coins for each coindata
                        foreach (CoinData cData in testlist)
                        {
                            DisplayCoins.Add(new Coin(this, cData, coinImage,
                            new Point(cData.coinPos.X, cData.coinPos.Y)));
                        }
                    }
                });

        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService<SpriteBatch>(spriteBatch);
            font = Content.Load<SpriteFont>("Message");
            Services.AddService<SpriteFont>(font);

            //Load in the background
           //background = Content.Load<Texture2D>("space");

            //worldsize
            gameView = new Rectangle(0, 0, GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height * 2);
            coinImage = Content.Load<Texture2D>("coin");


            //load music
            Song song = Content.Load<Song>("8bit");  
            MediaPlayer.Play(song);



            Tile.Content = Content;//t
            map.Generate(new int[,] {
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
                {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
                {3,3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,3,3},
            }, 64);

        }

       
      
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {

                // Calls the server's leave game method
                proxy.Invoke<PlayerData>("LeftGame", clientUserData).ContinueWith( // This is an inline delegate pattern that processes the message 
                                                                   // returned from the async Invoke Call
                        (p) => { // Wtih p do 
                            if (p.Result == null)
                                connectionMessage = "No player Data returned";
                            else
                            {
                                LeaveGame(clientUserData);
                            }

                        });

                Exit(); //Close down game/client
            }


            base.Update(gameTime);
        }

       
        protected override void Draw(GameTime gameTime)
         {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            map.Draw(spriteBatch);

            //Draw messages
            spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);

            //Draws all generated Coins
            foreach (Coin item in DisplayCoins)
            {
                item.Draw(gameTime);
            }
            //draw background
            //spriteBatch.Draw(background, gameView, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
