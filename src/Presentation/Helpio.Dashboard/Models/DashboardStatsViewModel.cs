namespace Helpio.Dashboard.Models
{
    public class DashboardStatsViewModel
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int TotalCustomers { get; set; }
        public int MyTickets { get; set; } // For agents
        public int MyClosedTickets { get; set; } // For agents
        public int ActiveTeams { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAgents { get; set; } // Added missing property
        
        // Additional metrics
        public double AvgResponseTime { get; set; } // in hours
        public int TodayTickets { get; set; }
        public int WeeklyTickets { get; set; }
        public int MonthlyTickets { get; set; }
        
        // Performance metrics
        public int UnassignedTickets { get; set; }
        public int OverdueTickets { get; set; }
        public int HighPriorityTickets { get; set; }
    }
}