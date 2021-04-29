namespace Discounting.Common.Types
{
    public interface IWithSystemFlag
    {
        /// <summary>
        /// If this propery is set to true, it marks the entity as default type
        /// </summary>
        bool IsSystemDefault { get; set; }
    }
}