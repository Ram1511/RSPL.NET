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
using RSPL.Common;
using RSPL.Domain.Models.Dashboard;
using System.Collections;
using RSPL.Contracts.Models.SampleResultModels;
using RSPL.Domain.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services
{
    internal sealed class SampleService : ISampleService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;
        private readonly ISampleResultService _sampleResultService;

        public SampleService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment, ISampleResultService sampleResultService)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
            _sampleResultService = sampleResultService;
        }

        public async Task<SampleViewModel> GetByIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var Sampledata = Sample.Adapt<SampleViewModel>();

            return Sampledata;
        }
        public async Task<SampleWithImageViewModel> GetByIdWithImagesAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var sampleData = Sample.Adapt<SampleWithImageViewModel>();
            var base64ProductString = await GetImageBase64Async(sampleData.ProductImageName);
            sampleData.ProductImageNameBase64 = base64ProductString;
            var base64ReferenceString = await GetImageBase64Async(sampleData.ReferenceImageName);
            sampleData.ReferenceImageNameBase64 = base64ReferenceString;
            return sampleData;
        }
        #region ExtraMethods
        public async Task<string> GetImageBase64Async(string imageName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", $"{imageName}");
            if (!System.IO.File.Exists(filePath))
            {
                // throw new ItemNotFoundException("Image", imageName);
                return null;
            }
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var base64String = Convert.ToBase64String(memory.ToArray());
            return base64String;
        }

        private async Task SaveImages(IFormFile productImage, IFormFile referenceImageName, Sample Sample, string imageNamePrefix, CancellationToken cancellationToken)
        {
            string uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            string productImagePath = Path.Combine(uploadsPath, GetUniqueFileName(imageNamePrefix, productImage));
            using (var productImageStream = System.IO.File.Create(productImagePath))
            {
                await productImage.CopyToAsync(productImageStream, cancellationToken);
                Sample.ProductImageName = Path.GetFileName(productImagePath);
            }
            if (referenceImageName.Length > 0)
            {
                string referenceImageNamePath = Path.Combine(uploadsPath, GetUniqueFileName(imageNamePrefix, referenceImageName));
                using (var referenceImageNameStream = System.IO.File.Create(referenceImageNamePath))
                {
                    await referenceImageName.CopyToAsync(referenceImageNameStream, cancellationToken);
                    Sample.ReferenceImageName = Path.GetFileName(referenceImageNamePath);
                }
            }
        }
        private string GetUniqueFileName(string prefix, IFormFile formFile)
        {
            /*
             Product =Img
            Reference=BRC
             */
            return prefix + "_" + Guid.NewGuid().ToString() + "_" + formFile.FileName;
        }
        #endregion
        public async Task UpdateAsync(SampleUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleRepository
                .GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(UpdateModel.Id);
            }
            else if (!string.IsNullOrEmpty(Sample.LabName))
            {
                throw new Exception("Lab already assigned for this sample " + Sample.LabName);
            }
            var TestDetail = await _repositoryManager.TestRepository.
                GetLastTestDetailByProductNameAndBrandNameAsync
                (UpdateModel.ProductOrMaterialName, UpdateModel.BrandName, cancellationToken);
            if (TestDetail is null)
            {
                throw new Exception("TestDetail not found for product name:- " + UpdateModel.ProductOrMaterialName);
            }
            //var TestDetail = await _repositoryManager.TestRepository.GetByIdAsync(UpdateModel.TestId, cancellationToken);
            //if (TestDetail is null)
            //{
            //    throw new Exception("TestDetail not found for product name " //+ Sample.ProductName
            //        );
            //}
            #region Set UniqueCode
            string randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(6);

            while (await _repositoryManager.SampleRepository.GetByTestCodeAsync
                (randomAlphaNumeric, cancellationToken))
            {
                randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(6); // Generate a new random alphanumeric number
            }
            #endregion
            var labDetails = await _repositoryManager.LabRepository.GetByNameAsync
                  (UpdateModel.LabName, cancellationToken);
            Sample.TestCode = labDetails.Code + randomAlphaNumeric;
            var test = await _repositoryManager.TestRepository.GetByProductNameAsync
                    (UpdateModel.ProductOrMaterialName, cancellationToken);
            Sample.TestId = test.Id;
            Sample.LabRemarks = UpdateModel.LabRemarks;
            Sample.ModifiedBy = UpdateModel.ModifiedBy;
            Sample.ModifiedDate = UpdateModel.ModifiedDate;
            //  Sample.TestCategory = (TestCategory)Enum.Parse(typeof(TestCategory), UpdateModel.TestCategory);
            Sample.LabName = UpdateModel.LabName;

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<SamplingDepartmentViewModel> GetSamplingDepartmentByIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }
            var Sampledata = Sample.Adapt<SamplingDepartmentViewModel>();
            return Sampledata;
        }
        public async Task<SamplingDepartmentWithImageViewModel> GetSamplingDepartmentByIdWithImagesAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleRepository.GetByIdAsync(SampleId, cancellationToken);
            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }
            var sampleData = Sample.Adapt<SamplingDepartmentWithImageViewModel>();
            sampleData.SampleDate = sampleData.SampleDate.UtcToIndianTime();
            sampleData.SampleTime = sampleData.SampleTime.Add(TimeSpan.FromHours(5)).Add(TimeSpan.FromMinutes(30));
            var base64ProductString = await GetImageBase64Async(sampleData.ProductImageName);
            sampleData.ProductImageNameBase64 = base64ProductString;
            var base64ReferenceString = await GetImageBase64Async(sampleData.ReferenceImageName);
            sampleData.ReferenceImageNameBase64 = base64ReferenceString;
            return sampleData;
        }
        public async Task<PaginationResponse<SamplingDepartmentViewModel>> GetAllSampleDepartmentAsync
            (string searchTerm, DateTime? fromDate, DateTime? toDate, string sampleReceivedFrom,string marketSampleType, string category,string productName, string rawMaterialName, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleRepository.GetAllSampleDepartmentAsync
                  (searchTerm, fromDate, toDate, sampleReceivedFrom, marketSampleType, category, productName,rawMaterialName, request, cancellationToken);
            var SampleData = response.Records.Adapt
                <IEnumerable<SamplingDepartmentViewModel>>();

            return new PaginationResponse<SamplingDepartmentViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = SampleData
            };
        }
        public async Task<IEnumerable<SamplingDepartmentViewModel>> SearchAllSampleDepartmentAsync
            (string searchTerm, DateTime? fromDate, DateTime? toDate,
            string sampleReceivedFrom, CancellationToken cancellationToken = default)
        {
            var Samples = await _repositoryManager.SampleRepository.SearchAllSampleDepartmentAsync
                (searchTerm, fromDate, toDate, sampleReceivedFrom, cancellationToken);

            var SamplesData = Samples.Adapt<IEnumerable<SamplingDepartmentViewModel>>();

            return SamplesData;
        }


        //SamplingCollectionDepartmentDownload

        public async Task<IEnumerable<SampleDepartmentDownloadDto>> SearchAllSampleDepartmentDownloadAsync
          (string searchTerm, DateTime? fromDate, DateTime? toDate,
          string sampleReceivedFrom, CancellationToken cancellationToken = default)
        {
            var Samples = await _repositoryManager.SampleRepository.SearchAllSampleDepartmentDownloadAsync
                (searchTerm, fromDate, toDate, sampleReceivedFrom, cancellationToken);

            var SamplesData = Samples.Adapt<IEnumerable<SampleDepartmentDownloadDto>>();

            return SamplesData;
        }




        public async Task<SampleDepartmentDashboardDto> GetSampleDepartmentDashboardCount(
            DateTime? fromDate,
    DateTime? toDate,
    string sourceType,
    string sampleSource,
    string productName,
    CancellationToken cancellationToken = default)
        {
            var result = new SampleDepartmentDashboardDto();

            var sampleCountData = await _sampleResultService.GetSampleResultCountsBySampleDate(fromDate, toDate, sourceType, sampleSource, productName, cancellationToken);

            var data = (await _repositoryManager.SampleRepository.GetAllSampleDataByFilter(fromDate, toDate, sourceType, sampleSource, productName, new PaginationRequest(), true, cancellationToken)).Records;

            //var query = data.Where(x => x.SampleDate >= fromDate && x.SampleDate <= toDate);

            var TotalSampleReceived = data.Count();
            var LabAssignedAndTestCodeGen = data.Where(x => !string.IsNullOrEmpty(x.LabName) && !string.IsNullOrEmpty(x.TestCode)).Count();
            var PendingForLabAssigned = TotalSampleReceived - LabAssignedAndTestCodeGen;
            var failed = sampleCountData.failed;
            var pass = sampleCountData.pass;
            var naRecords = sampleCountData.naRecords;
            //var SampleTestedAndResultRecorded = sampleCountData.sampleTested;
            var SampleTestedAndResultRecorded = sampleCountData.naRecords + sampleCountData.failed + sampleCountData.pass;


            // Create the SampleDepartmentDashboardDto object
            result.TotalSampleReceived = TotalSampleReceived;
            result.LabAssignedAndTestCodeCount = LabAssignedAndTestCodeGen;
            result.PendingForLabAssignment = PendingForLabAssigned;
            result.SampleTestedAndResultRecorded = SampleTestedAndResultRecorded;
            result.Failed = failed;
            result.Pass = pass;
            result.naRecords = naRecords;

            return result;
        }

        public async Task<List<FactorySampleCategoryCountDto>> GetSampleDepartmentDashboardFactoryCount(
            DateTime? fromDate,
            DateTime? toDate,
            string[] factoryCodes,
            CancellationToken cancellationToken = default)
        {
            var result = new List<FactorySampleCategoryCountDto>();

            var data = (await _repositoryManager.SampleRepository.GetAllSampleDataByFilter(
                fromDate, toDate, nameof(SourceType.Factory), "", "", new PaginationRequest(), true, cancellationToken)).Records;

            if (factoryCodes == null || factoryCodes.Count() == 0 || factoryCodes.Any(string.IsNullOrWhiteSpace))
            {
                factoryCodes = data.Select(x => x.FactorySample.FactoryCode).Distinct().ToArray();
            }



            //var groupedData = data.Where(x => factoryIds.Contains(x.FactorySample.Id))
            //                      .GroupBy(x => x.FactorySample.FactoryName);

            foreach (var factroyCode in factoryCodes)
            {
                var factoryCountData = new FactorySampleCategoryCountDto();
                var factoryData = data.Where(x => x.FactorySample.FactoryCode == factroyCode);
                factoryCountData.FactoryName = factoryData.FirstOrDefault() == null ? null : factoryData.FirstOrDefault().FactorySample.FactoryName;
                factoryCountData.RawMaterial = factoryData.Count(x => x.FactorySample.FactorySampleCategory == Category.RawMaterial);
                factoryCountData.FinishedGoods = factoryData.Count(x => x.FactorySample.FactorySampleCategory == Category.FinishedGoods);
                factoryCountData.SemiFinishedGoods = factoryData.Count(x => x.FactorySample.FactorySampleCategory == Category.SemiFinishedGoods);

                result.Add(factoryCountData);

            }

            return result;
        }




        public async Task<List<MarketSampleTypeCategoryDto>> GetMarketSampleDashboardCount(
        DateTime? fromDate,
        DateTime? toDate,
        string[] MarketSampleTypes,
        string[] states,
        CancellationToken cancellationToken = default)
        {
            var result = new List<MarketSampleTypeCategoryDto>();
            var completeSampleData = new List<Sample>();

            if (MarketSampleTypes.Where(x => x.Contains(nameof(MarketSampleType.MarketSample))).Count() > 0 || MarketSampleTypes.Count() == 0)
            {

                var data = (await _repositoryManager.SampleRepository.GetAllSampleDataByFilter(
                                fromDate, toDate, nameof(SourceType.Market), "", "", new PaginationRequest(), true, cancellationToken)).Records;


                completeSampleData.AddRange(data);
            }

            if (MarketSampleTypes.Where(x => x.Contains(nameof(MarketSampleType.QualityComplaint))).Count() > 0 || MarketSampleTypes.Count() == 0)
            {

                var data2 = (await _repositoryManager.SampleRepository.GetAllSampleDataByFilter(
                fromDate, toDate, nameof(SourceType.QualityComplaint), "", "", new PaginationRequest(), true, cancellationToken)).Records;
                completeSampleData.AddRange(data2);
            }
            // only market data has state so if not null apply filter on the basisi of that
            if (states.Count() > 0)
            {
                completeSampleData = completeSampleData.Where(x => x.MarketSample.State == null ? false : (states.Where(s =>
                x.MarketSample.State.ToLower().Trim() == s.ToLower().Trim()).Count() > 0)).ToList();
            }

            result = completeSampleData.GroupBy(x => x.MarketSample.State.ToUpper()).Select(x => new MarketSampleTypeCategoryDto()
            {
                State = x.Key,
                MarketSample = x.Where(x => x.MarketSample.MarketSampleType == MarketSampleType.MarketSample).Count(),
                MarketComplain = x.Where(x => x.MarketSample.MarketSampleType == MarketSampleType.QualityComplaint).Count()
            }).ToList();



            return result;
        }


        // 7 day data fo sample departDepartment weekly  dashboard
        //7 day data





        public async Task<List<SampleDepartment7dayDto>> GetSampleDepartmentOf7DayData(CancellationToken cancellationToken = default)
        {
            var todayUtc = DateTime.Now;
            var sevenDaysAgoUtc = todayUtc.AddDays(-7);

            var query = (await _repositoryManager.SampleRepository.GetAllSampleDataByFilter( sevenDaysAgoUtc, todayUtc, "","","", new PaginationRequest(),true,cancellationToken)).Records;

            var recordsWithin7Days = query.ToList();
            var result = new List<SampleDepartment7dayDto>();

            for (var i = 6; i >= 0; i--)
            {
                var daysAgoDateUtc = todayUtc.AddDays(-i);
                var ithDayCountData = GetSampleDepartmentCountsBasedOnDate(recordsWithin7Days.Where(x=>x.SampleDate.Date == daysAgoDateUtc.Date).ToList(), daysAgoDateUtc);
                result.Add(ithDayCountData);
            }
            return result;
        }

        public static SampleDepartment7dayDto GetSampleDepartmentCountsBasedOnDate(IList<Sample> dataForDate, DateTime date)
        {            
            var FactorySample = dataForDate.Count(x=> x.FactorySample!=null);
            var MarketSample = dataForDate.Count(x => x.MarketSample != null && x.MarketSample.MarketSampleType ==MarketSampleType.MarketSample);
            var QualityComplaint = dataForDate.Count(x => x.MarketSample!= null && x.MarketSample.MarketSampleType == MarketSampleType.QualityComplaint);

            var RawMaterial = dataForDate.Count(x => x.FactorySample != null && x.FactorySample.FactorySampleCategory==Category.RawMaterial);
            var FinishedGood = dataForDate.Count(x => x.FactorySample != null && x.FactorySample.FactorySampleCategory==Category.FinishedGoods);
            var SemiFinishedGood = dataForDate.Count(x => x.FactorySample != null && x.FactorySample.FactorySampleCategory==Category.SemiFinishedGoods);

            var sampleDepatment7DaysData = new SampleDepartment7dayDto
            {
                FactorySamples = FactorySample,
                MarketSamples = MarketSample,
                QualityComplaints = QualityComplaint,
                RawMaterial = RawMaterial,
                FinishedGoods = FinishedGood,
                SemiFinishedGoods = SemiFinishedGood,
                Date = date.Date,
            };
            return sampleDepatment7DaysData;
        }







        //public async Task DeleteAsync(long SampleId, CancellationToken cancellationToken = default)
        //{
        //    var Sample = await _repositoryManager.SampleRepository.GetByIdAsync(SampleId, cancellationToken);

        //    if (Sample is null)
        //    {
        //        throw new SampleNotFoundException(SampleId);
        //    }

        //    _repositoryManager.SampleRepository.Remove(Sample);

        //    await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        //}

        //SAMPLE Department monthaly 
        //SAMPLE Department monthaly 





        public async Task<List<object>> GetProductCountMonthly(int month, CancellationToken cancellationToken = default)
        {
            var todayUtc = DateTime.Now;
            var sevenDaysAgoUtc = todayUtc.AddDays(-30);

            var productCounts = await _repositoryManager.SampleRepository.GetProductCountsByDateRange(month,sevenDaysAgoUtc, todayUtc, cancellationToken);

           
            return productCounts;
        }


       



    }
}