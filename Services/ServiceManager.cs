using System;
using Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using RSPL.Services.Abstractions;

namespace Services
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<ILabService> _lazyLabService;
        private readonly Lazy<INotificationService> _lazyNotificationService;
        private readonly Lazy<IProductService> _lazyProductService;
        private readonly Lazy<IProductSKUService> _lazyProductSKUService;
        private readonly Lazy<IFactoryService> _lazyFactoryService;
         private readonly Lazy<ISampleFactoryService> _lazySampleFactoryService;
        private readonly Lazy<ISampleMarketService> _lazySampleMarketService;
        private readonly Lazy<ISampleComplaintService> _lazySampleComplaintService;
        private readonly Lazy<ISampleService> _lazySampleService;
        //  private readonly Lazy<ISampleDashboardService> _lazySampleDashboardService;
        private readonly Lazy<IFactoryDashboardService> _lazyFactoryDashboardService;
        //  private readonly Lazy<ISampleReceiverDashboardService> _lazySampleReceiverDashboardService;
        private readonly Lazy<ISampleResultService> _lazySampleResultService;
        private readonly Lazy<ITestService> _lazyTestService;
        private readonly Lazy<IUserService> _lazyUserService;

        private readonly Lazy<IRoleService> _lazyRoleService;
        private readonly Lazy<IModuleService> _lazyModuleService;
        private readonly Lazy<IComplaintTypeService> _lazyComplaintTypeService;
        private readonly Lazy<IUserRoleService> _lazyUserRoleService;
        private readonly Lazy<IRoleModulePermissionService> _lazyRoleModulePermissionService;
        public ServiceManager(IRepositoryManager repositoryManager, IHostingEnvironment environment)
        {
            _lazyLabService = new Lazy<ILabService>(() => new LabService(repositoryManager));
            _lazyNotificationService = new Lazy<INotificationService>(() => new NotificationService(repositoryManager));
            _lazyProductService = new Lazy<IProductService>(() => new ProductService(repositoryManager));
            _lazyProductSKUService = new Lazy<IProductSKUService>(() => new ProductSKUService(repositoryManager));
            _lazyFactoryService = new Lazy<IFactoryService>(() => new FactoryService(repositoryManager));
        
            _lazySampleFactoryService = new Lazy<ISampleFactoryService>(() => new SampleFactoryService
           (repositoryManager, environment));
            _lazySampleMarketService = new Lazy<ISampleMarketService>(() => new SampleMarketService
           (repositoryManager, environment));
            _lazySampleComplaintService = new Lazy<ISampleComplaintService>(() => new SampleComplaintService
         (repositoryManager, environment));

            _lazySampleService = new Lazy<ISampleService>(() => new SampleService
            (repositoryManager, environment, new SampleResultService
        (repositoryManager, environment)));

            //   _lazySampleDashboardService = new Lazy<ISampleDashboardService>(() => new SampleDashboardService
            //(repositoryManager, environment));
            _lazyFactoryDashboardService = new Lazy<IFactoryDashboardService>(() => new FactoryDashboardService
     (repositoryManager, environment));
            //          _lazySampleReceiverDashboardService = new Lazy<ISampleReceiverDashboardService>(() => new SampleReceiverDashboardService
            //(repositoryManager, environment));
            _lazySampleResultService = new Lazy<ISampleResultService>(() => new SampleResultService
        (repositoryManager, environment));
            _lazyTestService = new Lazy<ITestService>(() => new TestService(repositoryManager));
            _lazyUserService = new Lazy<IUserService>(() => new UserService(repositoryManager));

            _lazyRoleService = new Lazy<IRoleService>(() => 
            new RoleService(repositoryManager));

            _lazyUserRoleService = new Lazy<IUserRoleService>(() => 
            new UserRoleService(repositoryManager));

            _lazyModuleService = new Lazy<IModuleService>(() => 
            new ModuleService(repositoryManager));

            _lazyComplaintTypeService = new Lazy<IComplaintTypeService>(() =>
new ComplaintTypeService(repositoryManager));
            _lazyRoleModulePermissionService = new Lazy<IRoleModulePermissionService>(() =>
            new RoleModulePermissionService(repositoryManager));


        }
        public ILabService LabService
        {
            get
            {
                return _lazyLabService.Value;
            }
        }
        public INotificationService NotificationService => _lazyNotificationService.Value;
        public IFactoryService FactoryService => _lazyFactoryService.Value;
        public IProductService ProductService => _lazyProductService.Value;
        public IProductSKUService ProductSKUService => _lazyProductSKUService.Value;
        public ISampleFactoryService SampleFactoryService => _lazySampleFactoryService.Value;
        public ISampleMarketService SampleMarketService => _lazySampleMarketService.Value;
        public ISampleComplaintService SampleComplaintService => _lazySampleComplaintService.Value;
        public ISampleService SampleService => _lazySampleService.Value;
        //public ISampleDashboardService SampleDashboardService => _lazySampleDashboardService.Value;
        public IFactoryDashboardService FactoryDashboardService => _lazyFactoryDashboardService.Value;
        //  public ISampleReceiverDashboardService SampleReceiverDashboardService => _lazySampleReceiverDashboardService.Value;
        public ISampleResultService SampleResultService => _lazySampleResultService.Value;
        public ITestService TestService => _lazyTestService.Value;
        public IUserService UserService => _lazyUserService.Value;

        public IRoleService RoleService => _lazyRoleService.Value;
        public IModuleService ModuleService => _lazyModuleService.Value;
        public IComplaintTypeService ComplaintTypeService => _lazyComplaintTypeService.Value;
        public IUserRoleService UserRoleService => _lazyUserRoleService.Value;
        public IRoleModulePermissionService RoleModulePermissionService =>
            _lazyRoleModulePermissionService.Value;
    }
}
