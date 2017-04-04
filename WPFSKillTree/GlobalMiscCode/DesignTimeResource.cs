using System;
using System.ComponentModel;
using System.Windows;

namespace CSharpGlobalCode.GlobalMiscCode
{
    //Code from https://social.technet.microsoft.com/wiki/contents/articles/23287.trick-to-use-a-resourcedictionary-only-when-in-design-mode.aspx
    //Example on how to XAML resource loading
    //<Window.Resources>
    //  <MiscCode:DesignTimeResource DesignTimeSource = "pack://application:,,,/BlueColors.xaml" />
    //</ Window.Resources >
    public class DesignTimeResource : ResourceDictionary
    {
        /// <summary>
        /// Local field storing info about design-time source.
        /// </summary>
        private string designTimeSource;

        /// <summary>
        /// Gets or sets the design time source.
        /// </summary>
        /// <value>
        /// The design time source.
        /// </value>
        public string DesignTimeSource
        {
            get
            {
                return this.designTimeSource;
            }

            set
            {
                this.designTimeSource = value;
                if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
                {
                    base.Source = new Uri(designTimeSource);
                }
            }
        }

        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        /// <returns>The source location of an external resource dictionary. </returns>
        public new Uri Source
        {
            get
            {
                throw new Exception("Use DesignTimeSource instead Source!");
            }

            set
            {
                throw new Exception("Use DesignTimeSource instead Source!");
            }
        }
    }
}
