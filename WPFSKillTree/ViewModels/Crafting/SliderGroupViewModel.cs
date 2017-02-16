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
        
        private readonly string _textAfter;
        public string Text => string.Join("", Sliders.Select(s => s.Text)) + _textAfter;
        public bool ShowText { get; }

        public SliderGroupViewModel(IEnumerable<SliderViewModel> sliders, string textAfter)
        {
            Sliders = sliders.ToList();
            _textAfter = textAfter;
            ShowText = !Sliders.Any() || Sliders.Any(s => !s.ShowText);

            foreach (var slider in Sliders)
            {
                slider.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(Text));
            }
        }
    }
}