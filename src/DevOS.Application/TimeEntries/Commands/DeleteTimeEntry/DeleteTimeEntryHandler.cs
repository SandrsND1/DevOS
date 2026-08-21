using DevOS.Application.Exceptions;

namespace DevOS.Application.TimeEntries.Commands.DeleteTimeEntry
{
    public class DeleteTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;

        public DeleteTimeEntryHandler(ITimeEntryRepository timeEntryRepository)
        {
            _timeEntryRepository = timeEntryRepository;
        }

        public async Task HandleAsync(
            DeleteTimeEntryCommand command,
            CancellationToken cancellationToken = default)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(command.EntryId, command.ProjectId, cancellationToken);
            if (timeEntry is null)
                throw new TimeEntryNotFoundException(command.EntryId);

            await _timeEntryRepository.DeleteAsync(timeEntry, cancellationToken);
        }
    }
}
