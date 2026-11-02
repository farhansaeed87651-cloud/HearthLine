using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hearthline.Services;

namespace Hearthline;

public partial class MainWindow : Window
{
    private readonly BuildingApiClient _api = new();
    private readonly List<Resident> _residents = [];
    private readonly List<Apartment> _apartments = [];
    private readonly List<MaintenanceRequest> _maintenance = [];
    private readonly List<GuestPass> _guests = [];
    private readonly List<Announcement> _announcements = [];
    private readonly List<BuildingEvent> _events = [];
    private bool _residentMode;
    private bool _isAuthenticated;
    private string _currentPage = "Overview";
    private Resident _currentResident = new();

    public MainWindow()
    {
        InitializeComponent();
        SeedDemoData();
        _currentResident = _residents[5];
        StatusText.Text = _api.IsConfigured ? "Sign in to load building data" : "Demo mode - local data";
        RoleButton.Content = _api.IsConfigured ? "Sign in" : "Resident demo";
        Loaded += async (_, _) => await LoadServiceSnapshotAsync();
        Navigate("Overview");
    }

    private void SeedDemoData()
    {
        string[] firstNames = ["Ava", "Noah", "Mia", "Ethan", "Zoe", "Leo", "Nora", "Owen", "Iris", "Theo", "Lena", "Miles", "Ruby", "Jude", "Clara"];
        string[] lastNames = ["Carter", "Bennett", "Brooks", "Rivera", "Foster", "Hayes", "Morgan", "Ellis", "Reed", "Parker"];
        for (var index = 0; index < 150; index++)
        {
            var floor = index / 15 + 1;
            var door = index % 15 + 1;
            var apartmentNumber = $"{floor}{door:00}";
            var name = $"{firstNames[index % firstNames.Length]} {lastNames[index / firstNames.Length]}";
            var resident = new Resident
            {
                Name = name,
                Apartment = apartmentNumber,
                Email = $"{name.ToLowerInvariant().Replace(' ', '.')}@example.com",
                Phone = $"(555) 01{index / 100}{index % 100:00}",
                Lease = index % 4 == 0 ? "Renews in 60 days" : "Through Aug 2027",
                Status = index % 37 == 0 ? "On leave" : "Active"
            };
            _residents.Add(resident);
            _apartments.Add(new Apartment
            {
                Number = apartmentNumber,
                Resident = resident.Name,
                Shower = index % 29 == 0 ? "Check needed" : "Good",
                AirConditioning = index % 23 == 0 ? "Service due" : "Good",
                Heating = index % 31 == 0 ? "Check needed" : "Good",
                Appliances = index % 19 == 0 ? "Service due" : "Good"
            });
        }

        _maintenance.AddRange([
            new() { Id = "MH-0248", Apartment = "604", Issue = "AC not cooling consistently", Category = "HVAC", Priority = "High", Status = "In progress", Reported = "Today, 9:14 AM" },
            new() { Id = "MH-0247", Apartment = "203", Issue = "Slow drain in shower", Category = "Plumbing", Priority = "Normal", Status = "Open", Reported = "Today, 8:42 AM" },
            new() { Id = "MH-0246", Apartment = "1102", Issue = "Kitchen cabinet hinge loose", Category = "Appliance", Priority = "Low", Status = "Scheduled", Reported = "Yesterday" },
            new() { Id = "MH-0245", Apartment = "408", Issue = "Hallway light flickering", Category = "Electrical", Priority = "Normal", Status = "Open", Reported = "Yesterday" },
            new() { Id = "MH-0244", Apartment = "902", Issue = "Water pressure low", Category = "Plumbing", Priority = "High", Status = "In progress", Reported = "Oct 31" },
            new() { Id = "MH-0243", Apartment = "305", Issue = "Bedroom window latch", Category = "General", Priority = "Low", Status = "Resolved", Reported = "Oct 30" }
        ]);
        _guests.AddRange([
            new() { Guest = "Maya Chen", Host = _residents[5].Name, Apartment = _residents[5].Apartment, Arrival = "Today, 6:30 PM" },
            new() { Guest = "Andre Lewis", Host = "Nora Morgan", Apartment = "207", Arrival = "Today, 7:00 PM" },
            new() { Guest = "Sofia Patel", Host = "Ethan Bennett", Apartment = "102", Arrival = "Today, 8:15 PM" },
            new() { Guest = "Sam Walker", Host = "Iris Reed", Apartment = "909", Arrival = "Tomorrow, 11:00 AM" }
        ]);
        _announcements.AddRange([
            new() { Headline = "Water shutoff - East riser", Message = "Water service will be paused on floors 6-10 from 10 AM to noon on Thursday for scheduled valve maintenance.", Published = "Today, 8:05 AM" },
            new() { Headline = "Package room hours", Message = "The package room will close at 6 PM this Friday. Use the after-hours lockers for late deliveries.", Published = "Yesterday" },
            new() { Headline = "Lobby furniture refresh", Message = "New seating arrives next week. Please use the north entrance while the lobby is being updated.", Published = "Oct 29" }
        ]);
        _events.AddRange([
            new() { Name = "Courtyard supper", When = "Nov 06, 6:00 PM", Location = "South courtyard", Attendance = "38 going" },
            new() { Name = "Board games and tea", When = "Nov 12, 7:00 PM", Location = "Residents lounge", Attendance = "21 going" },
            new() { Name = "Winter coat collection", When = "Nov 18, 9:00 AM", Location = "Lobby", Attendance = "Community" }
        ]);
    }

    private void Navigation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string page })
            Navigate(page);
    }

    private void Navigate(string page)
    {
        _currentPage = page;
        PageTitle.Text = page;
        PageSubtitle.Text = page switch
        {
            "Overview" => _residentMode ? "Your home and community, in one place." : "A clear view of today's building operations.",
            "Residents" => "Resident directory and lease details.",
            "Apartments" => "Track in-unit systems and service condition.",
            "Maintenance" => _residentMode ? "Follow your reported issues and request service." : "Triage, schedule, and close building work orders.",
            "Guest passes" => _residentMode ? "Invite visitors to your apartment." : "Expected visitors and resident invitations.",
            "Announcements" => "Building updates and resident notices.",
            "Events" => "Gatherings and activities at Harbor House.",
            _ => ""
        };
        NavResidents.Visibility = _residentMode ? Visibility.Collapsed : Visibility.Visible;
        NavApartments.Visibility = _residentMode ? Visibility.Collapsed : Visibility.Visible;
        PageHost.Content = page switch
        {
            "Overview" => BuildOverview(),
            "Residents" => BuildResidents(),
            "Apartments" => BuildApartments(),
            "Maintenance" => BuildMaintenance(),
            "Guest passes" => BuildGuests(),
            "Announcements" => BuildAnnouncements(),
            "Events" => BuildEvents(),
            _ => BuildOverview()
        };
    }

    private async void RoleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_api.IsConfigured)
        {
            if (_isAuthenticated)
                SignOut();
            else
                await SignInAsync();
            return;
        }

        _residentMode = !_residentMode;
        RoleButton.Content = _residentMode ? "Exit resident demo" : "Resident demo";
        if (_residentMode && _currentPage is "Residents" or "Apartments")
            _currentPage = "Overview";
        Navigate(_currentPage);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PageHost is not null && !string.IsNullOrEmpty(_currentPage))
            Navigate(_currentPage);
    }

    private UIElement BuildOverview()
    {
        var stack = PageStack();
        stack.Children.Add(WelcomeBlock(_residentMode ? $"Welcome home, {_currentResident.Name.Split(' ')[0]}" : "Good morning, Building team", _residentMode ? $"Apartment {_currentResident.Apartment}  |  Harbor House" : "Tuesday, November 3  |  Harbor House"));
        var metrics = new UniformGrid { Columns = 4, Margin = new Thickness(0, 20, 0, 18) };
        if (_residentMode)
        {
            metrics.Children.Add(StatCard("2", "MY OPEN REQUESTS", "Repairs being handled", "#DDF0E4"));
            metrics.Children.Add(StatCard("1", "GUEST TODAY", "Maya Chen - 6:30 PM", "#FCE8DE"));
            metrics.Children.Add(StatCard("2", "UPCOMING EVENTS", "Next: Courtyard supper", "#E7ECF7"));
            metrics.Children.Add(StatCard("3", "NEW UPDATES", "Latest building notices", "#F5EDD4"));
        }
        else
        {
            metrics.Children.Add(StatCard("150", "RESIDENTS", "Across 150 apartments", "#DDF0E4"));
            var openCount = _maintenance.Count(item => item.Status is "Open" or "In progress" or "Scheduled");
            var guestCount = _guests.Count(guest => guest.Arrival.Contains("Today", StringComparison.OrdinalIgnoreCase));
            metrics.Children.Add(StatCard(openCount.ToString(), "OPEN WORK ORDERS", $"{_maintenance.Count(item => item.Priority == "High")} high priority", "#FCE8DE"));
            metrics.Children.Add(StatCard(guestCount.ToString(), "GUESTS TODAY", "Scheduled building visits", "#E7ECF7"));
            metrics.Children.Add(StatCard("100%", "OCCUPANCY", "150 apartments in service", "#F5EDD4"));
        }
        stack.Children.Add(metrics);
        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.85, GridUnitType.Star) });
        var work = Card("Maintenance needing attention", _residentMode ? "YOUR SERVICE REQUESTS" : "RECENT WORK ORDERS");
        var rows = _residentMode ? _maintenance.Where(item => item.Apartment == _currentResident.Apartment).ToList() : _maintenance.Take(4).ToList();
        ((StackPanel)work.Tag!).Children.Add(SimpleList(rows.Select(item => ($"{item.Issue}", $"Apt {item.Apartment}  |  {item.Id}", item.Status)).ToList()));
        Grid.SetColumn(work, 0);
        columns.Children.Add(work);
        var community = Card(_residentMode ? "Coming up at Harbor House" : "At Harbor House", "COMMUNITY & UPDATES");
        var firstEvent = _events.FirstOrDefault() ?? new BuildingEvent { Name = "No upcoming events", When = "Add an event to get started" };
        var communityItems = new List<(string Title, string Detail, string Tag)>
        {
            (firstEvent.Name, $"{firstEvent.When}  |  {firstEvent.Location}", "EVENT")
        };
        if (_announcements.FirstOrDefault() is { } latestAnnouncement)
            communityItems.Add((latestAnnouncement.Headline, $"Published {latestAnnouncement.Published}", "NOTICE"));
        if (_events.Skip(1).FirstOrDefault() is { } nextEvent)
            communityItems.Add((nextEvent.Name, $"{nextEvent.When}  |  {nextEvent.Location}", "EVENT"));
        ((StackPanel)community.Tag!).Children.Add(SimpleList(communityItems));
        Grid.SetColumn(community, 1);
        columns.Children.Add(community);
        stack.Children.Add(columns);
        var actions = Card("Quick actions", "SHORTCUTS");
        var actionRow = new WrapPanel();
        actionRow.Children.Add(ActionButton("Report a problem", () => { Navigate("Maintenance"); if (_residentMode) AddMaintenance(); }));
        actionRow.Children.Add(ActionButton("Invite a guest", () => { Navigate("Guest passes"); AddGuest(); }));
        if (!_residentMode)
        {
            actionRow.Children.Add(ActionButton("Post announcement", () => { Navigate("Announcements"); AddAnnouncement(); }));
            actionRow.Children.Add(ActionButton("Create event", () => { Navigate("Events"); AddEvent(); }));
        }
        ((StackPanel)actions.Tag!).Children.Add(actionRow);
        stack.Children.Add(actions);
        return stack;
    }

    private UIElement BuildResidents()
    {
        var stack = PageStack();
        stack.Children.Add(PageToolbar("150 resident records", "Resident directory", null));
        var results = _residents.Where(resident => Matches(SearchBox.Text, resident.Name, resident.Apartment, resident.Email, resident.Status)).ToList();
        stack.Children.Add(DataTable(results, 430));
        return stack;
    }

    private UIElement BuildApartments()
    {
        var stack = PageStack();
        stack.Children.Add(PageToolbar("150 apartments - shower, AC, heating, appliances", "Apartment systems", null));
        var rows = _apartments.Where(apt => Matches(SearchBox.Text, apt.Number, apt.Resident, apt.Shower, apt.AirConditioning, apt.Heating, apt.Appliances)).ToList();
        stack.Children.Add(DataTable(rows, 430));
        return stack;
    }

    private UIElement BuildMaintenance()
    {
        var stack = PageStack();
        var rows = _residentMode ? _maintenance.Where(item => item.Apartment == _currentResident.Apartment).ToList() : _maintenance;
        rows = rows.Where(item => Matches(SearchBox.Text, item.Id, item.Apartment, item.Issue, item.Category, item.Priority, item.Status)).ToList();
        stack.Children.Add(PageToolbar($"{rows.Count} visible work orders", "Maintenance queue", ActionButton("+  Report an issue", AddMaintenance)));
        stack.Children.Add(DataTable(rows, 430));
        return stack;
    }

    private UIElement BuildGuests()
    {
        var stack = PageStack();
        var rows = _residentMode ? _guests.Where(guest => guest.Apartment == _currentResident.Apartment).ToList() : _guests;
        rows = rows.Where(guest => Matches(SearchBox.Text, guest.Guest, guest.Host, guest.Apartment, guest.Arrival, guest.PassStatus)).ToList();
        stack.Children.Add(PageToolbar($"{rows.Count} invitations listed", "Guest passes", ActionButton("+  Invite a guest", AddGuest)));
        stack.Children.Add(DataTable(rows, 430));
        return stack;
    }

    private UIElement BuildAnnouncements()
    {
        var stack = PageStack();
        stack.Children.Add(PageToolbar($"{_announcements.Count} published notices", "Resident announcements", _residentMode ? null : ActionButton("+  New announcement", AddAnnouncement)));
        foreach (var announcement in _announcements.Where(item => Matches(SearchBox.Text, item.Headline, item.Message, item.Published)))
        {
            var card = Card(announcement.Headline, $"{announcement.Audience.ToUpperInvariant()}  |  {announcement.Published.ToUpperInvariant()}");
            ((StackPanel)card.Tag!).Children.Add(new TextBlock { Text = announcement.Message, FontSize = 13, Foreground = Brush("#46564E"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 2), LineHeight = 20 });
            stack.Children.Add(card);
        }
        return stack;
    }

    private UIElement BuildEvents()
    {
        var stack = PageStack();
        stack.Children.Add(PageToolbar($"{_events.Count} upcoming activities", "Building events", _residentMode ? null : ActionButton("+  Create event", AddEvent)));
        stack.Children.Add(DataTable(_events.Where(item => Matches(SearchBox.Text, item.Name, item.When, item.Location, item.Attendance)).ToList(), 350));
        return stack;
    }

    private StackPanel PageStack() => new() { Margin = new Thickness(28, 22, 28, 32) };

    private UIElement WelcomeBlock(string title, string subtitle)
    {
        var block = new StackPanel();
        block.Children.Add(new TextBlock { Text = title, FontSize = 25, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F") });
        block.Children.Add(new TextBlock { Text = subtitle, FontSize = 12, Foreground = Brush("#6B7972"), Margin = new Thickness(0, 6, 0, 0) });
        return block;
    }

    private Border StatCard(string value, string label, string detail, string tint)
    {
        var card = new Border { Background = Brushes.White, BorderBrush = Brush("#E2E9E3"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(16), Margin = new Thickness(0, 0, 12, 0) };
        var content = new StackPanel();
        content.Children.Add(new TextBlock { Text = label, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brush("#6B7972") });
        content.Children.Add(new TextBlock { Text = value, FontSize = 27, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F"), Margin = new Thickness(0, 8, 0, 2) });
        content.Children.Add(new TextBlock { Text = detail, FontSize = 10, Foreground = Brush("#6B7972"), TextTrimming = TextTrimming.CharacterEllipsis });
        card.Child = content;
        return card;
    }

    private Border Card(string title, string eyebrow)
    {
        var card = new Border { Background = Brushes.White, BorderBrush = Brush("#E2E9E3"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(18), Margin = new Thickness(0, 0, 14, 14) };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = eyebrow, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brush("#79877F") });
        stack.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F"), Margin = new Thickness(0, 5, 0, 14) });
        card.Child = stack;
        card.Tag = stack;
        return card;
    }

    private UIElement SimpleList(List<(string Title, string Detail, string Tag)> items)
    {
        var stack = new StackPanel();
        foreach (var item in items)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 5) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var text = new StackPanel();
            text.Children.Add(new TextBlock { Text = item.Title, FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = Brush("#24322B"), TextTrimming = TextTrimming.CharacterEllipsis });
            text.Children.Add(new TextBlock { Text = item.Detail, FontSize = 10, Foreground = Brush("#79877F"), Margin = new Thickness(0, 3, 0, 0) });
            row.Children.Add(text);
            var badge = new Border { Background = item.Tag is "Open" or "In progress" ? Brush("#FCE8DE") : Brush("#E6F0EA"), CornerRadius = new CornerRadius(3), Padding = new Thickness(7, 4, 7, 4), VerticalAlignment = VerticalAlignment.Center, Child = new TextBlock { Text = item.Tag.ToUpperInvariant(), FontSize = 8, FontWeight = FontWeights.Bold, Foreground = Brush("#496158") } };
            Grid.SetColumn(badge, 1);
            row.Children.Add(badge);
            stack.Children.Add(row);
            stack.Children.Add(new Border { Height = 1, Background = Brush("#EDF1ED"), Margin = new Thickness(0, 7, 0, 7) });
        }
        return stack;
    }

    private UIElement PageToolbar(string detail, string label, UIElement? action)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var left = new StackPanel();
        left.Children.Add(new TextBlock { Text = label, FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F") });
        left.Children.Add(new TextBlock { Text = detail, FontSize = 11, Foreground = Brush("#6B7972"), Margin = new Thickness(0, 4, 0, 0) });
        grid.Children.Add(left);
        if (action is not null)
        {
            Grid.SetColumn(action, 1);
            if (action is FrameworkElement element)
                element.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(action);
        }
        return grid;
    }

    private DataGrid DataTable<T>(List<T> rows, double height)
    {
        var table = new DataGrid
        {
            ItemsSource = rows,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserResizeRows = false,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Background = Brushes.White,
            RowBackground = Brushes.White,
            AlternatingRowBackground = Brush("#F7F9F7"),
            BorderBrush = Brush("#E2E9E3"),
            BorderThickness = new Thickness(1),
            HorizontalGridLinesBrush = Brush("#E9EEEA"),
            VerticalGridLinesBrush = Brushes.Transparent,
            MinHeight = height,
            MaxHeight = 560,
            FontSize = 11,
            RowHeight = 38
        };
        return table;
    }

    private Button ActionButton(string text, Action action)
    {
        var button = new Button { Content = text, Background = Brush("#214D42"), Foreground = Brushes.White, BorderThickness = new Thickness(0), Padding = new Thickness(14, 9, 14, 9), FontSize = 11, FontWeight = FontWeights.SemiBold, Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 8, 0) };
        button.Click += (_, _) => action();
        return button;
    }

    private async void AddMaintenance()
    {
        if (!CanWrite()) return;
        var fields = _residentMode ? PromptFields("Report a maintenance issue", "What needs attention?") : PromptFields("Report a maintenance issue", "Apartment number", "What needs attention?");
        if (fields is null || fields[^1].Length == 0) return;
        var apartment = _residentMode ? _currentResident.Apartment : fields[0];
        if (string.IsNullOrWhiteSpace(apartment)) return;
        var issue = fields[^1];
        var request = new MaintenanceRequest { Id = $"MH-{248 + _maintenance.Count + 1}", Apartment = apartment, Issue = issue, Category = "General", Priority = "Normal", Status = "Open", Reported = "Just now" };
        await PostToServiceAsync("api/maintenance", request);
        _maintenance.Insert(0, request);
        Navigate("Maintenance");
    }

    private async void AddGuest()
    {
        if (!CanWrite()) return;
        var fields = _residentMode ? PromptFields("Invite a guest", "Guest full name", "Expected arrival") : PromptFields("Invite a guest", "Guest full name", "Host apartment number", "Expected arrival");
        if (fields is null || string.IsNullOrWhiteSpace(fields[0])) return;
        var host = _residentMode ? _currentResident : _residents[5];
        if (!_residentMode)
            host = _residents.FirstOrDefault(resident => resident.Apartment == fields[1]) ?? host;
        var arrival = fields[^1];
        var pass = new GuestPass { Guest = fields[0], Host = host.Name, Apartment = host.Apartment, Arrival = string.IsNullOrWhiteSpace(arrival) ? "Time not set" : arrival, PassStatus = "Expected" };
        await PostToServiceAsync("api/guest-passes", pass);
        _guests.Insert(0, pass);
        Navigate("Guest passes");
    }

    private async void AddAnnouncement()
    {
        if (_residentMode || !CanWrite()) return;
        var fields = PromptFields("New announcement", "Announcement headline", "Message for residents");
        if (fields is null || string.IsNullOrWhiteSpace(fields[0]) || string.IsNullOrWhiteSpace(fields[1])) return;
        var announcement = new Announcement { Headline = fields[0], Message = fields[1], Published = "Just now" };
        await PostToServiceAsync("api/announcements", announcement);
        _announcements.Insert(0, announcement);
        Navigate("Announcements");
    }

    private async void AddEvent()
    {
        if (_residentMode || !CanWrite()) return;
        var fields = PromptFields("Create a building event", "Event name", "Date and time", "Location");
        if (fields is null || string.IsNullOrWhiteSpace(fields[0])) return;
        var item = new BuildingEvent { Name = fields[0], When = fields[1], Location = fields[2], Attendance = "Open" };
        await PostToServiceAsync("api/events", item);
        _events.Insert(0, item);
        Navigate("Events");
    }

    private string? Prompt(string title, string fieldLabel)
    {
        return PromptFields(title, fieldLabel)?[0];
    }

    private string[]? PromptFields(string title, params string[] fieldLabels)
    {
        var dialog = new Window { Title = title, Owner = this, Width = 440, Height = 158 + fieldLabels.Length * 74, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = Brush("#F3F6F2") };
        var panel = new StackPanel { Margin = new Thickness(22) };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F") });
        var inputs = new List<TextBox>();
        foreach (var fieldLabel in fieldLabels)
        {
            panel.Children.Add(new TextBlock { Text = fieldLabel, FontSize = 11, Foreground = Brush("#6B7972"), Margin = new Thickness(0, 12, 0, 5) });
            var input = new TextBox { Height = fieldLabel == "Message for residents" ? 70 : 34, Padding = new Thickness(9, 7, 9, 5), FontSize = 12, TextWrapping = fieldLabel == "Message for residents" ? TextWrapping.Wrap : TextWrapping.NoWrap, AcceptsReturn = fieldLabel == "Message for residents" };
            inputs.Add(input);
            panel.Children.Add(input);
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) };
        var cancel = new Button { Content = "Cancel", Padding = new Thickness(13, 7, 13, 7), Margin = new Thickness(0, 0, 8, 0), IsCancel = true };
        var save = new Button { Content = "Continue", Padding = new Thickness(13, 7, 13, 7), Background = Brush("#214D42"), Foreground = Brushes.White, IsDefault = true };
        save.Click += (_, _) => dialog.DialogResult = true;
        buttons.Children.Add(cancel);
        buttons.Children.Add(save);
        panel.Children.Add(buttons);
        dialog.Content = panel;
        return dialog.ShowDialog() == true ? inputs.Select(input => input.Text.Trim()).ToArray() : null;
    }

    private async Task LoadServiceSnapshotAsync()
    {
        if (!_api.IsConfigured || !_isAuthenticated) return;
        try
        {
            var snapshot = await _api.GetJsonAsync<BuildingSnapshot>("api/building/snapshot");
            Replace(_residents, snapshot.Residents);
            Replace(_apartments, snapshot.Apartments);
            Replace(_maintenance, snapshot.Maintenance);
            Replace(_guests, snapshot.Guests);
            Replace(_announcements, snapshot.Announcements);
            Replace(_events, snapshot.Events);
            if (_residentMode)
                _currentResident = _residents.FirstOrDefault(resident => resident.Apartment == _currentResident.Apartment) ?? _currentResident;
            StatusText.Text = "Internet service connected";
            Navigate(_currentPage);
        }
        catch (Exception exception)
        {
            StatusText.Text = "Service unavailable - demo data";
            MessageBox.Show(this, $"Could not load building data. Demo data remains available.\n\n{exception.Message}", "Service unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task PostToServiceAsync(string path, object payload)
    {
        if (!_api.IsConfigured) return;
        try
        {
            await _api.PostAsync(path, payload);
            StatusText.Text = "Internet service connected";
        }
        catch (Exception exception)
        {
            StatusText.Text = "Service unavailable - local changes only";
            MessageBox.Show(this, $"The change is visible in this session but was not sent to the building service.\n\n{exception.Message}", "Could not sync change", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task SignInAsync()
    {
        var credentials = PromptCredentials();
        if (credentials is null) return;
        try
        {
            using var response = await _api.SignInAsync(credentials.Value.Email, credentials.Value.Password);
            var root = response.RootElement;
            var role = root.GetProperty("role").GetString()?.ToLowerInvariant();
            if (role is not "resident" and not "admin")
                throw new InvalidOperationException("The building service returned an unsupported account role.");
            var token = root.GetProperty("accessToken").GetString();
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("The building service did not return an access token.");
            _api.SetAccessToken(token);
            _isAuthenticated = true;
            _residentMode = role == "resident";
            if (_residentMode)
            {
                var residentData = root.GetProperty("resident");
                var apartment = residentData.GetProperty("apartment").GetString() ?? "";
                _currentResident = _residents.FirstOrDefault(resident => resident.Apartment == apartment) ?? new Resident
                {
                    Name = residentData.GetProperty("name").GetString() ?? "Resident",
                    Apartment = apartment,
                    Email = credentials.Value.Email
                };
            }
            RoleButton.Content = "Sign out";
            StatusText.Text = _residentMode ? $"Resident - apartment {_currentResident.Apartment}" : "Administrator session";
            await LoadServiceSnapshotAsync();
            Navigate("Overview");
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Sign-in failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void SignOut()
    {
        _api.ClearAccessToken();
        _isAuthenticated = false;
        _residentMode = false;
        _residents.Clear();
        _apartments.Clear();
        _maintenance.Clear();
        _guests.Clear();
        _announcements.Clear();
        _events.Clear();
        SeedDemoData();
        _currentResident = _residents[5];
        RoleButton.Content = "Sign in";
        StatusText.Text = "Signed out - demo data";
        Navigate("Overview");
    }

    private bool CanWrite()
    {
        if (!_api.IsConfigured || _isAuthenticated) return true;
        MessageBox.Show(this, "Sign in before creating or changing building records.", "Sign-in required", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private (string Email, string Password)? PromptCredentials()
    {
        var dialog = new Window { Title = "Resident sign in", Owner = this, Width = 420, Height = 290, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = Brush("#F3F6F2") };
        var panel = new StackPanel { Margin = new Thickness(22) };
        panel.Children.Add(new TextBlock { Text = "Resident sign in", FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = Brush("#18231F") });
        panel.Children.Add(new TextBlock { Text = "Email address", FontSize = 11, Foreground = Brush("#6B7972"), Margin = new Thickness(0, 15, 0, 5) });
        var email = new TextBox { Height = 34, Padding = new Thickness(9, 7, 9, 5), FontSize = 12 };
        panel.Children.Add(email);
        panel.Children.Add(new TextBlock { Text = "Password", FontSize = 11, Foreground = Brush("#6B7972"), Margin = new Thickness(0, 12, 0, 5) });
        var password = new PasswordBox { Height = 34, Padding = new Thickness(9, 7, 9, 5), FontSize = 12 };
        panel.Children.Add(password);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 17, 0, 0) };
        buttons.Children.Add(new Button { Content = "Cancel", Padding = new Thickness(13, 7, 13, 7), Margin = new Thickness(0, 0, 8, 0), IsCancel = true });
        var signIn = new Button { Content = "Sign in", Padding = new Thickness(13, 7, 13, 7), Background = Brush("#214D42"), Foreground = Brushes.White, IsDefault = true };
        signIn.Click += (_, _) => dialog.DialogResult = true;
        buttons.Children.Add(signIn);
        panel.Children.Add(buttons);
        dialog.Content = panel;
        return dialog.ShowDialog() == true ? (email.Text.Trim(), password.Password) : null;
    }

    private static void Replace<T>(List<T> target, IEnumerable<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private async void CheckConnection_Click(object sender, RoutedEventArgs e)
    {
        if (!_api.IsConfigured)
        {
            MessageBox.Show(this, "Set HEARTHLINE_API_BASE_URL to your building service URL before connecting.", "Service not configured", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        StatusText.Text = "Checking service...";
        try
        {
            await _api.CheckConnectionAsync();
            StatusText.Text = "Internet service connected";
            MessageBox.Show(this, "The building service responded successfully.", "Connection confirmed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            StatusText.Text = "Service unavailable";
            MessageBox.Show(this, exception.Message, "Connection failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool Matches(string query, params string[] values) => string.IsNullOrWhiteSpace(query) || values.Any(value => value.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase));
    private static SolidColorBrush Brush(string color) => new((Color)ColorConverter.ConvertFromString(color));
}