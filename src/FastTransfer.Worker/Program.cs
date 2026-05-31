using FastTransfer.Infrastructure;
using FastTransfer.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFastTransferInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
