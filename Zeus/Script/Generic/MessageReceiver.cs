using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("MESSAGE RECEIVER", "Use this component with the vMessageSender to call Events.")]
    public class MessageReceiver : zMonoBehaviour
    {
        public static event OnReceiveMessage OnReceiveGlobalMessage;
        public List<MessageListener> MessagesListeners;
        [System.Serializable]
        public delegate void OnReceiveMessage(string name, string message = null);
        [System.Serializable]
        public class OnReceiveMessageEvent : UnityEngine.Events.UnityEvent<string> { }

        public event OnReceiveMessage OnReceiveLocalMessage;
        private void Start()
        {
            for (int i = 0; i < MessagesListeners.Count; i++)
            {
                MessageListener messageListener = MessagesListeners[i];
                if (messageListener.ReceiveFromGlobal)
                {
                    OnReceiveGlobalMessage -= messageListener.OnReceiveMessage;
                    OnReceiveGlobalMessage += messageListener.OnReceiveMessage;
                }
                else
                {
                    OnReceiveLocalMessage -= messageListener.OnReceiveMessage;
                    OnReceiveLocalMessage += messageListener.OnReceiveMessage;
                }
            }
        }
        [System.Serializable]
        public class MessageListener
        {
            public string Name;
            public bool ReceiveFromGlobal;
            public OnReceiveMessageEvent OnReceiveListenerMessage;

            public void OnReceiveMessage(string name, string message = null)
            {
                if (Name.Equals(name)) OnReceiveListenerMessage.Invoke(string.IsNullOrEmpty(message) ? string.Empty : message);

            }
            public MessageListener(string name)
            {
                this.Name = name;
            }
            public MessageListener(string name, UnityEngine.Events.UnityAction<string> listener)
            {
                this.Name = name;
                this.OnReceiveListenerMessage.AddListener(listener);
            }
        }

        /// <summary>
        /// Add Action Listener
        /// </summary>
        /// <param name="name">Message Name</param>
        /// <param name="listener">Action Listener</param>
        public void AddListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (MessagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = MessagesListeners.Find(l => l.Name.Equals(name));
                messageListener.OnReceiveListenerMessage.AddListener(listener);
            }
            else
            {
                MessagesListeners.Add(new MessageListener(name, listener));
            }
        }

        /// <summary>
        /// Remove Action Listener
        /// </summary>
        /// <param name="name">Message Name</param>
        /// <param name="listener">Action Listener</param>
        public void RemoveListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (MessagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = MessagesListeners.Find(l => l.Name.Equals(name));
                messageListener.OnReceiveListenerMessage.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Call Action without message
        /// </summary>
        /// <param name="name">message name</param>
        public void Send(string name)
        {
            if (this.enabled == false) return;
            OnReceiveLocalMessage?.Invoke(name, string.Empty);
        }

        /// <summary>
        /// Call Action with message
        /// </summary>
        /// <param name="name">message name</param>
        /// <param name="message">message value</param>
        public void Send(string name, string message)
        {
            if (this.enabled == false) return;
            OnReceiveLocalMessage?.Invoke(name, message);
        }

        public static void SendGlobal(string name, string message = null)
        {
            OnReceiveGlobalMessage?.Invoke(name, message);
        }
    }
}