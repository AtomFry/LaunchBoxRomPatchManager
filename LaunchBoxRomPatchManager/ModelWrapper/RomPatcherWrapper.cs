using LaunchBoxRomPatchManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace LaunchBoxRomPatchManager.ModelWrapper
{
    public class RomPatcherWrapper : ModelWrapperBase<RomPatcher>
    {
        public RomPatcherWrapper(RomPatcher model) : base(model)
        {            
        }

        protected override void InitializeCollectionProperties(RomPatcher model)
        {
            /*
            if(model.Emulators == null)
            {
                throw new ArgumentException("Emulators cannot be null");
            }

            Emulators = new ChangeTrackingCollection<RomPatcherEmulatorWrapper>(model.Emulators.Select(e => new RomPatcherEmulatorWrapper(e)));
            RegisterCollection(Emulators, model.Emulators);
            */

            if (model.Platforms == null)
            {
                throw new ArgumentException("Platforms cannot be null");
            }

            Platforms = new ChangeTrackingCollection<RomPatcherPlatformWrapper>(model.Platforms.Select(p => new RomPatcherPlatformWrapper(p)));
            RegisterCollection(Platforms, model.Platforms);
        }

        public string Id
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string IdOriginalValue => GetOriginalValue<string>(nameof(Id));

        public bool IdIsChanged => GetIsChanged(nameof(Id));

        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string NameOriginalValue => GetOriginalValue<string>(nameof(Name));

        public bool NameIsChanged => GetIsChanged(nameof(Name));

        public string FilePath
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string FilePathOriginalValue => GetOriginalValue<string>(nameof(FilePath));

        public bool FilePathIsChanged => GetIsChanged(nameof(FilePath));

        public string CommandLine
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string CommandLineOriginalValue => GetOriginalValue<string>(nameof(CommandLine));

        public bool CommandLineIsChanged => GetIsChanged(nameof(CommandLine));

        // public ChangeTrackingCollection<RomPatcherEmulatorWrapper> Emulators { get; private set; }
        
        public ChangeTrackingCollection<RomPatcherPlatformWrapper> Platforms { get; private set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Name is required", new[] { nameof(Name) });
            }

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                yield return new ValidationResult("File path is required", new[] { nameof(FilePath) });
            }
            else
            {
                if (!File.Exists(FilePath))
                {
                    yield return new ValidationResult("File path does not exist", new[] { nameof(FilePath) });
                }
            }

            if (string.IsNullOrWhiteSpace(CommandLine))
            {
                yield return new ValidationResult("Command line is required", new[] { nameof(CommandLine) });
            }
            else
            {
                if (!CommandLine.Contains("{rom}"))
                {
                    yield return new ValidationResult("Command line must contain {rom}", new[] { nameof(CommandLine) });
                }

                if (!CommandLine.Contains("{patch}"))
                {
                    yield return new ValidationResult("Command line must contain {patch}", new[] { nameof(CommandLine) });
                }
            }
        }
    }

}
