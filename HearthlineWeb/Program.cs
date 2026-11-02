using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5050");

var residents = new[]
{
    new { Id = "R-101", Name = "Ava Thompson", Apartment = "A-101", Email = "ava@hearthline.example", Status = "Occupied", Phone = "+1 (555) 010-1010" },
    new { Id = "R-102", Name = "Noah Patel", Apartment = "A-102", Email = "noah@hearthline.example", Status = "Occupied", Phone = "+1 (555) 010-1011" },
    new { Id = "R-103", Name = "Sofia Nguyen", Apartment = "B-211", Email = "sofia@hearthline.example", Status = "VIP", Phone = "+1 (555) 010-1012" },
    new { Id = "R-104", Name = "Liam Garcia", Apartment = "C-305", Email = "liam@hearthline.example", Status = "Occupied", Phone = "+1 (555) 010-1013" },
    new { Id = "R-105", Name = "Emma Brooks", Apartment = "D-420", Email = "emma@hearthline.example", Status = "New", Phone = "+1 (555) 010-1014" },
    new { Id = "R-106", Name = "Mason Lee", Apartment = "E-120", Email = "mason@hearthline.example", Status = "Occupied", Phone = "+1 (555) 010-1015" }
};

var apartments = new[]
{
    new { Number = "A-101", Resident = "Ava Thompson", Shower = "Working", AirConditioning = "Checked", Heating = "Stable", Appliances = "OK" },
    new { Number = "A-102", Resident = "Noah Patel", Shower = "Needs repair", AirConditioning = "Working", Heating = "Stable", Appliances = "OK" },
    new { Number = "B-211", Resident = "Sofia Nguyen", Shower = "Working", AirConditioning = "Working", Heating = "Stable", Appliances = "Needs review" },
    new { Number = "C-305", Resident = "Liam Garcia", Shower = "Working", AirConditioning = "Working", Heating = "Stable", Appliances = "OK" },
    new { Number = "D-420", Resident = "Emma Brooks", Shower = "Working", AirConditioning = "Maintenance", Heating = "Stable", Appliances = "OK" },
    new { Number = "E-120", Resident = "Mason Lee", Shower = "Working", AirConditioning = "Working", Heating = "Stable", Appliances = "OK" }
};

var maintenance = new[]
{
    new { Id = "M-1001", Apartment = "A-102", Issue = "Shower pressure weak", Category = "Plumbing", Priority = "High", Status = "Open" },
    new { Id = "M-1002", Apartment = "D-420", Issue = "AC not cooling", Category = "HVAC", Priority = "Urgent", Status = "In progress" },
    new { Id = "M-1003", Apartment = "B-211", Issue = "Microwave door hinge", Category = "Appliances", Priority = "Medium", Status = "Scheduled" },
    new { Id = "M-1004", Apartment = "C-305", Issue = "Bathroom leak", Category = "Plumbing", Priority = "High", Status = "Open" }
};

var guests = new[]
{
    new { Guest = "Olivia Grant", Host = "Ava Thompson", Apartment = "A-101", Arrival = "2026-11-04 18:30", PassStatus = "Approved" },
    new { Guest = "Daniel Sanchez", Host = "Noah Patel", Apartment = "A-102", Arrival = "2026-11-05 13:00", PassStatus = "Pending" },
    new { Guest = "Mia Chen", Host = "Sofia Nguyen", Apartment = "B-211", Arrival = "2026-11-06 20:15", PassStatus = "Approved" }
};

var announcements = new[]
{
    new { Id = 1, Headline = "Water main inspection", Message = "The building water system will be inspected on Friday from 9:00 AM to noon. Please limit usage during this window.", Audience = "all", Published = "Today" },
    new { Id = 2, Headline = "Holiday event signup", Message = "Residents can RSVP for the community holiday gathering before Wednesday at 6:00 PM.", Audience = "residents", Published = "This week" },
    new { Id = 3, Headline = "Guest lobby notice", Message = "Visitor check-in is available at the front desk from 7:00 AM to 9:00 PM.", Audience = "visitors", Published = "Today" }
};

var events = new[]
{
    new { Id = 1, Name = "Community BBQ", When = "Nov 12, 6:00 PM", Location = "Garden Courtyard", Attendance = "48 confirmed" },
    new { Id = 2, Name = "Wellness Workshop", When = "Nov 14, 4:30 PM", Location = "Club Room", Attendance = "22 confirmed" },
    new { Id = 3, Name = "Resident Town Hall", When = "Nov 18, 7:00 PM", Location = "Lobby Lounge", Attendance = "64 confirmed" }
};

app.MapGet("/api/health", () => Results.Json(new { status = "ok", service = "hearthline-web" }));
app.MapGet("/api/dashboard", () => Results.Json(new { residents = residents.Length, apartments = apartments.Length, openMaintenance = maintenance.Count(x => x.Status != "Resolved"), events = events.Length, announcements = announcements.Length, activeGuests = guests.Length }));
app.MapGet("/api/residents", () => Results.Json(residents));
app.MapGet("/api/apartments", () => Results.Json(apartments));
app.MapGet("/api/maintenance", () => Results.Json(maintenance));
app.MapGet("/api/guests", () => Results.Json(guests));
app.MapGet("/api/announcements", () => Results.Json(announcements));
app.MapGet("/api/events", () => Results.Json(events));

app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
