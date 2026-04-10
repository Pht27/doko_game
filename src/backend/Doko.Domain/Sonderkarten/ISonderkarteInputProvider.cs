using Doko.Domain.Players;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Provides interactive player choices needed by sonderkarten during <see cref="ISonderkarte.Apply"/>.
/// The Application layer implements this from the incoming command so that sonderkarten can produce
/// their full set of modifications without coupling to the command model.
/// </summary>
public interface ISonderkarteInputProvider
{
    /// <summary>
    /// The partner chosen by the Genscher player. Guaranteed non-null when a
    /// Genscherdamen or Gegengenscherdamen sonderkarte is being activated —
    /// the handler validates this before calling Apply.
    /// </summary>
    PlayerId GetGenscherPartner();
}
