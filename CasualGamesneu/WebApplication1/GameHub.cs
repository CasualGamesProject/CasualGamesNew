﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using CommonDataItems;

namespace WebApplication1
{
    public class GameHub : Hub
    {

        //Queue of registered players
        public static Queue<PlayerData> RegisteredPlayers = new Queue<PlayerData>(new PlayerData[]
        {
            new PlayerData {GamerTag = "Wengu", imageName = "", playerID = Guid.NewGuid().ToString(), Coins = 0 },
            new PlayerData {GamerTag = "Crush", imageName = "", playerID = Guid.NewGuid().ToString(), Coins = 0 },
            new PlayerData {GamerTag = "Hawke", imageName = "", playerID = Guid.NewGuid().ToString(), Coins = 0 },
            new PlayerData {GamerTag = "Bazooka", imageName = "", playerID = Guid.NewGuid().ToString(), Coins = 0 },

        });

        public static Queue<CoinData> coinDisplay = new Queue<CoinData>(new CoinData[]
      {
            new CoinData {  imageName = "", coinId = Guid.NewGuid().ToString()},
            new CoinData {  imageName = "", coinId = Guid.NewGuid().ToString()},
            new CoinData {  imageName = "", coinId = Guid.NewGuid().ToString()},
            new CoinData {  imageName = "", coinId = Guid.NewGuid().ToString()},
      });



        public static List<CoinData> coins = new List<CoinData>();

        public static Stack<string> coin = new Stack<string>(new string[] { "Coin" });

        //Generate Coins
        public void GenerateCoins()
        {
            Random r = new Random();

            for (int i = 0; i < 50; i++)
            {
               coins.Add(new CoinData { coinPos = new Position { X = r.Next(-300, 800), Y = r.Next(-50, 550) } });
            }
         
        }

        //Used to send coin information to clients
            public List<CoinData> CDATA()
            {
            List<CoinData> coinDATA = new List<CoinData>();

            coinDATA = coins;


            return coinDATA;
            }




        public static List<PlayerData> Players = new List<PlayerData>();

        public static Stack<string> characters = new Stack<string>(new string[] {"Player 4", "Player 3", "Player 2", "Player 1" });

        public void Hello()
        {
            Clients.All.hello();
        }


        public PlayerData Join()
        {

            if (Players.Count == 0)
            {
                GenerateCoins();
            }


            // Check and if the charcters
            if (characters.Count > 0)
            {
                // pop name
                string character = characters.Pop();
                // if there is a registered player
                if (RegisteredPlayers.Count > 0)
                {
                    PlayerData newPlayer = RegisteredPlayers.Dequeue();
                    newPlayer.imageName = character;
                    newPlayer.playerPosition = new Position
                    {
                        X = new Random().Next(700),
                        Y = new Random().Next(500)
                    };
                    // Tell all the other clients that this player has Joined
                    Clients.Others.Joined(newPlayer);
                    // Tell this client about all the other current 
                    Clients.Caller.CurrentPlayers(Players);
                    // add the new player on the server
                    Players.Add(newPlayer);
                    return newPlayer;
                }


            }
            return null;
        }


        public void Moved(string playerID, Position newPosition)
        {
            // Update the collection with the new player position is the player exists
            PlayerData found = Players.FirstOrDefault(p => p.playerID == playerID);

            if (found != null)
            {
                // Update the server player position
                found.playerPosition = newPosition;
                // Tell all the other clients this player has moved
                Clients.Others.OtherMove(playerID, newPosition);
            }
        }


        //Robs coin generation(Not used)
        public CoinData CoinsJoin()
        {
            if (characters.Count > 0)
            {
                string coinsCollection = coin.Pop();
                // if there is a registered player
                if (coinDisplay.Count > 0)
                {

                    CoinData newCoin = coinDisplay.Dequeue();
                    newCoin.imageName = coinsCollection;
                    newCoin.coinPos = new Position
                    {
                        X = new Random().Next(700),
                        Y = new Random().Next(500)
                    };
                    // Tell all the other clients that this player has Joined
                    Clients.Others.coinJoined(newCoin);
                    // Tell this client about all the other current 
                    Clients.Caller.clientCoins(coins);
                    // Finaly add the new player on teh server
                    coins.Add(newCoin);
                    return newCoin;
                }
            }
            return null;
        }


        //when player presses escape or red X to exit, this method is called
        //Allows assets and names to be reused but doesn't stop their image being drawn
        public void LeftGame(PlayerData pdata)
        {
            RegisteredPlayers.Enqueue(pdata);
            characters.Push(pdata.imageName); //Allows image to be used again
           
            Clients.Others.Left(pdata); // Calls the Action<PlayerData> left in the client
            Players.Remove(pdata); // remove from players on server data
        }



    }
}