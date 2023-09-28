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
using RSPL.Contracts.Models.TestModels;
using RSPL.Domain;
using System;

namespace Services
{
    internal sealed class TestService : ITestService
    {
        private readonly IRepositoryManager _repositoryManager;

        public TestService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }
        public async Task<IEnumerable<TestWithDetailsModel>> DownloadAsync(CancellationToken cancellationToken = default)
        {
            return await _repositoryManager.TestRepository.DownloadAsync(cancellationToken);
        }
        public async Task<IEnumerable<TestWithDetailsModel>> SearchAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            return await _repositoryManager.TestRepository.SearchAllAsync(searchTerm, cancellationToken);
        }
        public async Task<IEnumerable<TestBaseModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Tests = await _repositoryManager.TestRepository.GetAllAsync(cancellationToken);

            var TestsData = Tests.Adapt<IEnumerable<TestBaseModel>>();

            return TestsData;
        }
        public async Task<PaginationResponse<TestWithDetailsModel>> GetAllAsync
(string searchTerm, PaginationRequest request,
CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.TestRepository.GetAllAsync
                (searchTerm, request, cancellationToken);
            var TestData = response.Records.Adapt
                <IEnumerable<TestWithDetailsModel>>();

            return new PaginationResponse<TestWithDetailsModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = TestData
            };
        }

        public async Task<TestBaseModel> GetByIdAsync(int TestId, CancellationToken cancellationToken = default)
        {
            var Test = await _repositoryManager.TestRepository.GetByIdAsync(TestId, cancellationToken);

            if (Test is null)
            {
                throw new TestNotFoundException(TestId);
            }

            var TestData = Test.Adapt<TestBaseModel>();

            return TestData;
        }
        
            public async Task<TestBaseModel> GetByCategoryAsync
                 (int TestId, string TestCategory, CancellationToken cancellationToken = default)
        {
            var Test = await _repositoryManager.TestRepository.GetByCategoryAsync(TestId, TestCategory, cancellationToken);

            if (Test is null)
            {
                throw new Exception("Test not found with Id " + TestId);
            }

            var TestData = Test.Adapt<TestBaseModel>();

            return TestData;
        }
        public async Task<IEnumerable<TestDetailModel>> GetLastTestDetailByProductNameAsync
                 (string ProductName, CancellationToken cancellationToken = default)
        {
            var testdetails = await _repositoryManager.TestRepository
                .GetLastTestDetailByProductNameAsync(ProductName, cancellationToken);

            if (testdetails is null)
            {
                throw new Exception("Test not found with ProductName " + ProductName);
            }

            var TestData = testdetails.Adapt
                <IEnumerable<TestDetailModel>>();

            return TestData;
        }
        public async Task<IEnumerable<TestDetailModel>> GetLastTestDetailByProductNameAndBrandNameAsync
         (string ProductName, string BrandName, CancellationToken cancellationToken = default)
        {
            var testdetails = await _repositoryManager.TestRepository
                .GetLastTestDetailByProductNameAndBrandNameAsync(ProductName,BrandName, cancellationToken);

            if (testdetails is null)
            {
                throw new Exception("Test not found with ProductName " + ProductName);
            }

            var TestData = testdetails.Adapt
                <IEnumerable<TestDetailModel>>();

            return TestData;
        }
        public async Task<TestBaseModel> GetByProductNameAsync
     (string ProductName,CancellationToken cancellationToken = default)
        {
            var Test = await _repositoryManager.TestRepository
                .GetByProductNameAsync(ProductName, cancellationToken);

            if (Test is null)
            {
                throw new Exception("Test not found with ProductName " + ProductName);
            }

            var TestData = Test.Adapt<TestBaseModel>();

            return TestData;
        }
        public async Task<TestBaseModel> GetByProductNameAsync
         (string ProductName, string TestCategory, CancellationToken cancellationToken = default)
        {
            var Test = await _repositoryManager.TestRepository.GetByProductNameAsync(ProductName, TestCategory, cancellationToken);

            if (Test is null)
            {
                throw new Exception("Test not found with ProductName " + ProductName);
            }

            var TestData = Test.Adapt<TestBaseModel>();

            return TestData;
        }

        public async Task<TestBaseModel> CreateAsync(TestInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var result = new AppResponse();
            var Test = InsertModel.Adapt<Test>();
            var validate_result = ValidateTestEntity(Test);
            if (validate_result.Succeeded != true)
            {
                result.Message = validate_result.Message;
                //return result;
            }
            //if (InsertModel.TestDetails != null)
            //{
            //    if (InsertModel.TestDetails.Where(td => td.Routine).Count() == 0)
            //    {
            //        throw new Exception("There must be one Routine Test in Test Details!");
            //    }
            //    if (InsertModel.TestDetails.Where(td => td.Regular).Count() == 0)
            //    {
            //        throw new Exception("There must be one Regular Test in Test Details!");
            //    }
            //    if (InsertModel.TestDetails.Where(td => td.Complete).Count() == 0)
            //    {
            //        throw new Exception("There must be one Complete Test in Test Details!");
            //    }
            //}
            //else
            //{
            //    throw new Exception("Test Detail list can't be empty.");
            //}
            ValidateTestDetails(InsertModel.TestDetails);
            _repositoryManager.TestRepository.Insert(Test);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Test.Adapt<TestBaseModel>();
        }

        public async Task UpdateAsync( TestUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var result = new AppResponse();
            var entityItem = await _repositoryManager.TestRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);
          
            if (entityItem is null)
            {
                throw new TestNotFoundException(UpdateModel.Id);
            }
            var testModel = UpdateModel.Adapt<Test>();
            var validate_result = ValidateTestEntity(testModel);
            if (validate_result.Succeeded != true)
            {
                result.Message = validate_result.Message;
                //return result;
            }
            #region set_Test_Details
            var dbTestDetailIds = entityItem.TestDetails?.Select(x => x.Id);
            var newTestDetails = UpdateModel.TestDetails?.Where(x => x.Id <= 0);

                var updatedTestDetailIds = UpdateModel.TestDetails?.Where(x => x.Id > 0)?
                    .Select(x => x.Id)?.ToList();
                //var deletedTestDetailIds = dbTestDetailIds.Except(updatedTestDetailIds);

                //var deletedTestDetails = entityItem.TestDetails?
                //    .Where(x => deletedTestDetailIds.Contains(x.Id));
                //if (deletedTestDetails?.Any() == true)
                //    this._TestDetailEntities.RemoveRange(deletedTestDetails);
                //entityItem.TestDetails.RemoveRange(deletedTestDetails);

                #region Add_new_TestDetails
                if (newTestDetails?.Any() == true)
                {
                    var setnewTestDetailsResult = SetNewTestDetails(entityItem, newTestDetails);
                    if (setnewTestDetailsResult?.Succeeded != true)
                    {
                       // return setnewTestDetailsResult;
                    }
                }
                #endregion

                #region set_TestDetails
                if (updatedTestDetailIds?.Any() == true)
                {
                    var setUpdatedTestDetailsResult = SetUpdatedTestDetails(entityItem, UpdateModel);
                    if (setUpdatedTestDetailsResult?.Succeeded != true)
                    {
                       // return setUpdatedTestDetailsResult;
                    }
                }
            #endregion
            #endregion

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int TestId, CancellationToken cancellationToken = default)
        {
            var Test = await _repositoryManager.TestRepository.GetByIdAsync(TestId, cancellationToken);

            if (Test is null)
            {
                throw new TestNotFoundException(TestId);
            }

            _repositoryManager.TestRepository.Remove(Test);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        #region extension_methods
        private AppResponse SetNewTestDetails(Test entityItem,
      IEnumerable<TestDetailModel> TestDetailModels)
        {
            var result = new AppResponse();

            foreach (var item in TestDetailModels)
            {
                var TestDetailNormalEntity = item.ToTestDetailEntity();
                entityItem.TestDetails.Add(TestDetailNormalEntity);


            }
            result.Succeeded = true;
            return result;
        }
        private AppResponse SetUpdatedTestDetails(Test entityItem,
   TestUpdateModel UpdateModel)
        {
            var result = new AppResponse();
            foreach (var updatedTestDetail in UpdateModel.TestDetails?.
                Where(x => x.Id > 0))
            {
                var TestDetailEntityItem = entityItem.TestDetails.
                    Where(x => x.Id == updatedTestDetail.Id).FirstOrDefault();

                if (TestDetailEntityItem == null)
                {
                    //ToDo: error
                    result.Message = "updatedTestDetail doesn't exists";
                    return result;
                }

                TestDetailEntityItem.UpdateToTestDetailEntity( updatedTestDetail);


            }
            result.Succeeded = true;
            return result;

        }
        #endregion
        #region Validate_Methods

        private AppResponse ValidateTestEntity(Test item)
        {
            var result = new AppResponse();

            if (!EnumHelperMethods.EnumContainValue<BrandName>(item.BrandName))
            {
                result.Message = "BrandName value is not valid";
                return result;
            }
            if (!EnumHelperMethods.EnumContainValue<Category>(item.Category))
                {
                    result.Message = "Category value is not valid";
                    return result;
                }            
    
                if (!EnumHelperMethods.EnumContainValue<ProductType>(item.ProductType))
                {
                    result.Message = "ProductType value is not valid";
                    return result;
                }
            
            result.Succeeded = true;
            return result;
        }
        private static void ValidateTestDetails(ICollection<TestDetailModel> testDetails)
        {
            if (testDetails == null )
            {
                throw new Exception("Test Detail list can't be empty.");
            }

            if (testDetails.Any(td => td.TestType == null))
            {
                throw new Exception("Test Type can't be empty.");
            }

            foreach (var testDetail in testDetails)
            {
                if (!EnumHelperMethods.EnumContainValue<TestType>(testDetail.TestType))
                {
                    throw new Exception("Invalid Test Type value.");
                }
            }
            if (!testDetails.Any(td => td.Routine||td.Regular||td.Complete))
            {
                throw new Exception("There must be one Test in Test Details!");
            }

            //if (!testDetails.Any(td => td.Routine))
            //{
            //    throw new Exception("There must be one Routine Test in Test Details!");
            //}

            //if (!testDetails.Any(td => td.Regular))
            //{
            //    throw new Exception("There must be one Regular Test in Test Details!");
            //}

            //if (!testDetails.Any(td => td.Complete))
            //{
            //    throw new Exception("There must be one Complete Test in Test Details!");
            //}
            if (testDetails.Any(td => td.TestType == TestType.Quantitative.ToString() && td.LowerLimit >= td.UpperLimit))
            {
                throw new Exception("Test Lower Limit must be less than Upper Limit!");
            }
        }


        #endregion
    }
}