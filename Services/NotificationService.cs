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
using RSPL.Contracts.Models.NotificationModels;
using RSPL.Domain;
using System.Linq;

namespace Services
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly IRepositoryManager _repositoryManager;

        public NotificationService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetAllAsync(IEnumerable<int?> roleIds, CancellationToken cancellationToken = default)
        {
            var Notifications = await _repositoryManager.NotificationRepository.GetAllAsync(roleIds, cancellationToken);

            var NotificationsData = Notifications.Adapt<IEnumerable<NotificationViewModel>>();

            return NotificationsData;
        }
        public async Task<IEnumerable<NotificationViewModel>> GetAllMobileAsync(string DeviceToken, CancellationToken cancellationToken = default)
        {
            var Notifications = await _repositoryManager.NotificationRepository.GetAllMobileAsync(DeviceToken,cancellationToken);

            var NotificationsData = Notifications.Adapt<IEnumerable<NotificationViewModel>>();

            return NotificationsData;
        }
        public async Task<PaginationResponse<NotificationViewModel>> GetAllAsync
    (string searchTerm, PaginationRequest request,
    CancellationToken cancellationToken = default)
        {
            var response = await _repositoryManager.NotificationRepository.GetAllAsync
                (searchTerm, request, cancellationToken);
            var NotificationData = response.Records.Adapt
                <IEnumerable<NotificationViewModel>>();

            return new PaginationResponse<NotificationViewModel>
            {
                TotalRecords = response.TotalRecords,
                TotalPages = response.TotalPages,
                Records = NotificationData
            };
        }
        public async Task<IEnumerable<NotificationViewModel>> SearchAllAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var Notifications = await _repositoryManager.NotificationRepository.SearchAllAsync(searchTerm, cancellationToken);

            var NotificationsData = Notifications.Adapt<IEnumerable<NotificationViewModel>>();

            return NotificationsData;
        }
        public async Task<NotificationViewModel> GetByIdAsync(int NotificationId, CancellationToken cancellationToken = default)
        {
            var Notification = await _repositoryManager.NotificationRepository.GetByIdAsync(NotificationId, cancellationToken);

            if (Notification is null)
            {
                throw new NotificationNotFoundException(NotificationId);
            }

            var NotificationData = Notification.Adapt<NotificationViewModel>();

            return NotificationData;
        }

        public async Task<NotificationViewModel> CreateAsync(NotificationInsertModel InsertModel, CancellationToken cancellationToken = default)
        {
            var Notification = InsertModel.Adapt<Notification>();

            _repositoryManager.NotificationRepository.Insert(Notification);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Notification.Adapt<NotificationViewModel>();
        }
        
            public async Task UpdateMobileAsync(NotificationMobileUpdateModel UpdateMobileModel, CancellationToken cancellationToken = default)
        {
            var Notifications = await _repositoryManager.NotificationRepository.
                GetAllMobileAsync(UpdateMobileModel.DeviceToken, cancellationToken);

            if (Notifications is null)
            {
                throw new Exception("Notification not found");
            }

            foreach (var notification in Notifications)
            {
                notification.IsActive = UpdateMobileModel.IsActive;
                notification.ModifiedBy = UpdateMobileModel.ModifiedBy;
                notification.ModifiedDate= UpdateMobileModel.ModifiedDate;

            }
            
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateAsync( NotificationUpdateModel UpdateModel, CancellationToken cancellationToken = default)
        {
            var Notification = await _repositoryManager.NotificationRepository.GetByIdAsync(UpdateModel.Id, cancellationToken);

            if (Notification is null)
            {
                throw new NotificationNotFoundException(UpdateModel.Id);
            }

            Notification.IsActive = UpdateModel.IsActive; 

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateByRoleIdAsync(NotificationUpdateByRoleIdModel UpdateByRoleIdModel, CancellationToken cancellationToken = default)
        {
            var Notification = await _repositoryManager.NotificationRepository.GetByRoleIdAsync(UpdateByRoleIdModel.RoleId, cancellationToken);

            if (Notification is null)
            {
                throw new Exception("Notification not found with RoleId "+ UpdateByRoleIdModel.RoleId);
            }
            foreach (var item in Notification)
            {
                item.IsActive = false;
                item.ModifiedBy=UpdateByRoleIdModel.ModifiedBy;
                item.ModifiedDate = UpdateByRoleIdModel.ModifiedDate;
            } 

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync(int NotificationId, CancellationToken cancellationToken = default)
        {
            var Notification = await _repositoryManager.NotificationRepository.GetByIdAsync(NotificationId, cancellationToken);

            if (Notification is null)
            {
                throw new NotificationNotFoundException(NotificationId);
            }

            _repositoryManager.NotificationRepository.Remove(Notification);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}