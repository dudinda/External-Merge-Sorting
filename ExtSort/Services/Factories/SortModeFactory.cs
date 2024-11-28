using ExtSort.Code.Enums;
using ExtSort.Models.Settings;
using ExtSort.Services.Sorter;
using ExtSort.Services.Sorter.Implementation;

using Microsoft.Extensions.Configuration;

using System.Text;

namespace ExtSort.Services.Factories
{
    internal class SortModeFactory
    {
        private readonly IConfiguration _config;

        public SortModeFactory(IConfiguration config)
        {
            _config = config;
        }

        public ISorterService Get(SortMode mode)
        {
            SorterSettings settings; StringBuilder errors;
            switch(mode)
            {
                case SortMode.IO:
                    settings = new SorterIOSettings();
                    _config.GetSection(nameof(SorterSettings)).Bind(settings);
                    _config.GetSection(nameof(SorterIOSettings)).Bind(settings);
                    if (!settings.Validate(out errors))
                        throw new InvalidOperationException(errors.ToString());
                    return new SorterIOService(settings as SorterIOSettings);

                case SortMode.CPU:
                    settings = new SorterCPUSettings();
                    _config.GetSection(nameof(SorterSettings)).Bind(settings);
                    _config.GetSection(nameof(SorterCPUSettings)).Bind(settings);
                    if (!settings.Validate(out errors))
                        throw new InvalidOperationException(errors.ToString());
                    return new SorterCPUService(settings as SorterCPUSettings);
            }
            throw new NotSupportedException(mode.ToString());
        }
    }
}
