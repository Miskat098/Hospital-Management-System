using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches the feedback-history-item fields rendered on
    /// wwwroot/pages/patient/feedback.html.
    /// </summary>
    public class FeedbackDto
    {
        public int Id { get; set; }
        public string AppointmentId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    /// <summary>
    /// Matches the payload built by patient/feedback.js
    /// initFeedbackForm handler:
    /// { appointmentId, rating, comment, anonymous }
    /// </summary>
    public class SubmitFeedbackDto
    {
        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public bool Anonymous { get; set; }
    }
}