namespace Doko.Domain.Rules;

public enum DulleRule
{
    SecondBeatsFirst,
    FirstBeatsSecond,
}

public static class DulleRuleExtensions
{
    public static DulleRule Reversed(this DulleRule rule) =>
        rule == DulleRule.SecondBeatsFirst
            ? DulleRule.FirstBeatsSecond
            : DulleRule.SecondBeatsFirst;
}
