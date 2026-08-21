using DevOS.Application.TimeEntries.Commands.CreateTimeEntry;
using DevOS.Application.TimeEntries.Commands.UpdateTimeEntry;
using DevOS.Application.TimeEntries.Queries.GetTimeEntries;

namespace DevOS.Application.Tests.TimeEntries.Validation
{
    public class TimeEntryValidatorsTests
    {
        private readonly CreateTimeEntryValidator _createValidator = new();
        private readonly UpdateTimeEntryValidator _updateValidator = new();
        private readonly GetTimeEntriesValidator _getValidator = new();

        [Fact]
        public void CreateTimeEntryValidator_ValidCommand_ReturnsNoErrors()
        {
            var command = new CreateTimeEntryCommand
            {
                ProjectId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow.AddHours(-1),
                EndedAt = DateTime.UtcNow,
                Description = "Valid Description"
            };

            var errors = _createValidator.Validate(command);

            Assert.Empty(errors);
        }

        [Fact]
        public void CreateTimeEntryValidator_InvalidTimes_ReturnsError()
        {
            var command = new CreateTimeEntryCommand
            {
                ProjectId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddHours(-1)
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("StartedAt must be earlier than EndedAt"));
        }

        [Fact]
        public void CreateTimeEntryValidator_DescriptionExceedsLength_ReturnsError()
        {
            var command = new CreateTimeEntryCommand
            {
                ProjectId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow.AddHours(-1),
                EndedAt = DateTime.UtcNow,
                Description = new string('d', 2001)
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("Description cannot exceed 2000 characters"));
        }

        [Fact]
        public void UpdateTimeEntryValidator_ValidCommand_ReturnsNoErrors()
        {
            var command = new UpdateTimeEntryCommand
            {
                EntryId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow.AddHours(-1),
                EndedAt = DateTime.UtcNow,
                Description = "Valid Update"
            };

            var errors = _updateValidator.Validate(command);

            Assert.Empty(errors);
        }

        [Fact]
        public void GetTimeEntriesValidator_InvalidPeriod_ReturnsError()
        {
            var query = new GetTimeEntriesQuery
            {
                ProjectId = Guid.NewGuid(),
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(-1)
            };

            var errors = _getValidator.Validate(query);

            Assert.Contains(errors, e => e.Contains("'From' date must be earlier than 'To' date"));
        }
    }
}
