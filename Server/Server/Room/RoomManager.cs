using Server.Managers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Server.Room
{
    class RoomManager
    {
        private static RoomManager _inst;
        private List<Room> rooms;
        private int currentIndex;

        private RoomManager()
        {
            rooms = new List<Room>();
            currentIndex = 1;
        }

       

        public static RoomManager instance
        {
            get
            {
                if(_inst == null )
                {
                    _inst = new RoomManager();
                }
                return _inst;
            }
        }
       
        

        public List<Room> GetRooms()
        {
            return rooms;
        }

        public Room CreateRoom(Player player)
        {
            foreach(var room in rooms)
            {
                if(room.HasPlayer(player.ipep))
                {
                    throw GameManager.instance.GetException(ErrorCode.ROOM_CREATED);
                }
            }
            lock (this)
            {
                Room newRoom = new Room(currentIndex, player);
                newRoom.AddPlayer(player);
                rooms.Add(newRoom);
                currentIndex++;
                return newRoom;
            }
        }

        public bool DeleteRoom(int id)
        {
            try
            {
                // Check any room with given id
                foreach (Room room in rooms)
                {
                    if (room.id == id)
                    {
                        lock (this)
                        {
                            rooms.Remove(room); 
                            return true;
                        }
                    }
                }
                // No room with given id
                return false;   
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool AddPlayerToRoom(int roomId, Player player)
        {
            foreach (var r in rooms)
            {
                if (r.HasPlayer(player.ipep))
                {
                    throw GameManager.instance.GetException(ErrorCode.ROOM_JOINED);
                }
            }
            // Find room
            Room room = FindRoom(roomId);
            if (room != null)
            {
                lock (this)
                {
                    var playerAdded = room.AddPlayer(player);
                    return playerAdded;
                }
            }
            throw GameManager.instance.GetException(ErrorCode.ROOM_NOT_FOUND);
        }

        public bool RemovePlayerFromRoom(Room room, IPEndPoint playerIpep)
        {
            try
            {
                // Find room
                if (room != null)
                {
                    bool result = room.RemovePlayer(playerIpep);
                    if(room.IsEmpty)
                    {
                        rooms.Remove(room);
                    }
                    return result;
                }
                return false;   // no room
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if(e.Message == "LEAVE_ROOM")
                {
                    rooms.Remove(room);
                    return true;
                }
                return false;
            }
        }

        public void RemoveRoom(Room room)
        {
            rooms.Remove(room);
        }

        public void RemovePlayer(IPEndPoint playerIpep)
        {
            try
            {
                lock(rooms)
                {
                    foreach (Room room in rooms)
                    {
                        lock (room)
                        {
                            RemovePlayerFromRoom(room, playerIpep);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Room FindRoom(int id)
        {
            foreach(Room room in rooms)
            {
                if(room.id == id)
                {
                    return room;
                }
            }
            return null;
        }

        public Room FindRoomByMaster(Player player)
        {
            return rooms.Find((room) => room.CheckMaster(player));
        }

        public Room FindRoomByPlayer(Player player)
        {
            return rooms.Find((room) => room.HasPlayer(player.ipep));
        }

        public void StartGame(Player player)
        {
            Room room = FindRoomByMaster(player);
            if(room == null)
            {
                throw GameManager.instance.GetException(ErrorCode.ROOM_NOT_FOUND);
            }
            if(room.players.Count < 2)
            {
                throw GameManager.instance.GetException(ErrorCode.ROOM_1_PLAYER);
            }
            room.Start(player);
        }
    }
}
