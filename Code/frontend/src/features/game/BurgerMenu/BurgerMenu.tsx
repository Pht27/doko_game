import './BurgerMenu.css';

interface BurgerMenuProps {
  onClick: () => void;
}

export function BurgerMenu({ onClick }: BurgerMenuProps) {
  return (
    <button className="burger-btn" onClick={onClick} aria-label="Spielverlauf">
      <span /><span /><span />
    </button>
  );
}
