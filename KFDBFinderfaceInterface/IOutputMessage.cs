using System;

namespace KFDBFinder.Extensions
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        None,
        Info,
        Debug,
        Warn,
        Error,
    }

    public enum MessageDepartment
    {
        None,
        Client = 0x01,// 
        Script = 0x02,
        Resource = 0x04,
        Engine = 0x08,
    }

    public abstract class MessageEventArgs : EventArgs
    {
        public virtual string ErrorFile { get; set; } = "";
        public virtual string Message { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual MessageType Type { get; } = MessageType.None;

        public virtual MessageDepartment Department { set; get; } = MessageDepartment.Client;//归属部门
    }

    public sealed class InfoMessageEventArgs : MessageEventArgs
    {
        public override MessageType Type { get; } = MessageType.Info;
    }

    public sealed class DebugMessageEventArgs : MessageEventArgs
    {
        public override MessageType Type { get; } = MessageType.Debug;
    }

    public sealed class WarnMessageEventArgs : MessageEventArgs
    {
        public override MessageType Type { get; } = MessageType.Warn;
    }

    public sealed class ErrorMessageEventArgs : MessageEventArgs
    {
        public override MessageType Type { get; } = MessageType.Error;
    }

    public interface IOutputMessage
    {
        /// <summary>
        /// 输出消息消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void OutputMessageHandler(object sender, MessageEventArgs args);

        public event OutputMessageHandler OutputMessageEvent;
    }
}
