
var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//    .WithLifetime(ContainerLifetime.Persistent);

//var db = sql.AddDatabase("AuthDB");

//var authorizationAPI = builder.AddProject<Projects.AuthorizationAPI>("AuthorizationAPI");
//.WithReference(db);

var identityServer = builder.AddProject<Projects.Deunde_IdentityServer>("IdentityServer")
       .WithHttpsEndpoint(port: 6001)
       .WithExternalHttpEndpoints();

var profilesAPI = builder.AddProject<Projects.ProfilesAPI>("ProfilesAPI")
       .WithExternalHttpEndpoints();

//builder.AddJavaScriptApp("react-frontend", "../clinic-web-app-react")
//       .WithRunScript("start")
//       .WithReference(backend)
//       .WithEnvironment("REACT_APP_API_URL", backend.GetEndpoint("http"))
//       .WithHttpEndpoint(port: 3000, env: "PORT")
//       .WithExternalHttpEndpoints();

var bff = builder.AddProject<Projects.BFF_FrontendProxy>("BffProxy")
       // .WithEnvironment("AUTHORIZATION_API_URL", authorizationAPI.GetEndpoint("http"))
       // .WithEnvironment("PROFILES_API_URL", profilesAPI.GetEndpoint("http"))
       .WithHttpEndpoint(port: 5000)
       .WithHttpsEndpoint(port: 5001)
//       .WithReference(authorizationAPI)
       .WithReference(identityServer)
       .WithReference(profilesAPI);

//builder.AddViteApp("vite-frontend", "../Frontend/clinic-web-app-vite")


identityServer.WithReference(bff);

var frontend = builder.AddViteApp("vite-frontend", "../Frontend/clinic-web-app-frontend")
       .WithHttpEndpoint(port: 5173)
       .WithEnvironment("VITE_BFF_PROXY_URL", bff.GetEndpoint("https"))
       .WithReference(bff);



// .WithEnvironment("VITE_AUTHORIZATION_API_URL", authorizationAPI.GetEndpoint("http"))
// .WithEnvironment("VITE_PROFILES_API_URL", profilesAPI.GetEndpoint("http"))
// .WithReference(authorizationAPI)
// .WithReference(profilesAPI);


builder.Build().Run();
