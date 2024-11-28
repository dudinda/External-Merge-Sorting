using ExtSort.Code.Enums;
using ExtSort.Models.Settings;
using ExtSort.Services.Sorter;
using ExtSort.Services.Sorter.Implementation;

namespace ExtSort.Services.Factories
{
    internal class SortModeFactory
    {
        private readonly SorterSetting _settings;
        public SortModeFactory(SorterSetting settings)
        {
            _settings = settings;
        }

        public ISorterService Get(SortMode mode)
        {
            switch(mode)
            {
                case SortMode.IO:
                    return new SorterIOService(_settings);
                case SortMode.CPU:
                    return new SorterCPUService(_settings);
            }

            throw new NotSupportedException(mode.ToString());
        }
    }
}
