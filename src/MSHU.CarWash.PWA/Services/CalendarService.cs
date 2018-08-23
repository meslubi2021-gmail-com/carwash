﻿using System;
using Microsoft.Graph;
using MSHU.CarWash.ClassLibrary;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace MSHU.CarWash.PWA.Services
{
    /// <inheritdoc />
    public class CalendarService : ICalendarService
    {
        private readonly IGraphServiceClient _graphClient;
        private readonly TelemetryClient _telemetryClient;

        /// <inheritdoc />
        public CalendarService(IGraphService graphService)
        {
            _graphClient = graphService.GetAuthenticatedClient();

            _telemetryClient = new TelemetryClient();
        }

        /// <inheritdoc />
        public async Task<string> CreateEventAsync(Reservation reservation)
        {
            var calendarEvent = GetCalendarEventFromReservation(reservation);

            try
            {
                calendarEvent = await _graphClient.Me
                    .Events
                    .Request()
                    .AddAsync(calendarEvent);
            }
            catch (Exception e)
            {
                _telemetryClient.TrackException(e);
            }

            return calendarEvent.Id;
        }

        /// <inheritdoc />
        public async Task<string> UpdateEventAsync(Reservation reservation)
        {
            if (reservation.OutlookEventId == null) return await CreateEventAsync(reservation);

            var calendarEvent = GetCalendarEventFromReservation(reservation);

            try
            {
                calendarEvent = await _graphClient.Me
                    .Events[reservation.OutlookEventId]
                    .Request()
                    .UpdateAsync(calendarEvent);
            }
            catch (Exception e)
            {
                _telemetryClient.TrackException(e);
            }

            return calendarEvent.Id;
        }

        /// <inheritdoc />
        public async Task DeleteEventAsync(string outlookEventId)
        {
            if (outlookEventId == null) return;

            try
            {
                await _graphClient.Me
                    .Events[outlookEventId]
                    .Request()
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                _telemetryClient.TrackException(e);
            }
        }

        private static Event GetCalendarEventFromReservation(Reservation reservation)
        {
            return new Event
            {
                Subject = $"Car wash ({reservation.VehiclePlateNumber})",
                IsReminderOn = true,
                ReminderMinutesBeforeStart = 30,
                ShowAs = FreeBusyStatus.Free,
                Importance = Importance.Low,
                Type = EventType.SingleInstance,
                Start = new DateTimeTimeZone
                {
                    DateTime = reservation.DateFrom.ToString(),
                    TimeZone = "Europe/Budapest"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = reservation.DateFrom.ToString(),
                    TimeZone = "Europe/Budapest"
                },
                Location = new Location
                {
                    DisplayName = reservation.Location
                },
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = $"Hi {reservation.User.FirstName}!<br/>Please don't forget to leave the key at the reception and <a href=\"https://carwashu.azurewebsites.net\">confirm dropoff & vihacle location by clicking here</a>!"
                }
            };
        }
    }
}

