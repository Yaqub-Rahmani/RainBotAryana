using Core.Utilities.Files;

namespace WebAPI.Infrastructure
{
    public sealed class WebPathProvider : IPathProvider
    {
        private readonly IWebHostEnvironment _env;
        public WebPathProvider(IWebHostEnvironment env) => _env = env;

        public string ContentRootPath => _env.ContentRootPath;
        public string WebRootPath => _env.WebRootPath ?? System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");
    }
}
