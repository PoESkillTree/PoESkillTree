using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.StatTranslation;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for a group of mod value sliders. Shows a line of text describing the sliders at their current
    /// values.
    /// </summary>
    public class SliderGroupViewModel : Notifier
    {
        public IReadOnlyList<SliderViewModel> Sliders { get; }

        private readonly ITranslation _translation;
        public string Text => _translation.Translate(Sliders.Select(s => s.Value).ToList()) ?? "";

        public SliderGroupViewModel(IEnumerable<SliderViewModel> sliders, ITranslation translation)
        {
            _translation = translation;
            Sliders = sliders.ToList();

            foreach (var slider in Sliders)
            {
                slider.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(Text));
            }
        }
    }
}