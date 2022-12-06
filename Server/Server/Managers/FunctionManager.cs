using Server.Managers;
using Server.Model;
using Server.Room;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Function
{

    class FunctionManager
    {
        private static FunctionManager _inst;

        private Dictionary<string, Action<Player, string[]>> functions;

        private FunctionManager()
        {
            functions = new Dictionary<string, Action<Player, string[]>>();
            functions.Add("CREATE_ROOM", CreateRoom);
            functions.Add("JOIN_ROOM", JoinRoom);
            functions.Add("START_GAME", StartGame);
            functions.Add("LEAVE_ROOM", LeaveRoom);
            functions.Add("UPDATE_INFO", UpdateInfo);
            functions.Add("UPDATE_PROFILE", UpdateName);
            functions.Add("ANSWER", Answer);
        }

        void sendError(Exception e, Player player)
        {
            if (e.Data["ERR"] is Error)
            {
                var error = e.Data["ERR"] as Error;
                player.SendPackage("ERROR", error.ToPacket, ConnectionType.TCP);
            }
            else
            {
                Console.WriteLine(e.Message);
                player.SendPackage("ERROR", new List<string>() { e.Message }, ConnectionType.TCP);
            }
        }

        private void UpdateInfo(Player player, string[] param)
        {
            player.gameInfo = param;
        }

        private void CreateRoom(Player player, string[] param)
        {
            Console.WriteLine("Create Room");
            try
            {
                if (player.name == null)
                {
                    throw GameManager.instance.GetException(ErrorCode.PROFILE_REQUIRED);
                }
                var room = RoomManager.instance.CreateRoom(player);
                player.SendPackage("JOIN_ROOM",
                    new List<string>() { room.id.ToString(), true.ToString(), player.ipep.ToString(), player.ipep.ToString() }, ConnectionType.TCP);
                room.BroadcastPlayers();
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        private void JoinRoom(Player player, string[] param)
        {
            try
            {
                if (player.name == null)
                {
                    throw GameManager.instance.GetException(ErrorCode.PROFILE_REQUIRED);
                }
                if (int.TryParse(param[0], out int roomId))
                {
                    RoomManager.instance.AddPlayerToRoom(roomId, player);
                    var room = RoomManager.instance.FindRoom(roomId);
                    player.SendPackage("JOIN_ROOM", new List<string>() {
                        param[0],
                        false.ToString(),
                        player.ipep.ToString(),
                        room.master.ipep.ToString()
                    }, ConnectionType.TCP);
                    room.BroadcastPlayers();
                }
                else throw GameManager.instance.GetException(ErrorCode.INVALID_ID);
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        private void StartGame(Player player, string[] param)
        {
            try
            {
                RoomManager.instance.StartGame(player);
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        private void LeaveRoom(Player player, string[] param)
        {
            try
            {
                RoomManager.instance.RemovePlayer(player.ipep);
                player.SendPackage("LEAVE_ROOM", new List<string>() { "Success" }, ConnectionType.TCP);
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        private void UpdateName(Player player, string[] param)
        {
            try
            {
                lock (this)
                {
                    if (param.Length == 0)
                    {
                        throw GameManager.instance.GetException(ErrorCode.NAME_REQUIRED);
                    }
                    if (!Regex.IsMatch(param[0], @"^[_a-zA-Z0-9]+$"))
                    {
                        throw GameManager.instance.GetException(ErrorCode.INVALID_NAME);
                    }
                    if (player.name != param[0])
                    {
                        foreach (var p in ServerCore.players)
                        {
                            Console.WriteLine(p.name);
                            if (param[0] == p.name)
                            {
                                throw GameManager.instance.GetException(ErrorCode.NAME_EXISTED);
                            }
                        }
                    }
                    player.name = param[0];
                    if (param.Length == 2)
                    {
                        int.TryParse(param[1], out int result);
                        player.avatar = result;
                    }
                    player.SendPackage("UPDATE_PROFILE", new List<string> { "Sucesss" }, ConnectionType.TCP);
                }
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        public void Answer(Player player, string[] param)
        {
            try
            {
                var room = RoomManager.instance.FindRoomByPlayer(player);
                if (param.Length == 0)
                {
                    throw new Exception("CHOICE_REQUIRED");
                }
                if (!int.TryParse(param[0], out int result))
                {
                    throw new Exception("INVALID_CHOICE");
                }
                room.Answer(player, result);
            }
            catch (Exception e)
            {
                sendError(e, player);
            }
        }

        public void MatchFunction(Player player, string name, string[] param)
        {
            if (functions.ContainsKey(name))
            {
                functions[name]?.Invoke(player, param);
            }
        }

        public static FunctionManager instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new FunctionManager();
                }
                return _inst;
            }
        }
    }
}
