using BookStoreApi.Attributes;


namespace BookStoreApi.Models
{
    public class Book: IAuditableEntity
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string ISBN { get; set; } = string.Empty;
        [Sensitive] // Marking as sensitive
        public decimal Price { get; set; }
        public bool Availability { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int Pages { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Edition { get; set; } = string.Empty;
    }
}
