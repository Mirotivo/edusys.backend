using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class BookingRequestedEvent
    {
        public string TutorId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string StudentName { get; set; } = string.Empty;
    }

    public class BookingRequestedHandler : IEventHandler<BookingRequestedEvent>
    {
        private readonly INotificationService _notificationService;

        public BookingRequestedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(BookingRequestedEvent eventData)
        {
            var emailSubject = "New Lesson Proposal Received";
            var emailBody = $@"
            <p>Dear Tutor,</p>
            <p><strong>{eventData.StudentName}</strong> has proposed a new lesson.</p>
            <p><strong>Lesson Details:</strong></p>
            <ul>
                <li><strong>Date:</strong> {eventData.Date:MMMM dd, yyyy} at {eventData.Date:HH:mm}</li>
                <li><strong>Duration:</strong> {eventData.Duration} minutes</li>
                <li><strong>Price:</strong> ${eventData.Price}</li>
            </ul>
            <p>Click <a href='https://www.avancira.com/messages'>here</a> to review and accept.</p>
            <p>Best regards,<br>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.TutorId,
                NotificationEvent.BookingRequested,
                $"{eventData.StudentName} has proposed a new lesson on {eventData.Date:MMMM dd, yyyy} at {eventData.Date:HH:mm} for {eventData.Duration} minutes.",
                new BookingRequested
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    LessonId = eventData.LessonId,
                    Date = eventData.Date,
                    Duration = eventData.Duration,
                    Price = eventData.Price
                }
            );
        }
    }
}

