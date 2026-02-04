
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);




IResourceBuilder<ProjectResource> authApi = builder.AddProject<Projects.UnifiedOnboarding_Auth_Bff>("UnifiedOnboarding-Auth-Api");
IResourceBuilder<ProjectResource> registrationApi = builder.AddProject<Projects.UnifiedOnboarding_Registration_Bff>("UnifiedOnboarding-Registration-Api");


builder.Build().Run();
