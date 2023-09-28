using Domain.Repositories;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RSPL.Common;
using RSPL.Contracts.Models.SampleModels.FactorySamples;
using RSPL.Domain;
using RSPL.Domain.Exceptions;
using RSPL.Domain.Models;
using RSPL.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    internal sealed class SampleFactoryService : ISampleFactoryService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public SampleFactoryService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        } 
        public async Task<PaginationResponse<SampleViewModel>> GetAllSampleFactoryAsync
          (string searchTerm, DateTime? fromdate, DateTime? toDate,
              string? factoryName, string? factorySampleCategory,
            PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleFactoryRepository
                .GetAllSampleFactoryAsync
                (searchTerm,fromdate,toDate, factoryName, factorySampleCategory,
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

        //ram

     




        public async Task<PaginationResponse<RSPL.Contracts.Models.SampleModels.FactorySamples.SampleFactoryMobileViewModel>> GetAllSampleFactoryMobileAsync
(string searchTerm, DateTime? fromdate, DateTime? toDate, PaginationRequest request,
CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleFactoryRepository.GetAllSampleFactoryMobileAsync
                (searchTerm,fromdate,toDate, request, cancellationToken);
 
            var sampleData = response.Records.Adapt(new List<RSPL.Contracts.Models.SampleModels.FactorySamples.SampleFactoryMobileViewModel>());
            // Convert product images to base64 strings using paging
            for (int i = 0; i < sampleData.Count(); i++)
            {
                var base64String = await GetImageBase64Async(sampleData.ElementAt(i).ProductImageName);
                sampleData.ElementAt(i).ProductImageNameBase64 = base64String;
            }

            return new PaginationResponse<RSPL.Contracts.Models.SampleModels.FactorySamples.SampleFactoryMobileViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = sampleData
            };
        }
        public async Task<IEnumerable<SampleViewModel>> SearchAllSampleFactoryAsync
            (string searchTerm, DateTime? fromdate, DateTime? toDate,
              string? factoryName, string? FactorySampleCategory,
            CancellationToken cancellationToken = default)
        {
            var Samples = await _repositoryManager.SampleFactoryRepository.SearchAllSampleFactoryAsync
                (searchTerm,fromdate,toDate, factoryName, FactorySampleCategory, cancellationToken);

            var SamplesData = Samples.Adapt<IEnumerable<SampleViewModel>>();

            return SamplesData;
        }
        public async Task<SampleViewModel> GetByIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleFactoryRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var Sampledata = Sample.Adapt<SampleViewModel>();

            return Sampledata;
        }
        public async Task<SampleWithImageViewModel> GetByIdWithImagesAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleFactoryRepository.GetByIdAsync(SampleId, cancellationToken);

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
  
        public async Task<SampleViewModel> CreateFactoryAsync(SampleFactoryInsertModel SampleModel, CancellationToken cancellationToken = default)
        {
            if (SampleModel.FactorySample.FactorySampleCategory == "RawMaterial")
            {
                if (SampleModel.FactorySample.SampleTakenFrom == "Godown")
                {
                 
                    SampleModel.FactorySample.VehicleNo = string.Empty;
                    SampleModel.FactorySample.InvoiceNo= string.Empty;
                    SampleModel.FactorySample.DateOfGRN = null;
                    SampleModel.FactorySample.DateOfInward = null;
                    SampleModel.FactorySample.NameOfVendorOrSupplier = string.Empty;
                }
               
                else if (SampleModel.FactorySample.SampleTakenFrom == "Vehicle")
                {
                    SampleModel.FactorySample.Godown = string.Empty;
                    SampleModel.FactorySample.StorageLocation = string.Empty;
                }

                SampleModel.FactorySample.ProductName = string.Empty;
                SampleModel.FactorySample.SKU = string.Empty;
                SampleModel.FactorySample.Shift =null;
                SampleModel.FactorySample.MixerNo = string.Empty;
                SampleModel.FactorySample.BatchNo = string.Empty;
                SampleModel.FactorySample.MachineLineNo = string.Empty;
                SampleModel.FactorySample.ShiftInchargeName = string.Empty;
                SampleModel.FactorySample.SuperviserName = string.Empty;
            }
            else
            {
                SampleModel.FactorySample.RawMaterialName = string.Empty;
                SampleModel.FactorySample.SampleTakenFrom = null;
                SampleModel.FactorySample.Godown = string.Empty;
                SampleModel.FactorySample.StorageLocation = string.Empty;
                SampleModel.FactorySample.InvoiceNo = string.Empty;
                SampleModel.FactorySample.VehicleNo = string.Empty;
              //  SampleModel.FactorySample.NoOfSample = null;
                SampleModel.FactorySample.DateOfGRN = null;
                SampleModel.FactorySample.DateOfInward = null;
                SampleModel.FactorySample.NameOfVendorOrSupplier = string.Empty;
    }
            var Sample = SampleModel.Adapt<Sample>();
            if (SampleModel.productImage != null )
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

            while (await _repositoryManager.SampleFactoryRepository.GetByUniqueCodeAsync
                (randomAlphaNumeric, cancellationToken))
            {
                randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(8); // Generate a new random alphanumeric number
            }
            #endregion
            Sample.UniqueCode = randomAlphaNumeric;

            _repositoryManager.SampleFactoryRepository.Insert(Sample);
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


        //public async Task<IList<RSPL.Contracts.Models.SampleModels.FactorySamples.SampleWithTestResultOutputDto>> GetSamplesWithTestResultsAsync(string factoryCategory, string factoryCode,DateTime startdate,DateTime endDate)
        //{
        //    var samplesWithTestResults = await _repositoryManager.SampleFactoryRepository.GetSamplesWithTestResultsAsync(factoryCategory, factoryCode,startdate,endDate);
        //    return samplesWithTestResults;
        //}

        public async Task<IList<SampleWithTestResultOutputDto>> GetSamplesWithTestResultsAsync(
    string factoryCategory, string factoryCode, DateTime startdate, DateTime endDate)
        {
            var samplesWithTestResults = await _repositoryManager.SampleFactoryRepository.GetSamplesWithTestResultsAsync(factoryCategory, factoryCode, startdate, endDate);
            return samplesWithTestResults;
        }


        public async Task<IList<FactorySampleRoutineDto>> GetSamplesWithTestDetailsAsync(string factoryCode, string factorySampleCategory, string testName, DateTime startDate, DateTime endDate)
        {
            var factorySamplesWithTestResults = await _repositoryManager.SampleFactoryRepository.GetSamplesWithTestDetailsAsync( factoryCode, factorySampleCategory, testName,  startDate, endDate);
            return factorySamplesWithTestResults;
        }

    }
}