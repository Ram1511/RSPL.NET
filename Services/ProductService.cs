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
using RSPL.Contracts.Models.ProductModels;
using RSPL.Domain;
using RSPL.Domain.Views;

namespace Services
{
    internal sealed class ProductService : IProductService
    {
        private readonly IRepositoryManager _repositoryManager;

        public ProductService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Products = await _repositoryManager.ProductRepository.GetAllAsync(cancellationToken);

            var ProductsData = Products.Adapt<IEnumerable<ProductViewModel>>();

            return ProductsData;
        }
        public async Task<PaginationResponse<ProductViewModel>> GetAllAsync
    (string searchTerm, PaginationRequest request,
    CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.ProductRepository.GetAllAsync
                (searchTerm, request, cancellationToken);
            var ProductData = response.Records.Adapt
                <IEnumerable<ProductViewModel>>();

            return new PaginationResponse<ProductViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = ProductData
            };
        }
        public async Task<IEnumerable<ProductViewModel>> SearchAllAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var Products = await _repositoryManager.ProductRepository.SearchAllAsync(searchTerm,cancellationToken);

            var ProductsData = Products.Adapt<IEnumerable<ProductViewModel>>();

            return ProductsData;
        }
        public async Task<IEnumerable<ProductViewModel>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            var Products = await _repositoryManager.ProductRepository.GetByCategoryAsync(category, cancellationToken);

            var ProductsData = Products.Adapt<IEnumerable<ProductViewModel>>();

            return ProductsData;
        }
        public async Task<IEnumerable<ProductViewModel>> GetByCategoryAndBrandNameAsync(string category,string brandName, CancellationToken cancellationToken = default)
        {
            var Products = await _repositoryManager.ProductRepository.GetByCategoryAndBrandNameAsync(category, brandName, cancellationToken);

            var ProductsData = Products.Adapt<IEnumerable<ProductViewModel>>();

            return ProductsData;
        }
        
        public async Task<ProductViewModel> GetByIdAsync(int ProductId, CancellationToken cancellationToken = default)
        {
            var Product = await _repositoryManager.ProductRepository.GetByIdAsync(ProductId, cancellationToken);

            if (Product is null)
            {
                throw new ProductNotFoundException(ProductId);
            }

            var Productdata = Product.Adapt<ProductViewModel>();

            return Productdata;
        }
 }
}