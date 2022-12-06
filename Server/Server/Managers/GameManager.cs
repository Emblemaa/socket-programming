using Server.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Managers
{
    enum ErrorCode {
        PROFILE_REQUIRED,
        INVALID_ID, 
        NAME_REQUIRED,
        INVALID_NAME,
        NAME_EXISTED,
        ROOM_CREATED,
        ROOM_JOINED,
        ROOM_NOT_FOUND,
        ROOM_INPRGRESS,
        ROOM_1_PLAYER,
        UNSYNC
    }

    class GameManager
    {
        private static GameManager _inst;

        private Dictionary<ErrorCode, Error> errors;

        private GameManager() 
        {
            errors = new Dictionary<ErrorCode, Error>();
            errors.Add(ErrorCode.PROFILE_REQUIRED, new Error("PROFILE_REQUIRED", "A profile is required to proceed."));
            errors.Add(ErrorCode.INVALID_ID, new Error("INVALID_ID", "ID is invalid."));
            errors.Add(ErrorCode.NAME_REQUIRED, new Error("NAME_REQUIRED", "A name is required to proceed."));
            errors.Add(ErrorCode.INVALID_NAME, new Error("INVALID_NAME", "Name is invalid."));
            errors.Add(ErrorCode.NAME_EXISTED, new Error("NAME_EXISTED", "This name is already taken."));
            errors.Add(ErrorCode.ROOM_CREATED, new Error("ROOM_CREATED", "A room is already created by this player."));
            errors.Add(ErrorCode.ROOM_JOINED, new Error("ROOM_JOINED", "This player has already joined this room."));
            errors.Add(ErrorCode.ROOM_NOT_FOUND, new Error("ROOM_NOT_FOUND", "No room is found using this ID."));
            errors.Add(ErrorCode.ROOM_1_PLAYER, new Error("ROOM_1_PLAYER", "Cannot start a game with one player."));
            errors.Add(ErrorCode.UNSYNC, new Error("UNSYNC", "This is not your turn."));

            questions = new List<Question>();
            using (var reader = new StreamReader("quiz.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var question = new Question(
                        values[0], 
                        new List<string>() { 
                            values[1], 
                            values[2], 
                            values[3], 
                            values[4] }, 
                        int.Parse(values[5]));
                    questions.Add(question);
                }
            }
        }

        private List<Question> questions;

        public Exception GetException(ErrorCode code)
        {
            var exception = new Exception();
            exception.Data.Add("ERR", errors[code]);
            return exception;
        }

        public List<Question> GetQuestionSet(int playerNum)
        {
            var result = new List<Question>();
            var selectedQuestions = new List<int>();
            var random = new Random();
            int questionsLength = random.Next(playerNum * 3, questions.Count);
            int current = 0;
            while(current < questionsLength)
            {
                int tmp = random.Next(0, questions.Count);
                if (!selectedQuestions.Contains(tmp))
                {
                    current++;
                    result.Add(questions[tmp]);
                    selectedQuestions.Add(tmp);
                }
            }
            return result;
        }


        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static GameManager instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new GameManager();
                }
                return _inst;
            }
        }
    }
}
