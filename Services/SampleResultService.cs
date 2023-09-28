using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RSPL.Domain.Exceptions;
using Domain.Repositories;
using Mapster;
using RSPL.Services.Abstractions;
using RSPL.Domain.Entities.Master;
using RSPL.Common;
using System.Linq;
using static RSPL.Common.Enums;
using RSPL.Domain.Models;
using RSPL.Contracts.Models.SampleResultModels;
using RSPL.Domain;
using System;
using Microsoft.AspNetCore.Hosting;
using RSPL.Contracts.Models.SampleModels;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using RSPL.Domain.Models.Dashboard;
using System.Runtime.CompilerServices;

namespace Services
{
    internal sealed class SampleResultService : ISampleResultService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;
        public SampleResultService(IRepositoryManager repositoryManager,
             IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        }
        //public async Task<IEnumerable<SampleResultWithDetailsModel>> DownloadAsync(CancellationToken cancellationToken = default)
        //{
        //    return await _repositoryManager.SampleResultRepository.DownloadAsync(cancellationToken);
        //}
        public async Task<IEnumerable<SampleResultBaseModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var SampleResults = await _repositoryManager.SampleResultRepository.GetAllAsync(cancellationToken);

            var SampleResultsData = SampleResults.Adapt<IEnumerable<SampleResultBaseModel>>();

            return SampleResultsData;
        }
        //ram
        public async Task<PaginationResponse<SampleWithResultViewModel>> GetSampleResultAsync
(string searchTerm, DateTime? fromdate, DateTime? toDate,string testResult,
 PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleResultRepository.GetSampleResultAsync
                (searchTerm, fromdate, toDate, testResult, request, cancellationToken);
            var sampleResultData = response.Records.Adapt
                      <IEnumerable<SampleWithResultViewModel>>();

            return new PaginationResponse<SampleWithResultViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = sampleResultData
            };
        }
        public async Task<IEnumerable<SampleWithResultViewModel>> SearchAllAsync
            (string searchTerm, DateTime? fromdate, DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var sampleResults = await _repositoryManager.SampleResultRepository.SearchAllAsync
                (searchTerm, fromdate, toDate, cancellationToken);

            var sampleResultData = sampleResults.Adapt<IEnumerable<SampleWithResultViewModel>>();

            return sampleResultData;
        }


        //error in SampleGetId


        public async Task<SampleWithResultViewModel> GetBySampleIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var sample = await _repositoryManager.SampleResultRepository.GetBySampleIdAsync(SampleId, cancellationToken);

            if (sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var sampleData = sample.Adapt<SampleWithResultViewModel>();

            return sampleData;
        } 
        //error in SampleGetId


        public async Task<SampleResultBaseModel> GetByIdAsync(long SampleResultId, CancellationToken cancellationToken = default)
        {
            var SampleResult = await _repositoryManager.SampleResultRepository.GetByIdAsync(SampleResultId, cancellationToken);

            if (SampleResult is null)
            {
                throw new SampleResultNotFoundException(SampleResultId);
            }

            var SampleResultData = SampleResult.Adapt<SampleResultBaseModel>();

            return SampleResultData;
        }

        public async Task<SampleResultBaseModel> CreateAsync(SampleResultInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var result = new AppResponse();

            var SampleResultData = await _repositoryManager.SampleResultRepository.GetResultBySampleIdAsync(InsertModel.SampleId, cancellationToken);

            if (SampleResultData is not null)
            {
                throw new RecordAlreadyExistsException("Sample ", InsertModel.SampleId);
            }

            var sampleData = await _repositoryManager.SampleRepository.GetByIdAsync(InsertModel.SampleId, cancellationToken);

            if (sampleData != null && sampleData.MarketSample != null && sampleData.MarketSample.BrandName != BrandName.RSPL)
            {
                InsertModel.TestResult = "NA";
            }


            var SampleResult = InsertModel.Adapt<SampleResult>();

            if (SampleResult.SampleTestResults != null)
            {
                foreach (var testResult in SampleResult.SampleTestResults)
                {
                    if (!string.IsNullOrEmpty(testResult.QualitativeValue))
                    {
                        if (!EnumHelperMethods.EnumContainValue<QualitativeValue>(testResult.QualitativeValue))
                        {
                            throw new Exception("Qualitative value is not valid Value=" + testResult.QualitativeValue);
                        }
                    }
                }

                if (SampleResult.SampleTestResults.Any(td => td.QualitativeValue == "NotOk")
                            && SampleResult.TestResult != "NA")
                {
                    SampleResult.TestResult = "Failed";
                }
            }

            var validateResult = ValidateSampleResultEntity(SampleResult);

            var validate_result = ValidateSampleResultEntity(SampleResult);
            if (validate_result.Succeeded != true)
            {
                result.Message = validate_result.Message;
                //return result;
            }
            //setting the testing date
            SampleResult.SampleTestingDate = DateTime.Now;
            _repositoryManager.SampleResultRepository.Insert(SampleResult);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return SampleResult.Adapt<SampleResultBaseModel>();
        }

        // write actual implemenatation   
        /// <summary>
        /// returns different count analisys of sample results for dashboard for lab department after lab is assigned 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<SampleReultCountDto> GetSampleResultCounts(
       DateTime? from,
       DateTime? to,
       CancellationToken cancellationToken = default)
        {
            var query = (await _repositoryManager.SampleResultRepository.GetSampleResultAsync("", null, null, null, new PaginationRequest(), cancellationToken, true)).Records;

            if (from != null && to != null && from <= to)
            {
                query = query.Where(x => x.SampleReceivingDate != null && x.SampleReceivingDate.Value.Date >= from && x.SampleReceivingDate.Value.Date <= to);
            }

            var failedRecords = query.Count(x => x.TestResult?.Trim().ToLower() == "failed");
            var successRecords = query.Count(x => x.TestResult?.Trim().ToLower() == "pass");
            var naRecords = query.Count(x => !string.IsNullOrEmpty(x.TestResult)) - (failedRecords + successRecords);
            var pending = query.Count(x => string.IsNullOrEmpty(x.TestResult));
            var totalRecords = query.Count();
            var labDepartmentDashboard = new SampleReultCountDto
            {
                failed = failedRecords,
                pass = successRecords,
                pending = pending,
                sampleReceived = totalRecords,
                sampleTested = failedRecords + successRecords,
                naRecords = naRecords
            };

            return labDepartmentDashboard;
        }


        /// <summary>
        ///  returns different count analisys of sample results for dashboard for smaple department selection before lab assignment i.e. based on Sample date 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>


        public async Task<SampleReultCountDto> GetSampleResultCountsBySampleDate(
      DateTime? from,
      DateTime? to,
      string sourceType,string sampleSource,string productName,
      CancellationToken cancellationToken = default)
        {
            var query = (await _repositoryManager.SampleResultRepository.GetSampleResultAsync("", null, null, null,new PaginationRequest(), cancellationToken, true)).Records;
            if (from != null && to != null)
            {
                query = query.Where(x => x.SampleDate != null ? (x.SampleDate.Date >= from && x.SampleDate.Date <= to):false);
            }

            var failedRecords = query.Where(x => x.TestResult?.Trim().ToLower() == "failed").Count();
            var successRecords = query.Where(x => x.TestResult?.Trim().ToLower() == "pass").Count();
            var naRecords = query.Count(x => !string.IsNullOrEmpty(x.TestResult)) - (failedRecords + successRecords);
            var pending = query.Count(x => string.IsNullOrEmpty(x.TestResult));
            var totalRecords = query.Count();
            var labDepartmentDashboard = new SampleReultCountDto
            {
                failed = failedRecords,
                pass = successRecords,
                pending = pending,
                sampleReceived = totalRecords,
                sampleTested = failedRecords + successRecords,
                naRecords = naRecords
            };

            return labDepartmentDashboard;
        }




        //7 day data 


        public async Task<List<SampleResultCounts7DaysDto>> GetSampleResultOf7Day(CancellationToken cancellationToken = default)
        {
            var todayUtc = DateTime.Now;
            var sevenDaysAgoUtc = todayUtc.AddDays(-7);


            var query = await _repositoryManager.SampleResultRepository.GetSampleResultAsync("", null, null, null, new PaginationRequest(), cancellationToken, true);
            var recordsWithin7Days = query.Records.Where(x => x.SampleReceivingDate >= sevenDaysAgoUtc && x.SampleReceivingDate <= todayUtc);

            var result = new List<SampleResultCounts7DaysDto>();

            for (var i = 6; i >= 0; i--)
            {
                var daysAgoDateUtc = todayUtc.AddDays(-i);
                var ithDayCountData = GetSampleResultsCountsBasedOnDate(recordsWithin7Days, daysAgoDateUtc);
                result.Add(ithDayCountData);
            }

            // Check if today's data is missing and add it if necessary
            var todayDataUtc = result.FirstOrDefault(r => r.Date.Date == todayUtc.Date);
            if (todayDataUtc == null)
            {
                var todayCountData = GetSampleResultsCountsBasedOnDate(recordsWithin7Days, todayUtc);
                result.Add(todayCountData);
            }

            return result;
        }

        public static SampleResultCounts7DaysDto GetSampleResultsCountsBasedOnDate(IEnumerable<SampleWithResultViewModel> query, DateTime dateUtc)
        {
            var filteredData = query.Where(x => x.SampleReceivingDate == null ? false : (x.SampleReceivingDate.Value.Date == dateUtc.Date));
            var failedRecords = filteredData.Count(x => x.TestResult == null ? false : x.TestResult.Trim().Equals("failed", StringComparison.OrdinalIgnoreCase));
            var successRecords = filteredData.Count(x => x.TestResult == null ? false : x.TestResult.Trim().Equals("pass", StringComparison.OrdinalIgnoreCase));
            var pending = filteredData.Count(x => string.IsNullOrWhiteSpace(x.TestResult));
            var totalRecords = filteredData.Count();
            var labDepartment7DaysData = new SampleResultCounts7DaysDto
            {
                failed = failedRecords,
                pass = successRecords,
                sampleReceived = totalRecords,
                pending = pending,
                sampleTested = failedRecords + successRecords,
                Date = dateUtc.Date
            };
            return labDepartment7DaysData;
        }
        //public async Task<HashSet<object>> GetTestResultsStatusService(string productCategory, string productType, string testType, DateTime startDate, DateTime endDate)
        //{

        //    var samplesWithTestResults = await _repositoryManager.SampleResultRepository.GetTestResultsStatus(productCategory, productType,testType, startDate, endDate);

        //    return samplesWithTestResults;
        //}


        //RAm
        public async Task<IList<SampleResultStatusDownloadDto>>GetTestResultsStatusService(string productCategory, string productType, string testType, string testResult, DateTime startDate, DateTime endDate)
        {

            var samplesWithTestResults = await _repositoryManager.SampleResultRepository.GetTestResultsStatus(productCategory, productType, testType,testResult, startDate, endDate);

            return samplesWithTestResults;
        }





        #region extension_methods
        private AppResponse SetNewSampleTestResults(SampleResult entityItem,
      IEnumerable<SampleTestResultModel> SampleTestResultModels)
        {
            var result = new AppResponse();

            foreach (var item in SampleTestResultModels)
            {
                var SampleTestResultNormalEntity = item.ToSampleTestResultEntity();
                entityItem.SampleTestResults.Add(SampleTestResultNormalEntity);


            }
            result.Succeeded = true;
            return result;
        }

        #endregion
        #region Validate_Methods

        private AppResponse ValidateSampleResultEntity(SampleResult item)
        {
            var result = new AppResponse();

            if (!EnumHelperMethods.EnumContainValue<TestCategory>(item.TestCategory))
            {
                result.Message = "TestCategory value is not valid";
                return result;
            }
            if (!EnumHelperMethods.EnumContainValue<ProductType>(item.ProductType))
            {
                result.Message = "ProductType value is not valid";
                return result;
            }

            if (!EnumHelperMethods.EnumContainValue<TestResult>(item.TestResult))
            {
                result.Message = "TestResult value is not valid";
                return result;
            }

            result.Succeeded = true;
            return result;
        }
        #endregion
    }
}






