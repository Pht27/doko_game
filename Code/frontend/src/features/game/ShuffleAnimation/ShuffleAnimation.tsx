import { useEffect } from 'react';
import { cardBackSvgPath } from '@/api/cards';
import './ShuffleAnimation.css';

interface ShuffleAnimationProps {
  onDone: () => void;
}

export function ShuffleAnimation({ onDone }: ShuffleAnimationProps) {
  useEffect(() => {
    const id = setTimeout(onDone, 4000);
    return () => clearTimeout(id);
  }, [onDone]);

  return (
    <div className="sa-overlay">
      <img src={cardBackSvgPath} alt="" className="sa-card sa-center" />
      <img src={cardBackSvgPath} alt="" className="sa-card sa-north" />
      <img src={cardBackSvgPath} alt="" className="sa-card sa-east" />
      <img src={cardBackSvgPath} alt="" className="sa-card sa-south" />
      <img src={cardBackSvgPath} alt="" className="sa-card sa-west" />
    </div>
  );
}
