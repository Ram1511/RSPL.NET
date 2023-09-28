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
using RSPL.Contracts.Models.ProductSKUModels;
using RSPL.Domain.Views;

namespace Services
{
    internal sealed class ProductSKUService : IProductSKUService
    {
        private readonly IRepositoryManager _repositoryManager;

        public ProductSKUService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ProductSKUViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var ProductSKUs = await _repositoryManager.ProductSKURepository.GetAllAsync(cancellationToken);

            var ProductSKUsData = ProductSKUs.Adapt<IEnumerable<ProductSKUViewModel>>();

            return ProductSKUsData;
        }
        public async Task<IEnumerable<ProductSKUViewModel>> GetByProductIdAsync(int ProductId, CancellationToken cancellationToken = default)
        {
            var ProductSKU = await _repositoryManager.ProductSKURepository.GetByProductIdAsync(ProductId, cancellationToken);

            if (ProductSKU is null)
            {
                throw new ItemNotFoundException("ProductId", ProductId.ToString());

            }

            var ProductSKUdata = ProductSKU.Adapt<IEnumerable<ProductSKUViewModel>>();

            return ProductSKUdata;
        }
        public async Task<ProductSKUViewModel> GetByIdAsync(int ProductSKUId, CancellationToken cancellationToken = default)
        {
            var ProductSKU = await _repositoryManager.ProductSKURepository.GetByIdAsync(ProductSKUId, cancellationToken);

            if (ProductSKU is null)
            {
                throw new ItemNotFoundException("ProductSKU", ProductSKUId.ToString());

            }

            var ProductSKUdata = ProductSKU.Adapt<ProductSKUViewModel>();

            return ProductSKUdata;
        }
 }
}