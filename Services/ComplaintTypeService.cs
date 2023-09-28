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
using RSPL.Contracts.Models.ComplaintTypeModels;

namespace Services
{
    internal sealed class ComplaintTypeService : IComplaintTypeService
    {
        private readonly IRepositoryManager _repositoryManager;

        public ComplaintTypeService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ComplaintTypeViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var ComplaintTypes = await _repositoryManager.ComplaintTypeRepository.GetAllAsync(cancellationToken);

            var ComplaintTypesData = ComplaintTypes.Adapt<IEnumerable<ComplaintTypeViewModel>>();

            return ComplaintTypesData;
        }

        public async Task<ComplaintTypeViewModel> GetByIdAsync(int ComplaintTypeId, CancellationToken cancellationToken = default)
        {
            var ComplaintType = await _repositoryManager.ComplaintTypeRepository.GetByIdAsync(ComplaintTypeId, cancellationToken);

            if (ComplaintType is null)
            {
                throw new ComplaintTypeNotFoundException(ComplaintTypeId);
            }

            var ComplaintTypeData = ComplaintType.Adapt<ComplaintTypeViewModel>();

            return ComplaintTypeData;
        }

        public async Task<ComplaintTypeViewModel> CreateAsync(ComplaintTypeInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var ComplaintType = InsertModel.Adapt<ComplaintType>();

            _repositoryManager.ComplaintTypeRepository.Insert(ComplaintType);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return ComplaintType.Adapt<ComplaintTypeViewModel>();
        }

        public async Task UpdateAsync( ComplaintTypeUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var ComplaintType = await _repositoryManager.ComplaintTypeRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (ComplaintType is null)
            {
                throw new ComplaintTypeNotFoundException(UpdateModel.Id);
            }

            ComplaintType.Name = UpdateModel.Name;

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int ComplaintTypeId, CancellationToken cancellationToken = default)
        {
            var ComplaintType = await _repositoryManager.ComplaintTypeRepository.GetByIdAsync(ComplaintTypeId, cancellationToken);

            if (ComplaintType is null)
            {
                throw new ComplaintTypeNotFoundException(ComplaintTypeId);
            }

            _repositoryManager.ComplaintTypeRepository.Remove(ComplaintType);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}