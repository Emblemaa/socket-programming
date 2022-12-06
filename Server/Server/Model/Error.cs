using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Model
{
    class Error
    {
        private string errorCode;
        private string message;

        public Error(string errorCode, string message)
        {
            this.errorCode = errorCode;
            this.message = message;
        }

        public List<string> ToPacket {
            get
            {
                var result = new List<string>();
                result.Add(errorCode);
                result.Add(message);
                return result;
            }
        }
    }
}
