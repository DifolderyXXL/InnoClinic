
var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//    .WithLifetime(ContainerLifetime.Persistent);

//var db = sql.AddDatabase("AuthDB");

var authorizationAPI = builder.AddProject<Projects.AuthorizationAPI>("AuthorizationAPI");
//.WithReference(db);


var profilesAPI = builder.AddProject<Projects.ProfilesAPI>("ProfilesAPI");

//builder.AddJavaScriptApp("react-frontend", "../clinic-web-app-react")
//       .WithRunScript("start")
//       .WithReference(backend)
//       .WithEnvironment("REACT_APP_API_URL", backend.GetEndpoint("http"))
//       .WithHttpEndpoint(port: 3000, env: "PORT")
//       .WithExternalHttpEndpoints();

builder.AddViteApp("vite-frontend", "../clinic-web-app-vite")
       .WithHttpEndpoint(port: 3000)
       .WithEnvironment("VITE_AUTHORIZATION_API_URL", authorizationAPI.GetEndpoint("http"))
       .WithEnvironment("VITE_PROFILES_API_URL", profilesAPI.GetEndpoint("http"))
       .WithReference(authorizationAPI)
       .WithReference(profilesAPI);


builder.Build().Run();
