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
using RSPL.Domain.Entities.Master;
using RSPL.Domain;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Mapster.Utils;
using static RSPL.Common.Enums;
using RSPL.Contracts.Models.SampleModels;
using RSPL.Contracts.Models.SamplingDepartmentModels;
using RSPL.Contracts.Models.LabModels;
using RSPL.Contracts.Models.FactoryModels;
using System.Linq;
using RSPL.Domain.Models.Dashboard;

namespace Services
{
    internal sealed class FactoryDashboardService : IFactoryDashboardService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public FactoryDashboardService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        }

        public async Task<IEnumerable<DateWiseFactorySampleModel>> GetDateWiseSampleSendToLabDataAsync(string searchText)
        {
            return await _repositoryManager.FactoryDashboardRepository.GetDateWiseSampleSendToLabDataAsync(searchText);

        }

        public async Task<IEnumerable<DateWiseFactorySampleModel>> GetDateWiseSubmissionDataAsync(string searchText)
        {
            return await _repositoryManager.FactoryDashboardRepository.GetDateWiseSubmissionDataAsync(searchText);

        }

        public async Task<IEnumerable<MonthWiseFactorySampleModel>> GetMonthWiseSampleSendToLabDataAsync(string searchText)
        {
            return await _repositoryManager.FactoryDashboardRepository.GetMonthWiseSampleSendToLabDataAsync(searchText);

        }

        public async Task<IEnumerable<MonthWiseFactorySampleModel>> GetMonthWiseSubmissionDataAsync(string searchText)
        {
            return await _repositoryManager.FactoryDashboardRepository.GetMonthWiseSubmissionDataAsync(searchText);

        }
    }
    
}