using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Services.Schedules
{
    internal static class ScheduleIntervalHelper
    {
        public static IReadOnlyList<ScheduleInterval> Merge(
            IEnumerable<ScheduleEntity> schedules)
        {
            var intervals = new List<ScheduleInterval>();

            foreach (ScheduleEntity schedule in schedules
                .Where(schedule => schedule.IsAvailable)
                .OrderBy(schedule => schedule.StartTime))
            {
                if (intervals.Count == 0)
                {
                    intervals.Add(new ScheduleInterval(
                        schedule.StartTime,
                        schedule.EndTime));
                    continue;
                }

                ScheduleInterval previous = intervals[^1];

                if (schedule.StartTime > previous.EndTime)
                {
                    intervals.Add(new ScheduleInterval(
                        schedule.StartTime,
                        schedule.EndTime));
                    continue;
                }

                if (schedule.EndTime > previous.EndTime)
                {
                    intervals[^1] = previous with
                    {
                        EndTime = schedule.EndTime
                    };
                }
            }

            return intervals;
        }
    }

    internal readonly record struct ScheduleInterval(
        TimeOnly StartTime,
        TimeOnly EndTime);
}
