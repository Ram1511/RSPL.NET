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
using RSPL.Contracts.Models.ModuleModels;

namespace Services
{
    internal sealed class ModuleService : IModuleService
    {
        private readonly IRepositoryManager _repositoryManager;

        public ModuleService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ModuleViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Modules = await _repositoryManager.ModuleRepository.GetAllAsync(cancellationToken);

            var ModulesData = Modules.Adapt<IEnumerable<ModuleViewModel>>();

            return ModulesData;
        }

        public async Task<ModuleViewModel> GetByIdAsync(int ModuleId, CancellationToken cancellationToken = default)
        {
            var Module = await _repositoryManager.ModuleRepository.GetByIdAsync(ModuleId, cancellationToken);

            if (Module is null)
            {
                throw new ModuleNotFoundException(ModuleId);
            }

            var ModuleData = Module.Adapt<ModuleViewModel>();

            return ModuleData;
        }

        public async Task<ModuleViewModel> CreateAsync(ModuleInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var Module = InsertModel.Adapt<Module>();

            _repositoryManager.ModuleRepository.Insert(Module);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Module.Adapt<ModuleViewModel>();
        }

        public async Task UpdateAsync( ModuleUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var Module = await _repositoryManager.ModuleRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (Module is null)
            {
                throw new ModuleNotFoundException(UpdateModel.Id);
            }

            Module.Name = UpdateModel.Name;

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int ModuleId, CancellationToken cancellationToken = default)
        {
            var Module = await _repositoryManager.ModuleRepository.GetByIdAsync(ModuleId, cancellationToken);

            if (Module is null)
            {
                throw new ModuleNotFoundException(ModuleId);
            }

            _repositoryManager.ModuleRepository.Remove(Module);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}