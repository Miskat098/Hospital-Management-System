namespace Hospital_Management_System.Models
{
    public enum AppointmentStatus
    {
        Upcoming,
        InProgress,
        Completed,
        Cancelled
    }

    public enum InvoiceStatus
    {
        Pending,
        Paid,
        Overdue
    }

    /// <summary>
    /// Mirrors the role strings used throughout the frontend
    /// (role-select.html, role-tag CSS classes, api.js mock users) so
    /// DTOs can serialize the exact same string values the UI expects.
    /// </summary>
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Patient = "Patient";
        public const string Staff = "Staff";

        public static readonly string[] All = { Admin, Doctor, Patient, Staff };
    }
}