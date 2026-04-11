import '../../styles/OwnPartyLabel.css';

interface OwnPartyLabelProps {
  party: string;
}

/**
 * Renders the own player's party (Re/Kontra) as a subtle overlay label
 * at the bottom-center of the screen, floating above the hand display.
 */
export function OwnPartyLabel({ party }: OwnPartyLabelProps) {
  return (
    <div className="own-party-label">
      {party}
    </div>
  );
}
