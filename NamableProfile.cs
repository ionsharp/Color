using Imagin.Core;
using Imagin.Core.Colors;
using System;

namespace Imagin.Apps.Color;

[DisplayName("Profile")]
[Serializable]
public class NamableProfile : Namable<WorkingProfile>
{
    [Object(ObjectLevel.High)]
    public override WorkingProfile Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    public NamableProfile() : base() { }

    public NamableProfile(string name, WorkingProfile profile) : base(name, profile) { }
}