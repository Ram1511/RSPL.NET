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
using RSPL.Contracts.Models.RoleModels;
using RSPL.Domain;

namespace Services
{
    internal sealed class RoleService : IRoleService
    {
        private readonly IRepositoryManager _repositoryManager;

        public RoleService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<RoleViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Roles = await _repositoryManager.RoleRepository.GetAllAsync(cancellationToken);

            var RolesData = Roles.Adapt<IEnumerable<RoleViewModel>>();

            return RolesData;
        }

        public async Task<PaginationResponse<RoleViewModel>> GetAllAsync
    (string searchTerm, PaginationRequest request,
    CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.RoleRepository.GetAllAsync
                (searchTerm, request, cancellationToken);
            var RoleData = response.Records.Adapt
                <IEnumerable<RoleViewModel>>();

            return new PaginationResponse<RoleViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = RoleData
            };
        }
        public async Task<RoleViewModel> GetByIdAsync(int RoleId, CancellationToken cancellationToken = default)
        {
            var Role = await _repositoryManager.RoleRepository.GetByIdAsync(RoleId, cancellationToken);

            if (Role is null)
            {
                throw new RoleNotFoundException(RoleId);
            }

            var RoleData = Role.Adapt<RoleViewModel>();

            return RoleData;
        }

        public async Task<RoleViewModel> CreateAsync(RoleInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var Role = InsertModel.Adapt<Role>();

            _repositoryManager.RoleRepository.Insert(Role);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Role.Adapt<RoleViewModel>();
        }

        public async Task UpdateAsync( RoleUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var Role = await _repositoryManager.RoleRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (Role is null)
            {
                throw new RoleNotFoundException(UpdateModel.Id);
            }

            Role.Name = UpdateModel.Name;
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int RoleId, CancellationToken cancellationToken = default)
        {
            var Role = await _repositoryManager.RoleRepository.GetByIdAsync(RoleId, cancellationToken);

            if (Role is null)
            {
                throw new RoleNotFoundException(RoleId);
            }

            _repositoryManager.RoleRepository.Remove(Role);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}