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
using static RSPL.Common.Enums; 
using System.Linq;
using RSPL.Contracts.Models.SampleModels.ComplaintSample;
using RSPL.Common;

namespace Services
{
    internal sealed class SampleComplaintService : ISampleComplaintService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHostingEnvironment _environment;

        public SampleComplaintService(IRepositoryManager repositoryManager,
            IHostingEnvironment environment)
        {
            _repositoryManager = repositoryManager;
            _environment = environment;
        } 
        public async Task<PaginationResponse<SampleViewModel>> GetAllSampleComplaintAsync
          (string searchTerm, DateTime? fromdate, DateTime? toDate, 
            PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleComplaintRepository
                .GetAllSampleComplaintAsync
                (searchTerm,fromdate,toDate, 
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
        public async Task<PaginationResponse<SampleComplaintMobileViewModel>> GetAllSampleComplaintMobileAsync
(string searchTerm, DateTime? fromdate, DateTime? toDate, PaginationRequest request,
CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.SampleComplaintRepository.GetAllSampleComplaintMobileAsync
                (searchTerm,fromdate,toDate, request, cancellationToken);
 
            var sampleData = response.Records.Adapt(new List<SampleComplaintMobileViewModel>());
            // Convert product images to base64 strings using paging
            for (int i = 0; i < sampleData.Count(); i++)
            {
                var base64String = await GetImageBase64Async(sampleData.ElementAt(i).ProductImageName);
                sampleData.ElementAt(i).ProductImageNameBase64 = base64String;
            }

            return new PaginationResponse<SampleComplaintMobileViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = sampleData
            };
        }
        public async Task<IEnumerable<SampleViewModel>> SearchAllSampleComplaintAsync
            (string searchTerm, DateTime? fromdate, DateTime? toDate, 
            CancellationToken cancellationToken = default)
        {
            var Samples = await _repositoryManager.SampleComplaintRepository.SearchAllSampleComplaintAsync
                (searchTerm,fromdate,toDate,  cancellationToken);

            var SamplesData = Samples.Adapt<IEnumerable<SampleViewModel>>();

            return SamplesData;
        }
        public async Task<SampleViewModel> GetByIdAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleComplaintRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var Sampledata = Sample.Adapt<SampleViewModel>();

            return Sampledata;
        }
        public async Task<SampleWithImageViewModel> GetByIdWithImagesAsync(long SampleId, CancellationToken cancellationToken = default)
        {
            var Sample = await _repositoryManager.SampleComplaintRepository.GetByIdAsync(SampleId, cancellationToken);

            if (Sample is null)
            {
                throw new SampleNotFoundException(SampleId);
            }

            var sampleData = Sample.Adapt<SampleWithImageViewModel>();
            var base64ProductString = await GetImageBase64Async(sampleData.ProductImageName);
            sampleData.ProductImageNameBase64 = base64ProductString;
            if (!string.IsNullOrEmpty(sampleData.ReferenceImageName))
            {
                var base64ReferenceString = await GetImageBase64Async(sampleData.ReferenceImageName);
                sampleData.ReferenceImageNameBase64 = base64ReferenceString;
            }
            else
                sampleData.ReferenceImageNameBase64 = null;
            return sampleData;
        }
  
        public async Task<SampleViewModel> CreateComplaintAsync(SampleComplaintInsertModel SampleModel, CancellationToken cancellationToken = default)
        {
            if (SampleModel.MarketSample.ComplaintSource != "Retailer"
                && SampleModel.MarketSample.ComplaintSource != "Consumer")
            {
                SampleModel.MarketSample.NameOfRetailer = string.Empty;
                SampleModel.MarketSample.RetailerFirmName = string.Empty;
                SampleModel.MarketSample.RetailerAddress = string.Empty;
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

            while (await _repositoryManager.SampleComplaintRepository.GetByUniqueCodeAsync
                (randomAlphaNumeric, cancellationToken))
            {
                randomAlphaNumeric = MethodHelpers.GenerateRandomAlphaNumeric(8); // Generate a new random alphanumeric number
            }
            #endregion
            Sample.UniqueCode = randomAlphaNumeric;

            _repositoryManager.SampleComplaintRepository.Insert(Sample);
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

 


    }
}