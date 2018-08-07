namespace Game.Models.Messages
{
    using System;
    using System.Collections.Generic;

    public abstract class MessageBase
    {
        public enum MessageTypes
        {
            Hello,
            Ping,
            Spawn,
            View,
            ControlInput,
            Hook
        }

        public readonly static Dictionary<MessageTypes, Type> MessageTypeMap = 
            new Dictionary<MessageTypes, Type>
            {
                { MessageTypes.Hello,           typeof(Hello) },
                { MessageTypes.Ping,            typeof(Ping) },
                { MessageTypes.Spawn,           typeof(Spawn) },
                { MessageTypes.View,            typeof(View) },
                { MessageTypes.ControlInput,    typeof(ControlInput) },
                { MessageTypes.Hook,            typeof(Hook) }
            };

        public abstract MessageTypes Type { get; }
    }
}
