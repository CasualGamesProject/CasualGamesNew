using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataItems
{
  public class Position
    {
        public int X;
        public int Y;
    }

    public class PlayerData
    {

        public string playerID;
        public string imageName = string.Empty;
        public string GamerTag = string.Empty;
        public string PlayerName = string.Empty;
        public int Coins;
        public Position playerPosition;
        public string Password;
    }



    public class CoinData
    {
        public string coinID;
        public string imageName = string.Empty;
        public Position coinPos;

    }

    //used to share the coin data between players
    public class GenericInfo
    {

        public static List<CoinData> Coins = new List<CoinData>();

    }

}
