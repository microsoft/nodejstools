// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.NodejsTools.Extras;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveErrorFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Error";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveErrorFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationError;
            this.ForegroundColor = Color.FromRgb(0xee, 00, 00);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveBlackFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Black";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveBlackFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationBlack;
            this.ForegroundColor = Colors.Black;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkRedFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkRed";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkRedFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkRed;
            this.ForegroundColor = Color.FromRgb(0xd4, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkGreenFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkGreen";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkGreenFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkGreen;
            this.ForegroundColor = Color.FromRgb(0x00, 0x8a, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkYellowFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkYellow";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkYellowFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkYellow;
            this.ForegroundColor = Color.FromRgb(0x98, 0x70, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkBlueFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkBlue";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkBlueFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkBlue;
            this.ForegroundColor = Color.FromRgb(0x00, 0x57, 0xff);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkMagentaFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkMagenta";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkMagentaFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkMagenta;
            this.ForegroundColor = Color.FromRgb(0xbb, 0x00, 0xbb);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkCyanFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkCyan";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkCyanFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkCyan;
            this.ForegroundColor = Color.FromRgb(0x00, 0x7f, 0x7f);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveGrayFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Gray";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF
        public InteractiveGrayFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationGray;
            this.ForegroundColor = Color.FromRgb(0x76, 0x76, 0x76);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveDarkGrayFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - DarkGray";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveDarkGrayFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationDarkGray;
            this.ForegroundColor = Color.FromRgb(0x69, 0x69, 0x69);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveRedFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Red";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveRedFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationRed;
            this.ForegroundColor = Color.FromRgb(0xEE, 0, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveGreenFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Green";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveGreenFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationGreen;
            this.ForegroundColor = Color.FromRgb(0x00, 0x80, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveYellowFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Yellow";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveYellowFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationYellow;
            this.ForegroundColor = Color.FromRgb(0xff, 0xff, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal class InteractiveBlueFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Blue";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveBlueFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationBlue;
            this.ForegroundColor = Color.FromRgb(0x00, 0x00, 0xff);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveMagentaFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Magenta";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveMagentaFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationMagenta;
            this.ForegroundColor = Color.FromRgb(0xd1, 0x00, 0xd1);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveCyanFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - Cyan";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveCyanFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationCyan;
            this.ForegroundColor = Color.FromRgb(0x00, 0xff, 0xff);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    internal class InteractiveWhiteFormatDefinition : ClassificationFormatDefinition
    {
        public const string Name = "Node.js Interactive - White";

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.NaturalLanguage)]
        internal static ClassificationTypeDefinition Definition = null; // Set via MEF

        public InteractiveWhiteFormatDefinition()
        {
            this.DisplayName = Resources.ClassificationWhite;
            this.ForegroundColor = Color.FromRgb(0xff, 0xff, 0xff);
        }
    }
}
