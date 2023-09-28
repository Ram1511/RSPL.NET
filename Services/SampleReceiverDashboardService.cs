using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Repositories;
using RSPL.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using RSPL.Domain.Models.Dashboard;

namespace Services
{
    internal sealed class SampleReceiverDashboardService : ISampleReceiverDashboardService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public SampleReceiverDashboardService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        }

        public async Task<IEnumerable<DateWiseSampleReceiverModel>> GetDateWiseSampleSendToLabDataAsync(string searchText, string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleReceiverDashboardRepository.GetDateWiseSampleSendToLabDataAsync(searchText, sampleReceivedFrom);

        }

        public async Task<IEnumerable<DateWiseSampleReceiverModel>> GetDateWiseSubmissionDataAsync(string searchText, string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleReceiverDashboardRepository.GetDateWiseSubmissionDataAsync(searchText, sampleReceivedFrom);

        }

        public async Task<IEnumerable<MonthWiseSampleReceiverModel>> GetMonthWiseSampleSendToLabDataAsync(string searchText, string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleReceiverDashboardRepository.GetMonthWiseSampleSendToLabDataAsync(searchText, sampleReceivedFrom);

        }

        public async Task<IEnumerable<MonthWiseSampleReceiverModel>> GetMonthWiseSubmissionDataAsync(string searchText, string sampleReceivedFrom)
        {
            return await _repositoryManager.SampleReceiverDashboardRepository.GetMonthWiseSubmissionDataAsync(searchText, sampleReceivedFrom);

        }
    }

}