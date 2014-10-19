using System;
using System.Drawing;
using System.Text;

namespace ProdigalSoftware.TiVE.Starter
{
    /// <summary>
    /// Holds one line of text in the message screen
    /// </summary>
    internal sealed class MessageLine
    {
        /// <summary>
        /// Linked list of messages that make up this line of text
        /// </summary>
        private Message m_firstMessage;

        /// <summary>
        /// Gets the height (in pixels) of this message line. 
        /// <see cref="CalculateHeight"/> must have been called first.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the text contained in this message line
        /// </summary>
        public string Text
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                ForeachMessage(message =>
                    {
                        TextMessage textMessage = message as TextMessage;
                        if (textMessage != null)
                            builder.Append(textMessage.Text);
                    });
                builder.Append('\n');
                return builder.ToString();
            }
        }

        /// <summary>
        /// Adds the specified message to this message line
        /// </summary>
        /// <param name="messageToAdd"></param>
        public void AddMessage(Message messageToAdd)
        {
            if (m_firstMessage == null)
                m_firstMessage = messageToAdd;
            else
            {
                Message message = m_firstMessage;
                while (message.NextMessage != null)
                    message = message.NextMessage;
                
                message.NextMessage = messageToAdd;
            }
        }

        /// <summary>
        /// Calculates the height of this message line using the specified state information and the specified graphics
        /// </summary>
        public void CalculateHeight(TextState state, Graphics g)
        {
            int maxHeight = 0;
            ForeachMessage(message =>
                {
                    Size size = Size.Empty;
                    TextMessage textMessage = message as TextMessage;
                    if (textMessage != null)
                        size = textMessage.GetSize(state, g);
                    message.UpdateState(state);
                    message.Size = size;
                    maxHeight = Math.Max(maxHeight, size.Height);
                });

            Height = maxHeight + 1; // for 1 pixel line
        }

        /// <summary>
        /// Draws this message line using the specified graphics and the specified text state
        /// </summary>
        public void Draw(TextState state, Graphics g)
        {
            ForeachMessage(message => 
                {
                    message.DrawText(state, g);
                    message.UpdateState(state);
                    state.X += message.Size.Width;
                });
        }

        /// <summary>
        /// Runs the specified action on each message in this message line
        /// </summary>
        private void ForeachMessage(Action<Message> action)
        {
            Message message = m_firstMessage;
            while (message != null)
            {
                action(message);
                message = message.NextMessage;
            }
        }
    }
}
