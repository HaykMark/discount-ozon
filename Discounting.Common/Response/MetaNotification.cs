namespace Discounting.Common.Response
{
    public interface IMetaNotification
    {
        string type { get; }
        string message { get; }
    }

    public class BarNotification : IMetaNotification
    {
        public BarNotification(string startDate, string message)
        {
            this.startDate = startDate;
            this.message = message;
        }

        // used to show a bar at the top.
        public string type { get; } = "bar";
        public string startDate { get; }
        public string message { get; }
    }

    public class ToasterNotification : IMetaNotification
    {
        public ToasterNotification(string title, string message)
        {
            this.title = title;
            this.message = message;
        }

        // simply popups that can be closed
        public string type { get; } = "toaster";
        public string title { get; }
        public string message { get; }
    }

    public class DialogNotification : IMetaNotification
    {
        public DialogNotification(string title, string message)
        {
            this.title = title;
            this.message = message;
        }

        // span over the whole window
        public string type { get; } = "dialog";
        public string title { get; }
        public string message { get; }
    }
}