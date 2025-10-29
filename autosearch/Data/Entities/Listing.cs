namespace autosearch.Data.Entities;

public class Listing
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Price { get; set; }
    public string? Location { get; set; }
    public string ImageUrl { get; set; }
}

public class ListingDTO
{
    public string Url { get; set; }
    public string Title { get; set; }
    public string Price { get; set; }
    public string? Location { get; set; }
    public string ImageUrl { get; set; }
}

public class ListingMapper
{
    public static ListingDTO ToDTO(Listing listing)
    {
        return new ListingDTO
        {
            Url = listing.Url,
            Title = listing.Title,
            Price = listing.Price,
            Location = listing.Location,
            ImageUrl = listing.ImageUrl
        };
    }
}