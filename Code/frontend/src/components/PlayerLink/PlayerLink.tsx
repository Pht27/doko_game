import { Link } from 'react-router-dom';
import './PlayerLink.css';

export function PlayerLink({
  player,
  className,
}: {
  player: { id: number; name: string };
  className?: string;
}) {
  return (
    <Link
      to={`/players/${player.id}`}
      className={`player-link${className ? ` ${className}` : ''}`}
      onClick={(e) => e.stopPropagation()}
    >
      {player.name}
    </Link>
  );
}
