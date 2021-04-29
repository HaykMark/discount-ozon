namespace Discounting.Common.Types
{
    // <summary>
    /// Abstraction which represents an identity of entity.
    /// </summary>
    public interface IEntity<TKey> 
    {
        TKey Id { get; set; }
    }
}