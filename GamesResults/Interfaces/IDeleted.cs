namespace GamesResults.Interfaces
{
    public interface IDeleted
    {
        public DateTime? DeletedAt { get; set; }

        public long? DeleterId { get; set; }

    }
}
