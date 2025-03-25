using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class PropositionRespondedEvent
    {
        public string StudentId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public LessonStatus Status { get; set; }
        public string TutorName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string MeetingUrl { get; set; } = string.Empty;
    }

    public class PropositionRespondedHandler : IEventHandler<PropositionRespondedEvent>
    {
        private readonly INotificationService _notificationService;

        public PropositionRespondedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(PropositionRespondedEvent eventData)
        {
            var statusMessage = $"Your proposition for {eventData.LessonTitle} has been {(eventData.Status == LessonStatus.Booked ? "accepted" : "declined")} by {eventData.TutorName}.";

            // Define Email Subject & Body
            var emailSubject = eventData.Status == LessonStatus.Booked
                ? "Lesson Proposition Accepted"
                : "Lesson Proposition Declined";

            var emailBody = eventData.Status == LessonStatus.Booked
                ? $@"
            <p>Hi,</p>
            <p>Your lesson proposition for <strong>{eventData.LessonTitle}</strong> has been <strong>accepted</strong> by {eventData.TutorName}.</p>
            <p><strong>Lesson Details:</strong></p>
            <ul>
                <li><strong>Date:</strong> {eventData.Date:MMMM dd, yyyy} at {eventData.Date:HH:mm}</li>
                <li><strong>Duration:</strong> {eventData.Duration} minutes</li>
                <li><strong>Price:</strong> ${eventData.Price}</li>
            </ul>
            <p>Click <a href='{eventData.MeetingUrl}'>here</a> to join the lesson at the scheduled time.</p>
            <p>Best regards,<br>Avancira Team</p>"
                : $@"
            <p>Hi,</p>
            <p>Unfortunately, your lesson proposition for <strong>{eventData.LessonTitle}</strong> has been <strong>declined</strong> by {eventData.TutorName}.</p>
            <p>If you have any questions, feel free to reach out to the tutor.</p>
            <p>Best regards,<br>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.StudentId,
                NotificationEvent.PropositionResponded,
                statusMessage,
                new PropositionResponded
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    LessonId = eventData.LessonId,
                    Status = eventData.Status
                }
            );
        }
    }

}

