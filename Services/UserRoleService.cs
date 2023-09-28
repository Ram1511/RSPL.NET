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
using RSPL.Contracts.Models.UserRoleModels;

namespace Services
{
    internal sealed class UserRoleService : IUserRoleService
    {
        private readonly IRepositoryManager _repositoryManager;

        public UserRoleService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<UserRoleViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var UserRoles = await _repositoryManager.UserRoleRepository.GetAllAsync(cancellationToken);

            var UserRolesData = UserRoles.Adapt<IEnumerable<UserRoleViewModel>>();

            return UserRolesData;
        }

        public async Task<UserRoleViewModel> GetByIdAsync(int UserRoleId, CancellationToken cancellationToken = default)
        {
            var UserRole = await _repositoryManager.UserRoleRepository.GetByIdAsync(UserRoleId, cancellationToken);

            if (UserRole is null)
            {
                throw new UserRoleNotFoundException(UserRoleId);
            }

            var UserRoleData = UserRole.Adapt<UserRoleViewModel>();

            return UserRoleData;
        }

        public async Task<UserRoleViewModel> CreateAsync(UserRoleInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var UserRole = InsertModel.Adapt<UserRole>();

            _repositoryManager.UserRoleRepository.Insert(UserRole);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return UserRole.Adapt<UserRoleViewModel>();
        }

        public async Task UpdateAsync( UserRoleUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var UserRole = await _repositoryManager.UserRoleRepository.GetByUserIdAsync(UpdateModel.UserId, cancellationToken);

            if (UserRole is null)
            {
                throw new UserRoleNotFoundException(UpdateModel.UserId);
            }

            UserRole.RoleId = UpdateModel.RoleId;
            UserRole.UserId = UpdateModel.UserId; 
            UserRole.ModifiedDate= UpdateModel.ModifiedDate;    
            UserRole.ModifiedBy= UpdateModel.ModifiedBy;    
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int UserRoleId, CancellationToken cancellationToken = default)
        {
            var UserRole = await _repositoryManager.UserRoleRepository.GetByIdAsync(UserRoleId, cancellationToken);

            if (UserRole is null)
            {
                throw new UserRoleNotFoundException(UserRoleId);
            }

            _repositoryManager.UserRoleRepository.Remove(UserRole);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}