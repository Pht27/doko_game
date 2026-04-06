namespace Doko.Domain.GameFlow;

public enum GamePhase
{
    Dealing,

    /// <summary>Round 1: each player declares Gesund (no reservation) or Vorbehalt (has one).</summary>
    ReservationHealthCheck,

    /// <summary>
    /// Round 2a: Vorbehalt players declare their Solo (or pass). If exactly one player said
    /// Vorbehalt, they may freely declare any eligible reservation here.
    /// </summary>
    ReservationSoloCheck,

    /// <summary>Round 2b: Vorbehalt players declare Armut or pass (only when no Solo winner).</summary>
    ReservationArmutCheck,

    /// <summary>Round 2c: Vorbehalt players declare Schmeißen or pass (only when no Armut).</summary>
    ReservationSchmeissenCheck,

    /// <summary>Round 2d: Vorbehalt players declare Hochzeit or pass (only when no Schmeißen).</summary>
    ReservationHochzeitCheck,

    /// <summary>Armut won: players sitting after the poor player are asked whether they accept.</summary>
    ArmutPartnerFinding,

    /// <summary>Rich player accepted Armut: trumps are exchanged between poor and rich player.</summary>
    ArmutCardExchange,

    Playing,
    Scoring,
    Finished,

    /// <summary>A player declared Schmeißen and it won the reservation — game ends with no result.</summary>
    Geschmissen,
}
