namespace EventHub.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Statistics
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRegistrations { get; set; }
        public int EventsThisMonth { get; set; }
        public int RegistrationsThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }

        // Recent Data
        public List<RecentEventDto> RecentEvents { get; set; } = new();
        public List<RecentRegistrationDto> RecentRegistrations { get; set; } = new();
        public List<PopularCategoryDto> PopularCategories { get; set; } = new();

        // Charts Data
        public ChartDataDto RegistrationsByMonth { get; set; } = new();
        public ChartDataDto EventsByCategory { get; set; } = new();
    }

    public class RecentEventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public int RegistrationsCount { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    public class RecentRegistrationDto
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime EventDateTime { get; set; }
    }

    public class PopularCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
    }

    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
    }
}