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
using RSPL.Contracts.Models.FactoryModels;
using RSPL.Domain;
using System.Linq;
using RSPL.Domain.Views;
using RSPL.Contracts.Models.SamplingDepartmentModels;

namespace Services
{
    internal sealed class FactoryService : IFactoryService
    {
        private readonly IRepositoryManager _repositoryManager;

        public FactoryService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }
        public async Task<IEnumerable<FactoryViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Factorys = await _repositoryManager.FactoryRepository.GetAllAsync(cancellationToken);

            var FactorysData = Factorys.Adapt<IEnumerable<FactoryViewModel>>();

            return FactorysData;
        }
        public async Task <PaginationResponse<FactoryViewModel>> GetAllAsync
            (string searchTerm, PaginationRequest request, 
            CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.FactoryRepository.GetAllAsync
                (searchTerm,request,cancellationToken);
            var factoryData = response.Records.Adapt
                <IEnumerable<FactoryViewModel>>();
            
            return new PaginationResponse<FactoryViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = factoryData
            };
        }
        public async Task<IEnumerable<FactoryViewModel>> SearchAllAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var Factorys = await _repositoryManager.FactoryRepository.SearchAllAsync(searchTerm,cancellationToken);

            var FactorysData = Factorys.Adapt<IEnumerable<FactoryViewModel>>();

            return FactorysData;
        }

        public async Task<FactoryViewModel> GetByIdAsync(int FactoryId, CancellationToken cancellationToken = default)
        {
            var Factory = await _repositoryManager.FactoryRepository.GetByIdAsync(FactoryId, cancellationToken);

            if (Factory is null)
            {
                throw new FactoryNotFoundException(FactoryId);
            }

            var FactoryData = Factory.Adapt<FactoryViewModel>();

            return FactoryData;
        }
        public async Task<HashSet<object>> GetFactoryDataCountMonthly(int month, CancellationToken cancellationToken = default)
        {
            var todayUtc = DateTime.Now;
            var sevenDaysAgoUtc = todayUtc.AddDays(-30);

            var factoryDataCount = await _repositoryManager.FactoryRepository.GetFactoryDataCountsByDateRange( month,sevenDaysAgoUtc, todayUtc, cancellationToken);
           

            return factoryDataCount;
        }

        //ram
        public async Task<List<object>> GetFactoryCategoryYearlyDataAsync(int year)
        {
            var result = await _repositoryManager.FactoryRepository.GetFactoryCategoryYearlyDataAsync(year);

            return result;
        }

        public async Task<List<object>> GetMonthlyProductNameCountsAsync(int year)
        {
            var result = await _repositoryManager.FactoryRepository.GetMonthlyProductNameCountsAsync(year);

            return result;
        }
    }
}