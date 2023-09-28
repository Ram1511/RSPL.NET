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
using RSPL.Contracts.Models.UserModels;
using RSPL.Domain.Models;
using RSPL.Domain.Views;

namespace Services
{
    internal sealed class UserService : IUserService
    {
        private readonly IRepositoryManager _repositoryManager;

        public UserService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var Users = await _repositoryManager.UserRepository.GetAllAsync(cancellationToken);

            var UsersData = Users.Adapt<IEnumerable<UserViewModel>>();

            return UsersData;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllWithoutRoleAsync(CancellationToken cancellationToken = default)
        {
            var Users = await _repositoryManager.UserRepository.GetAllWithoutRoleAsync(cancellationToken);

            var UsersData = Users.Adapt<IEnumerable<UserViewModel>>();

            return UsersData;
        }
        public async Task<IEnumerable<UserWithRoleViewModel>> GetAllWithRoleAsync(CancellationToken cancellationToken = default)
        {
            var Users = await _repositoryManager.UserRepository.GetAllWithRoleAsync(cancellationToken);

            var UsersData = Users.Adapt<IEnumerable<UserWithRoleViewModel>>();

            return UsersData;
        }

        public async Task<UserViewModel> SearchWithoutRoleAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var user = await _repositoryManager.UserRepository.SearchWithoutRoleAsync( searchTerm, cancellationToken);

            var userData = user.Adapt<UserViewModel>();

            return userData;
        }

        public async Task<IEnumerable<UserWithRoleViewModel>> SearchWithRoleAsync(string? searchTerm, bool? isActive, int? roleId, CancellationToken cancellationToken = default)
        {
            var Users = await _repositoryManager.UserRepository.SearchWithRoleAsync(searchTerm,isActive,roleId,cancellationToken);

            var UsersData = Users.Adapt<IEnumerable<UserWithRoleViewModel>>();

            return UsersData;
        }
        public async Task<UserViewModel> GetByIdAsync(int UserId, CancellationToken cancellationToken = default)
        {
            var User = await _repositoryManager.UserRepository.GetByIdAsync(UserId, cancellationToken);

            if (User is null)
            {
                throw new UserNotFoundException(UserId);
            }

            var Userdata = User.Adapt<UserViewModel>();

            return Userdata;
        }
        public async Task<UserWithRoleAndPermissionViewModel> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            var User = await _repositoryManager.UserRepository.GetUserProfileAsync(userId, cancellationToken);

            if (User is null)
            {
                throw new UserNotFoundException(userId);
            }

            var Userdata = User.Adapt<UserWithRoleAndPermissionViewModel>();

            return Userdata;
        }

        public async Task<UserWithRoleViewModel> GetUserRoleByIdAsync(int UserId, CancellationToken cancellationToken = default)
        {
            var User = await _repositoryManager.UserRepository.GetUserRoleByIdAsync(UserId, cancellationToken);

            if (User is null)
            {
                throw new UserNotFoundException(UserId);
            }

            var Userdata = User.Adapt<UserWithRoleViewModel>();

            return Userdata;
        }
       #region Paging

    
        public async Task<PaginationResponse<UserWithRoleViewModel>> GetWithRoleAsync(string searchTerm,
            bool? isActive,int? roleId, PaginationRequest request, 
            CancellationToken cancellationToken = default)
        {
             var response = await _repositoryManager.UserRepository.GetWithRoleAsync
                (searchTerm,isActive,roleId, request, cancellationToken);
            var userData = response.Records.Adapt
                       <IEnumerable<UserWithRoleViewModel>>();

            return new PaginationResponse<UserWithRoleViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = userData
            };
        }

        public async Task<PaginationResponse<UserView>> GetWithoutRoleAsync(string searchTerm, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.UserRepository.GetWithoutRoleAsync
                     (searchTerm, request, cancellationToken);
            var userData = response.Records.Adapt
                    <IEnumerable<UserView>>();

            return new PaginationResponse<UserView>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = userData
            };
        }

      
        #endregion
    }
}