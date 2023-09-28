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
    internal sealed class SampleDashboardService : ISampleDashboardService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public SampleDashboardService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        }

        public async Task<(int currentMonthSamples, int previousMonthSamples, double percentageChange)> GetMonthlySampleCountAsync(string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleDashboardRepository.GetMonthlySampleCountAsync(sampleReceivedFrom);
        }
        public async Task<IEnumerable<WeeklySampleModel>> GetWeeklySampleCountAsync(string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleDashboardRepository.GetWeeklySampleCountAsync(sampleReceivedFrom);
        }
        public async Task<IEnumerable<MonthWiseSampleModel>> GetMonthWiseSampleCountAsync(string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleDashboardRepository.GetMonthWiseSampleCountAsync(sampleReceivedFrom);
        }

        public async Task<ResultDashboardModel> GetSampleResultstatusDashboardAsync(DateTime fromDate, DateTime toDate, string status)
        {
            return await _repositoryManager.SampleDashboardRepository.GetSampleResultstatusDashboardAsync(fromDate,toDate,status);
        }
        public async Task<SampleDepartmentDashboardModel> GetSampleDepartmentDashboardAsync(DateTime fromDate, DateTime toDate)
        {
            return await _repositoryManager.SampleDashboardRepository.GetSampleDepartmentDashboardAsync(fromDate, toDate);
        }
        public async Task<AdminDashboardModel> GetAdminDashboardAsync(DateTime fromDate, DateTime toDate)
        {
            return await _repositoryManager.SampleDashboardRepository.GetAdminDashboardAsync(fromDate, toDate);

        }
    }
    
}