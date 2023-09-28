using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RSPL.Contracts;
using RSPL.Domain.Entities;
using RSPL.Domain.Exceptions;
using Domain.Repositories;
using Mapster;
using RSPL.Services.Abstractions;
using RSPL.Contracts.Models.LabModels;
using RSPL.Domain;
using RSPL.Domain.Views;

namespace Services
{
    internal sealed class LabService : ILabService
    {
        private readonly IRepositoryManager _repositoryManager;

        public LabService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<LabViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Labs = await _repositoryManager.LabRepository.GetAllAsync(cancellationToken);

            var LabsData = Labs.Adapt<IEnumerable<LabViewModel>>();

            return LabsData;
        }
        public async Task<PaginationResponse<LabViewModel>> GetAllAsync
    (string searchTerm, PaginationRequest request,
    CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.LabRepository.GetAllAsync
                (searchTerm, request, cancellationToken);
            var LabData = response.Records.Adapt
                <IEnumerable<LabViewModel>>();

            return new PaginationResponse<LabViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = LabData
            };
        }
        public async Task<IEnumerable<LabViewModel>> SearchAllAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var Labs = await _repositoryManager.LabRepository.SearchAllAsync(searchTerm, cancellationToken);

            var LabsData = Labs.Adapt<IEnumerable<LabViewModel>>();

            return LabsData;
        }
        public async Task<LabViewModel> GetByIdAsync(int LabId, CancellationToken cancellationToken = default)
        {
            var Lab = await _repositoryManager.LabRepository.GetByIdAsync(LabId, cancellationToken);

            if (Lab is null)
            {
                throw new LabNotFoundException(LabId);
            }

            var LabData = Lab.Adapt<LabViewModel>();

            return LabData;
        }
        public async Task<LabViewModel> GetByNameAsync(string labName, CancellationToken cancellationToken = default)
        {
            var Lab = await _repositoryManager.LabRepository.GetByNameAsync(labName, cancellationToken);

            if (Lab is null)
            {
                throw new Exception("Lab not found by name  "+labName);
            }

            var LabData = Lab.Adapt<LabViewModel>();

            return LabData;
        }

    }
}