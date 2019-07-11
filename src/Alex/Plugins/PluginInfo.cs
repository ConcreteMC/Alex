using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Plugins
{
    public sealed class PluginInfo : Attribute
    {
        /// <summary>
        /// The name of the plugin
        /// <example>MyPlugin</example>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version of the plugin
        /// <example>1.4.1</example>
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// A human friendly description of the functionality this plugin provides
        /// <example>This plugin is so 1337. You can set yourself on fire.</example>
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Uniqely identifies who developed this plugin
        /// <example>TruDan</example>
        /// <example>TruDan &lt;trudan@example.com&gt;</example>
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Allows you to list multiple authors, if it is a collaborative project.
        /// <seealso cref="Author"/>
        /// </summary>
        public string[] Authors { get; set; }


        /// <summary>
        /// The plugin's or author's website.
        /// <example>example.com/MyAwesomePlugin</example>
        /// </summary>
        public string Website { get; set; }

    }
}
