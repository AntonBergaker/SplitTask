using Newtonsoft.Json.Linq;
using Planner;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFunctions
{
    //TODO more consistent.
    class RecievedDataEventArgs : EventArgs
    {
        public string textData { get { return Encoding.UTF8.GetString(byteData); } }
        public byte[] byteData { get; set; }
    }
    class RecievedTasksEventArgs : EventArgs
    {
        public TaskCollection tasks { get; set; }
        public int senderID {get; set; }
    }
    class RecievedTaskEventArgs : EventArgs
    {
        public Task task { get; set; }
        public string parentTask { get; set; }
    }
    class RecievedJsonEventArgs : EventArgs
    {
        public int senderID { get; set; }
        public bool sendToAll { get; set; }
        public JObject obj { get; set; }
    }
}
