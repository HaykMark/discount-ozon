namespace Discounting.Common.Types
{
    /// <summary>
    /// Marker interface with IsActive property for entities to have shadow conceptual-property Deleted.
    /// </summary>
    public interface IActivatable
    {
        bool IsActive { get; set; }
    }
}