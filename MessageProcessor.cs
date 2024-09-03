using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace trivia_client
{
    public static class MessageProcessor
    {
        public enum ActionCode : byte
        {
            Login = 111,
            LoginResponse = 100,
            ChatMessage = 3,
            UserList = 4,
            RoomList = 5,
            CreateRoom = 6,
            JoinRoom = 7,
            LeaveRoom = 8
            // Add more action codes as needed
        }

        public static (byte ActionCode, string JsonContent) CreateMessage(ActionCode action, object content)
        {
            string jsonContent = JsonConvert.SerializeObject(content);
            return ((byte)action, jsonContent);
        }

        public static (ActionCode Action, JToken Content) ProcessMessage(byte actionCode, string jsonContent)
        {
            ActionCode action = (ActionCode)actionCode;
            JToken content = JToken.Parse(jsonContent);
            return (action, content);
        }
    }
}