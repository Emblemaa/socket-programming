using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Model
{
    class Question
    {
        public Question(string question, List<string> options, int answer)
        {
            this.question = question;
            this.options = options;
            this.answer = answer;
        }

        public string question;
        public List<string> options;
        public int answer;
    }
}
