namespace Hackaton.Domain.Interface.Markers
{
    public interface IMustAudited
    {
        Guid CreationUserId { get; set; }
        DateTime CreationTime { get; set; }
        Guid? ModificationUserId { get; set; }
        DateTime? ModificationTime { get; set; }
    }
}
