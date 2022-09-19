using AutoMapper;
using IceSync.Domain.Dtos;
using IceSync.Domain.Entities;
using IceSync.Domain.Interfaces;
using IceSync.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using static IceSync.Domain.Utils.Constants;

namespace IceSync.Infrastructure.Services;

public class UniversalLoaderSyncService : IUniversalLoaderSyncService
{
    private readonly IMapper _mapper;
    private readonly ILogger<UniversalLoaderSyncService> _logger;
    private readonly IWorkflowReporitory _workflowReporitory;
    private readonly IUniversalLoaderService _universalLoaderService;

    public UniversalLoaderSyncService(
        IMapper mapper,
        ILogger<UniversalLoaderSyncService> logger,
        IWorkflowReporitory workflowReporitory,
        IUniversalLoaderService universalLoaderService)
    {
        _mapper = mapper;
        _logger = logger;
        _workflowReporitory = workflowReporitory;
        _universalLoaderService = universalLoaderService;
    }

    public async Task SyncData(CancellationToken cancellationToken)
    {
        try
        {
            var allWorkflowsAPI = await _universalLoaderService.GetWorkflows(cancellationToken).ConfigureAwait(false);
            var workflowIdToIdMapper = await _workflowReporitory.WorkflowIdToIdMapper(cancellationToken).ConfigureAwait(false);
            var allWorkflowsIdsDB = workflowIdToIdMapper.Keys.ToList();

            var allWorkflowsAPIDictionary = allWorkflowsAPI.ToDictionary(x => x.Id);
            var allWorkflowsIdsAPI = allWorkflowsAPIDictionary.Keys;

            await DeleteAllMissingEntities(allWorkflowsIdsAPI, allWorkflowsIdsDB, cancellationToken).ConfigureAwait(false);
            await AddAllNewEntities(allWorkflowsAPI, allWorkflowsIdsDB, cancellationToken).ConfigureAwait(false);
            await UpdateAllExistingEntities(allWorkflowsAPI, allWorkflowsIdsDB, workflowIdToIdMapper, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if(ex is OperationCanceledException canceledEx)
            {
                _logger.LogWarning(canceledEx, "Synchronisation interrupted.");
            }
            else
            {
                _logger.LogError(ex, "Synchronisation failed.");

                throw;
            }
        }
    }

    private async Task UpdateAllExistingEntities(IEnumerable<WorkflowDto> allWorkflowsAPI, List<int> allWorkflowsIdsDB, IDictionary<int, int> workflowIdToIdMapper, CancellationToken cancellationToken)
    {
        var batch = 0;
        var entitiesForUpdate = new List<Workflow>();

        while (batch * EFParameterLimit < allWorkflowsIdsDB.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchIdsForSearch = allWorkflowsIdsDB.Skip(batch * EFParameterLimit).Take(EFParameterLimit);
            var existingEntitiesDto = allWorkflowsAPI.Where(x => batchIdsForSearch.Any(id => x.Id == id));

            IEnumerable<Workflow> existingEntities = MapWorkflowDtoToEntity(existingEntitiesDto, workflowIdToIdMapper);
            entitiesForUpdate.AddRange(existingEntities);

            batch++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await _workflowReporitory.BulkUpdate(entitiesForUpdate, cancellationToken).ConfigureAwait(false);
        await _workflowReporitory.SaveChanges(cancellationToken).ConfigureAwait(false);
    }

    private async Task AddAllNewEntities(IEnumerable<WorkflowDto> allWorkflowsAPI, IEnumerable<int> allWorkflowsIdsDB, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var newEntities = new List<Workflow>();
        var newEntitiesIds = allWorkflowsAPI.Select(x => x.Id).Except(allWorkflowsIdsDB).ToList();

        var batch = 0;

        while (batch * EFParameterLimit < newEntitiesIds.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchIdsForSearch = newEntitiesIds.Skip(batch * EFParameterLimit).Take(EFParameterLimit);
            var newEntitiesDto = allWorkflowsAPI.Where(x => batchIdsForSearch.Any(id => x.Id == id));
            newEntities.AddRange(_mapper.Map<IEnumerable<Workflow>>(newEntitiesDto));

            batch++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await _workflowReporitory.BulkInsert(newEntities, cancellationToken).ConfigureAwait(false);
        await _workflowReporitory.SaveChanges(cancellationToken).ConfigureAwait(false);
    }

    private async Task DeleteAllMissingEntities(IEnumerable<int> allWorkflowsIdsAPI, List<int> allWorkflowsIdsDB, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var forDeletion = allWorkflowsIdsDB.Except(allWorkflowsIdsAPI).ToList();
        var batch = 0;

        while (batch * EFParameterLimit < forDeletion.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchIdsForDeletion = forDeletion.Skip(batch * EFParameterLimit).Take(EFParameterLimit);
            await _workflowReporitory.BulkDelete(x => batchIdsForDeletion.Contains(x.WorkflowId), cancellationToken).ConfigureAwait(false);

            allWorkflowsIdsDB.RemoveAll(x => batchIdsForDeletion.Contains(x));

            batch++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await _workflowReporitory.SaveChanges(cancellationToken).ConfigureAwait(false);
    }

    private IEnumerable<Workflow> MapWorkflowDtoToEntity(IEnumerable<WorkflowDto> existingEntitiesDto, IDictionary<int, int> workflowIdToIdMapper)
    {
        var existingEntities = _mapper.Map<IEnumerable<Workflow>>(existingEntitiesDto).ToList();

        if (existingEntities.Count < EFParameterLimit / 3)
        {
            foreach (var entity in existingEntities)
                entity.Id = workflowIdToIdMapper[entity.WorkflowId];
        }
        else
        {
            Parallel.ForEach(existingEntities, entity => { entity.Id = workflowIdToIdMapper[entity.WorkflowId]; });
        }

        return existingEntities;
    }

    private void CheckForCancellationRequested(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();
    }
}
