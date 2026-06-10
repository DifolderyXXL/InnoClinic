
var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Projects.AuthorizationAPI>("AuthorizationAPI");

//builder.AddJavaScriptApp("react-frontend", "../clinic-web-app-react")
//       .WithRunScript("start")
//       .WithReference(backend)
//       .WithEnvironment("REACT_APP_API_URL", backend.GetEndpoint("http"))
//       .WithHttpEndpoint(port: 3000, env: "PORT")
//       .WithExternalHttpEndpoints();

builder.AddViteApp("vite-frontend", "../clinic-web-app-vite")
       .WithReference(backend);

builder.Build().Run();
