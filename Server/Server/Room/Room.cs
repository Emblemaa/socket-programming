using Server.Function;
using Server.Managers;
using Server.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server.Room
{
    class Room
    {
        public int id
        {
            get;
        }
        public List<Player> players;
        public Player master;
        private bool inProgress;

        private List<Question> questionSet;
        private int currentQuestion;
        private int currentPlayer;
        private int? currentAnswer;
        private List<int> playerOrder;
        private List<int> outOfPass;
        private List<int> eliminated;
        private Thread gameThread;
        private List<Player> removeLater; 
        

        public bool IsEmpty
        {
            get
            {
                return players.Count == 0;
            }
        }

        public Room(int id, Player master)
        {
            this.id = id;
            players = new List<Player>();
            eliminated = new List<int>();
            outOfPass = new List<int>();
            removeLater = new List<Player>();
            this.master = master;
            inProgress = false;
            currentPlayer = 0;
            currentQuestion = 0;
        }

        public bool CheckMaster(Player player)
        {
            return player.ipep == master.ipep;
        }

        private Player FindPlayer(IPEndPoint playerIpep)
        {
            foreach (Player player in players)
            {
                if (player.match(playerIpep))
                {
                    return player;
                }
            }
            return null;
        }

        public bool HasPlayer(IPEndPoint playerIpep)
        {
            Player player = FindPlayer(playerIpep);
            if (player != null)
            {
                return true;
            }
            return false;
        }

        public List<Player> GetPlayers()
        {
            return players;
        }

        public bool AddPlayer(Player player)
        {
            if(inProgress)
            {
                throw GameManager.instance.GetException(ErrorCode.ROOM_INPRGRESS);
            }
            // UPDATE_PLAYER#IPEP1|IPEP2...
            try
            {
                // Check if player exists
                Player checkPlayer = FindPlayer(player.ipep);
                
                if (checkPlayer == null)
                {
                    // Add player to room
                    lock (this)
                    {
                        this.players.Add(player);
                    }
                    return true;
                }

                return false;   // player exists
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool RemovePlayer(IPEndPoint playerIpep)
        {
            // UPDATE_PLAYER#IPEP1|IPEP2|...
            try
            {
                // Find player with given id
                foreach (Player player in players)
                {
                    if (player.match(playerIpep))
                    {
                        if(!inProgress)
                        {
                            lock (this)
                            {
                                players.Remove(player);
                                if (player.ipep != master.ipep)
                                {
                                    BroadcastPlayers();
                                }
                                else
                                {
                                    Broadcast("LEAVE_ROOM", null);
                                    players.Clear();
                                    RoomManager.instance.RemoveRoom(this);
                                }
                            }
                            return true;
                        }
                        else
                        {
                            for(int i = 0; i < playerOrder.Count; i++)
                            {
                                if(players[playerOrder[i]].match(player.ipep))
                                {
                                    lock (eliminated)
                                    {
                                        eliminated.Add(i); 
                                        break;
                                    }
                                }
                            }
                            removeLater.Add(player);
                            return true;
                        }
                    }
                }

                return false;   // No player with given id
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void Shuffle<T>(List<T> array)
        {
            var rng = new Random();
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public void Start(Player player)
        {
            lock(this)
            {
                if(player.ipep == master.ipep)
                {
                    questionSet = GameManager.instance.GetQuestionSet(players.Count);
                    playerOrder = new List<int>();
                    for(int i =0; i < players.Count; i++)
                    {
                        playerOrder.Add(i);
                    }
                    Shuffle<int>(playerOrder);
                    inProgress = true;
                    Broadcast("GAME_START", null);
                    StartGameThread();
                }
                else
                {
                    throw new Exception("CANNOT_START");
                }
            }
        }

        public void BroadcastPlayers()
        {
            var param = new List<string>();
            foreach(var player in players)
            {
                if(!removeLater.Contains(player))
                {
                    param.Add(player.ipep.ToString());
                    param.Add(player.name);
                    param.Add(player.avatar.ToString());
                }
            }
            Broadcast("LIST_PLAYER", param);
        }

        public void Broadcast(String type, List<string> param)
        {
            foreach(var player in players)
            {
                if(!removeLater.Contains(player))
                {
                    player.SendPackage(type, param, ConnectionType.TCP);
                }
            }
        }
        public void BroadcastWithoutPlayer(String type, List<string> param, Player target)
        {
            foreach (var player in players) if(!player.Equals(target))
            {
                player.SendPackage(type, param, ConnectionType.TCP);
            }
        }
        private void Reset()
        {
            currentAnswer = null;
            currentPlayer = 0;
            currentQuestion = 0;
            eliminated.Clear();
            outOfPass.Clear();
            questionSet.Clear();
            foreach(var player in removeLater)
            {
                lock (this)
                {
                    players.Remove(player);
                    if (player.ipep != master.ipep)
                    {
                        BroadcastPlayers();
                    }
                    else
                    {
                        Broadcast("LEAVE_ROOM", null);
                        players.Clear();
                        RoomManager.instance.RemoveRoom(this);
                    }
                }
            }
            removeLater.Clear();
        }

        public void Answer(Player player, int choice)
        {
            if (!players[playerOrder[currentPlayer]].match(player.ipep)) {
                throw GameManager.instance.GetException(ErrorCode.UNSYNC);
            }
            lock(this)
            {
                currentAnswer = choice;
            }
        }

        private void StartGameThread()
        {
            
            gameThread = new Thread(new ThreadStart(() =>
            {
                while (inProgress)
                {
                    int countSecond = 20;
                    if (eliminated.Contains(currentPlayer))
                    {
                        currentPlayer++;
                        if (currentPlayer == players.Count)
                        {
                            currentPlayer = 0;
                        }
                        continue;
                    }
                    if (currentQuestion == questionSet.Count - 1 || eliminated.Count == players.Count - 1)
                    {
                        Broadcast("WIN", new List<string>() { players[playerOrder[currentPlayer]].ipep.ToString() });
                        inProgress = false;
                        break;
                    }
                    Broadcast("QUESTION",new List<string>() { 
                        players[playerOrder[currentPlayer]].ipep.ToString(),
                        questionSet[currentQuestion].question,
                        questionSet[currentQuestion].options[0],
                        questionSet[currentQuestion].options[1], 
                        questionSet[currentQuestion].options[2],
                        questionSet[currentQuestion].options[3]
                    });
                    while (countSecond != 0)
                    {
                        if (eliminated.Contains(currentPlayer) || eliminated.Count == players.Count - 1)
                        {
                            break;
                        }
                        if (currentAnswer != null)
                        {
                            Console.WriteLine(currentAnswer);
                            if(currentAnswer == -1)
                            {
                                if (outOfPass.Contains(currentPlayer))
                                {     
                                    players[playerOrder[currentPlayer]].SendPackage(
                                        "ERROR", 
                                        new List<string>() { 
                                            "OUT_OF_PASS", 
                                            "You cannot pass anymore." }, 
                                        ConnectionType.TCP);
                                    currentAnswer = null;
                                }
                                else
                                {
                                    outOfPass.Add(currentPlayer);
                                    currentAnswer = null;
                                    currentPlayer++;
                                    if (currentPlayer == players.Count)
                                    {
                                        currentPlayer = 0;
                                    }
                                    break;
                                }
                            }
                            else if(currentAnswer != questionSet[currentQuestion].answer)
                            {
                                Broadcast("RESULT", new List<string> { (-1).ToString() });
                                eliminated.Add(currentPlayer);
                                currentPlayer++;
                                if(currentPlayer == players.Count)
                                {
                                    currentPlayer = 0;
                                }
                                currentAnswer = null;
                                break;
                            }
                            else
                            {
                                Broadcast("RESULT", new List<string> { (1).ToString() });
                                currentAnswer = null;
                                break;
                            }
                            
                            
                        }
                        Thread.Sleep(1000);
                        countSecond--;
                    }
                    if(countSecond == 0)
                    {
                        Broadcast("RESULT", new List<string> { (0).ToString() });
                        eliminated.Add(currentPlayer);
                        currentPlayer++;
                        if (currentPlayer == players.Count)
                        {
                            currentPlayer = 0;
                        }
                    }
                    countSecond = 20;
                    currentQuestion++;
                }
                Reset();
            }));
            gameThread.Start();
        }
    }
}
