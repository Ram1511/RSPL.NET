using System;
using RSPL.Services.Abstractions;
using RSPL.Domain.Entities.Master;
using RSPL.Domain;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; 
using static RSPL.Common.Enums; 
using System.Linq;
using RSPL.Contracts.Models.SampleModels.MarketSamples;
using RSPL.Common;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RSPL.Contracts;
using RSPL.Domain.Entities;
using RSPL.Domain.Exceptions;
using Domain.Repositories;
using Mapster;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
//using RSPL.Domain.Models;

namespace Services
{
    internal sealed class SampleMarketService : ISampleMarketService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public SampleMarketService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        }
        public async Task<PaginationResponse<SampleViewModel>> GetAllSampleMarketAsync
          (string searchTerm, DateTime? fromdate, DateTime? toDate,
            PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleMarketRepository
                .GetAllSampleMarketAsync
                (searchTerm, fromdate, toDate,
                request, cancellationToken);
            var SampleData = response.Records.Adapt
                <IEnumerable<SampleViewModel>>();

            return new PaginationResponse<SampleViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = SampleData
            };
        }
        public async Task<PaginationResponse<SampleMarketMobileViewModel>> GetAllSampleMarketMobileAsync
(string searchTerm, DateTime? fromdate, DateTime? toDate, PaginationRequest request,
CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleMarketRepository.GetAllSampleMarketMobileAsync
                (searchTerm, fromdate, toDate, request, cancellationToken);

            var sampleData = response.Records.Adapt(new List<SampleMarketMobileViewModel>());
            // Convert product images to base64 strings using paging
            for (int i = 0; i < sampleData.Count(); i++)
            {
                var base64String = await GetImageBase64Async(sampleData.ElementAt(i).ProductImageName);
                sampleData.ElementAt(i).ProductImageNameBase64 = base64String;
            }

            return new PaginationResponse<SampleMarketMobileViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = sampleData
            };
        }
        public async Task<IEnumerable<SampleViewModel>> SearchAllSampleMarketAsync
            (string searchTerm, DateTime? fromdate, DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var Samples = await _repositoryManager.SampleMarketRepository.SearchAllSampleMarketAsync
                (searchTerm, fromdate, toDate, cancellationToken);

            var SamplesData = Samples.Adapt<IEnumerable<SampleViewModel>>();

            return SamplesData;
        }
        public async Task<SampleViewModel> GetByIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleMarketRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var Sampledata = Sample.Adapt<SampleViewModel>();

            return Sampledata;
        }
        public async Task<SampleWithImageViewModel> GetByIdWithImagesAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleMarketRepository.GetByIdAsync(SampleId, cancellationToken);

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

        public async Task<SampleViewModel> CreateMarketAsync(SampleMarketInsertModel SampleModel, CancellationToken cancellationToken = default)
        {
            if (SampleModel.MarketSample.SampleSource != "Retailer")
            {
                SampleModel.MarketSample.NameOfRetailer = string.Empty;
                SampleModel.MarketSample.RetailerFirmName = string.Empty;
                SampleModel.MarketSample.RetailerAddress = string.Empty;
            }
            if (SampleModel.MarketSample.SampleSource == "RSPLBD")
            {
                SampleModel.MarketSample.NameOfRetailer = string.Empty;
                SampleModel.MarketSample.RetailerFirmName = string.Empty;
                SampleModel.MarketSample.RetailerAddress = string.Empty;
                SampleModel.MarketSample.BusinessPartnerName = string.Empty;
                SampleModel.MarketSample.BusinessPartnerFirmName = string.Empty;
                SampleModel.MarketSample.BusinessPartnerCode = string.Empty;
                SampleModel.MarketSample.BusinessPartnerAddress = string.Empty;
                SampleModel.MarketSample.Area = string.Empty;
                SampleModel.MarketSample.City = string.Empty;
                SampleModel.MarketSample.District = string.Empty;
                SampleModel.MarketSample.State = string.Empty;
            }
            var Sample = SampleModel.Adapt<Sample>();
            if (SampleModel.productImage != null)
            {
                if (SampleModel.productImage.Length > 0)
                {
                    await SaveImages(SampleModel.productImage, SampleModel.referenceImageName, Sample, "F", cancellationToken);
                }
            }
            else
            {
                throw new Exception("Product Image field can't be null");

            }
            #region Set UniqueCode
            string randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(8);

            while (await _repositoryManager.SampleMarketRepository.GetByUniqueCodeAsync
                (randomAlphaNumeric, cancellationToken))
            {
                randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(8); // Generate a new random alphanumeric number
            }
            #endregion
            Sample.UniqueCode = randomAlphaNumeric;

            _repositoryManager.SampleMarketRepository.Insert(Sample);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Sample.Adapt<SampleViewModel>();
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
            if (referenceImageName != null)
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
             Product =ImgFact
            Reference=BRCFact
             */
            return prefix + "_" + Guid.NewGuid().ToString() + "_" + formFile.FileName;
        }
        #endregion


        //public async Task<HashSet<object>> GetMarketSampleResultServicesAsync(string complaintSource, string sampleSource, string marketSampleType, string brandName, string productName, string sku, string complaintType, DateTime startDate, DateTime endDate)
        //{
        //    var result = await _repositoryManager.SampleMarketRepository.GetMarketSampleResultsAsync(complaintSource, sampleSource, marketSampleType, brandName, productName, sku, complaintType, startDate, endDate);

        //    return result;
        //}

        // Ram

        public async Task<IList<RSPL.Domain.Models.MarketSampleResultDownloadDto>>GetMarketSampleResultServicesAsync(string complaintSource, string sampleSource, string marketSampleType, string brandName, string productName, string sku, string complaintType, DateTime startDate, DateTime endDate)
        {
            var marketSampleResult = await _repositoryManager.SampleMarketRepository.GetMarketSampleResultsAsync(complaintSource, sampleSource, marketSampleType, brandName, productName, sku, complaintType, startDate, endDate);

            return marketSampleResult;
        }


        //state
        public async Task<IList<RSPL.Domain.Models.MarketSampleFactoryWiseDownloadDto>> GetMarketSampleFactoryReportServiceAsync(string testName, string productName, string sku, string state, string businessPartnerName, string nameOfRetailer, string businessPartnerFirmName, DateTime startDate, DateTime endDate)
        {
            var result = await _repositoryManager.SampleMarketRepository.GetMarketSampleFactoryReportAsync(testName, productName, sku,state, businessPartnerName, nameOfRetailer, businessPartnerFirmName, startDate, endDate);

            return result;
        }
        public async Task<IList<RSPL.Domain.Models.BrandWiseReportDownloadDto>> GetAggregatedMarketSampleResultsServiceAsync(string productName, string brandName, string sku, DateTime startDate, DateTime endDate)
        {
            var sampleBrandwise = await _repositoryManager.SampleMarketRepository.GetAggregatedMarketSampleResultsAsync(productName, brandName, sku, startDate, endDate);

            return sampleBrandwise;
        }

        public async Task<HashSet<object>> GetAggregatedChemicalAndGLCAnalysisAsyncService(string productName, string labName, string sku, DateTime startDate, DateTime endDate)
        {
            var result = await _repositoryManager.SampleMarketRepository.GetAggregatedChemicalAndGLCAnalysisAsync(productName, labName, sku, startDate, endDate);

            return result;
        }

      public async Task<IList<RSPL.Domain.Models.QualityComplaintReportDownloadDto>> GetAggregatedQualityComplaintReportAsync(string productName, string test, string sku, string businessPartnerName, string nameOfRetailer, string businessPartnerFirmName, string state, DateTime startDate, DateTime endDate)
        {
         var result = await _repositoryManager.SampleMarketRepository.GetAggregatedQualityComplaintReportAsync(productName,test,sku, businessPartnerName,nameOfRetailer,businessPartnerFirmName,state,startDate,endDate);

            return result;
        }

        public async Task<HashSet<object>> GetSampleDepartmentYearlyDataAsync(int year)
        {
            var result = await _repositoryManager.SampleMarketRepository.GetSampleDepartmentYearlyDataAsync(year);

            return result;
        }

        
    }
}