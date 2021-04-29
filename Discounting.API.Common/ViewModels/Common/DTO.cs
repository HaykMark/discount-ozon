namespace Discounting.API.Common.ViewModels.Common
{
    /// <summary>
    /// Base class for Data Transfer Object models.
    /// </summary>
    public class DTO<TKey>
    {
        public TKey Id { get; set; }
        public string _type => GetType().Name.Replace("DTO", string.Empty);
    }
}