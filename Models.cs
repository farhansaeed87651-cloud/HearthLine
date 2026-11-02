namespace Hearthline;

public sealed class Resident
{
    public string Name { get; set; } = "";
    public string Apartment { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Lease { get; set; } = "";
    public string Status { get; set; } = "Active";
}

public sealed class Apartment
{
    public string Number { get; set; } = "";
    public string Resident { get; set; } = "";
    public string Occupancy { get; set; } = "Occupied";
    public string Shower { get; set; } = "Good";
    public string AirConditioning { get; set; } = "Good";
    public string Heating { get; set; } = "Good";
    public string Appliances { get; set; } = "Good";
}

public sealed class MaintenanceRequest
{
    public string Id { get; set; } = "";
    public string Apartment { get; set; } = "";
    public string Issue { get; set; } = "";
    public string Category { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public string Reported { get; set; } = "";
}

public sealed class GuestPass
{
    public string Guest { get; set; } = "";
    public string Host { get; set; } = "";
    public string Apartment { get; set; } = "";
    public string Arrival { get; set; } = "";
    public string PassStatus { get; set; } = "Expected";
}

public sealed class Announcement
{
    public string Headline { get; set; } = "";
    public string Message { get; set; } = "";
    public string Audience { get; set; } = "All residents";
    public string Published { get; set; } = "";
}

public sealed class BuildingEvent
{
    public string Name { get; set; } = "";
    public string When { get; set; } = "";
    public string Location { get; set; } = "";
    public string Attendance { get; set; } = "Open";
}

public sealed class BuildingSnapshot
{
    public List<Resident> Residents { get; set; } = [];
    public List<Apartment> Apartments { get; set; } = [];
    public List<MaintenanceRequest> Maintenance { get; set; } = [];
    public List<GuestPass> Guests { get; set; } = [];
    public List<Announcement> Announcements { get; set; } = [];
    public List<BuildingEvent> Events { get; set; } = [];
}