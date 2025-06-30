namespace GamesResults.Interfaces
{
    public interface IEditable
    {
        public DateTime? ModifiedAt { get; set; }

        public long? EditorId { get; set; }

    }
}
