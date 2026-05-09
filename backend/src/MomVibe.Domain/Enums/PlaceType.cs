namespace MomVibe.Domain.Enums;

/// <summary>
/// Classifies the type of a child-friendly place submission.
/// </summary>
public enum PlaceType
{
    /// <summary>An outdoor walking trail or path suitable for families.</summary>
    Walk = 0,

    /// <summary>A playground equipped with swings, slides, or similar play equipment.</summary>
    Playground = 1,

    /// <summary>A restaurant that offers a child-friendly dining environment.</summary>
    Restaurant = 2,

    /// <summary>A cafe that welcomes families and young children.</summary>
    Cafe = 3,

    /// <summary>A museum with exhibits or programmes suitable for children.</summary>
    Museum = 4,

    /// <summary>A zoo or wildlife park where children can observe animals.</summary>
    Zoo = 5,

    /// <summary>A beach or lakeside area appropriate for family visits.</summary>
    Beach = 6,

    /// <summary>A public or private park with open green space for families.</summary>
    Park = 7,

    /// <summary>A theme park, amusement ride, or similar ticketed attraction.</summary>
    ThemeAttraction = 8,

    /// <summary>A structured sports or physical activity venue catering to children.</summary>
    SportsActivity = 9,

    /// <summary>A child-friendly place that does not fit any of the other categories.</summary>
    Other = 10,
}
