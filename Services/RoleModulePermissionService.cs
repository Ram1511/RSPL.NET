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
using RSPL.Contracts.Models.RoleModulePermissionModels;

namespace Services
{
    internal sealed class RoleModulePermissionService : IRoleModulePermissionService
    {
        private readonly IRepositoryManager _repositoryManager;

        public RoleModulePermissionService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<RoleModulePermissionViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var RoleModulePermissions = await _repositoryManager.RoleModulePermissionRepository.GetAllAsync(cancellationToken);

            var RoleModulePermissionsData = RoleModulePermissions.Adapt<IEnumerable<RoleModulePermissionViewModel>>();

            return RoleModulePermissionsData;
        }

        public async Task<IEnumerable<RoleModulePermissionViewModel>> GetByRoleIdAsync(int roleId,CancellationToken cancellationToken = default)
        {
            var RoleModulePermissions = await _repositoryManager.RoleModulePermissionRepository.GetByRoleIdAsync(roleId,cancellationToken);

            var RoleModulePermissionsData = RoleModulePermissions.Adapt<IEnumerable<RoleModulePermissionViewModel>>();

            return RoleModulePermissionsData;
        }

        public async Task<RoleModulePermissionViewModel> GetByIdAsync(int RoleModulePermissionId, CancellationToken cancellationToken = default)
        {
            var RoleModulePermission = await _repositoryManager.RoleModulePermissionRepository.GetByIdAsync(RoleModulePermissionId, cancellationToken);

            if (RoleModulePermission is null)
            {
                throw new RoleModulePermissionNotFoundException(RoleModulePermissionId);
            }

            var RoleModulePermissionData = RoleModulePermission.Adapt<RoleModulePermissionViewModel>();

            return RoleModulePermissionData;
        }

        public async Task<RoleModulePermissionViewModel> CreateAsync(RoleModulePermissionInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var RoleModulePermission = InsertModel.Adapt<RoleModulePermission>();

            _repositoryManager.RoleModulePermissionRepository.Insert(RoleModulePermission);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return RoleModulePermission.Adapt<RoleModulePermissionViewModel>();
        }

        public async Task UpdateAsync( RoleModulePermissionUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var RoleModulePermission = await _repositoryManager.RoleModulePermissionRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (RoleModulePermission is null)
            {
                throw new RoleModulePermissionNotFoundException(UpdateModel.Id);
            }

            RoleModulePermission.CanView = UpdateModel.CanView;
            RoleModulePermission.CanAdd = UpdateModel.CanAdd;
            RoleModulePermission.CanEdit = UpdateModel.CanView;
            RoleModulePermission.CanDelete = UpdateModel.CanView;
            RoleModulePermission.CanExport = UpdateModel.CanView;
          
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task InsertOrUpdateAsync(IEnumerable<RoleModulePermissionUpdateModel> roleModulePermissions ,
            CancellationToken cancellationToken = default)
        {
            foreach (var roleModulePermission in roleModulePermissions)
            {
                // Check if the RoleModulePermission object already exists in the database
                var existingRoleModulePermission =  await _repositoryManager.RoleModulePermissionRepository.GetByIdAsync(roleModulePermission.Id, cancellationToken);
                    if (existingRoleModulePermission != null)
                {
                    // Update the existing RoleModulePermission object
                    existingRoleModulePermission.CanView = roleModulePermission.CanView;
                    existingRoleModulePermission.CanAdd = roleModulePermission.CanAdd;
                    existingRoleModulePermission.CanEdit = roleModulePermission.CanEdit;
                    existingRoleModulePermission.CanDelete = roleModulePermission.CanDelete;
                    existingRoleModulePermission.CanExport = roleModulePermission.CanExport;
                    _repositoryManager.RoleModulePermissionRepository.Update(existingRoleModulePermission);
                }
                else
                {
                    // Insert the new RoleModulePermission object
                    var newRoleModulePermission = new RoleModulePermission
                    {
                        RoleId = roleModulePermission.RoleId,
                        ModuleId = roleModulePermission.ModuleId,
                        CanView = roleModulePermission.CanView,
                        CanAdd = roleModulePermission.CanAdd,
                        CanEdit = roleModulePermission.CanEdit,
                        CanDelete = roleModulePermission.CanDelete,
                        CanExport = roleModulePermission.CanExport
                    };
                    _repositoryManager.RoleModulePermissionRepository.Insert(newRoleModulePermission);
                }
            }

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }


        public async Task DeleteAsync(int RoleModulePermissionId, CancellationToken cancellationToken = default)
        {
            var RoleModulePermission = await _repositoryManager.RoleModulePermissionRepository.GetByIdAsync(RoleModulePermissionId, cancellationToken);

            if (RoleModulePermission is null)
            {
                throw new RoleModulePermissionNotFoundException(RoleModulePermissionId);
            }

            _repositoryManager.RoleModulePermissionRepository.Remove(RoleModulePermission);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}