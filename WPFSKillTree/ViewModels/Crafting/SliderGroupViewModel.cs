using System.Collections.Generic;
using System.Linq;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for a group of mod value sliders. Shows additional text if there are either no sliders
    /// or no slider is shown.
    /// </summary>
    public class SliderGroupViewModel : Notifier
    {
        public IReadOnlyList<SliderViewModel> Sliders { get; }
        
        private readonly string _format;
        public string Text => string.Format(_format, Sliders.Select(s => s.Value).Cast<object>().ToArray());

        public SliderGroupViewModel(IEnumerable<SliderViewModel> sliders, string format)
        {
            Sliders = sliders.ToList();
            _format = format;

            foreach (var slider in Sliders)
            {
                slider.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(Text));
            }
        }
    }
}