import { useEffect, useRef, useState } from 'react';
import './Toast.css';

interface ToastProps {
  message: string;
  duration?: number;
  confettiColors?: string[];
  onDone: () => void;
}

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  color: string;
  size: number;
  rotation: number;
  rotationSpeed: number;
  alpha: number;
}

const DEFAULT_COLORS = ['#f59e0b', '#10b981', '#6366f1', '#ec4899', '#f97316', '#14b8a6', '#a855f7'];

function createParticles(cx: number, cy: number, colors: string[]): Particle[] {
  return Array.from({ length: 80 }, () => {
    const angle = Math.random() * Math.PI * 2;
    const speed = 4 + Math.random() * 8;
    return {
      x: cx,
      y: cy,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed - 5,
      color: colors[Math.floor(Math.random() * colors.length)],
      size: 6 + Math.random() * 7,
      rotation: Math.random() * 360,
      rotationSpeed: (Math.random() - 0.5) * 12,
      alpha: 1,
    };
  });
}

function Confetti({ active, colors }: { active: boolean; colors: string[] }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rafRef = useRef<number>(0);
  const particlesRef = useRef<Particle[]>([]);

  useEffect(() => {
    if (!active) return;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    particlesRef.current = createParticles(canvas.width / 2, canvas.height / 2, colors);

    function tick() {
      if (!ctx || !canvas) return;
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      particlesRef.current = particlesRef.current.filter(p => p.alpha > 0.01);
      for (const p of particlesRef.current) {
        p.x += p.vx;
        p.y += p.vy;
        p.vy += 0.28;
        p.vx *= 0.99;
        p.rotation += p.rotationSpeed;
        p.alpha -= 0.010;
        ctx.save();
        ctx.globalAlpha = Math.max(0, p.alpha);
        ctx.translate(p.x, p.y);
        ctx.rotate((p.rotation * Math.PI) / 180);
        ctx.fillStyle = p.color;
        ctx.fillRect(-p.size / 2, -p.size / 4, p.size, p.size / 2);
        ctx.restore();
      }
      if (particlesRef.current.length > 0) {
        rafRef.current = requestAnimationFrame(tick);
      }
    }

    rafRef.current = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(rafRef.current);
  }, [active]);

  return <canvas ref={canvasRef} className="toast-confetti" />;
}

export function Toast({ message, duration = 2800, confettiColors = DEFAULT_COLORS, onDone }: ToastProps) {
  const [phase, setPhase] = useState<'in' | 'hold' | 'out'>('in');
  const lines = message.split('\n');

  useEffect(() => {
    const toHold = setTimeout(() => setPhase('hold'), 400);
    const toOut = setTimeout(() => setPhase('out'), duration - 500);
    const done = setTimeout(onDone, duration);
    return () => {
      clearTimeout(toHold);
      clearTimeout(toOut);
      clearTimeout(done);
    };
  }, [duration, onDone]);

  return (
    <div className={`toast-overlay toast-overlay--${phase}`} aria-live="polite">
      <Confetti active={phase === 'in' || phase === 'hold'} colors={confettiColors} />
      <div className="toast-card">
        <div className="toast-content">
          {lines.map((line, i) => (
            <p key={i} className={i === lines.length - 1 && lines.length > 1 ? 'toast-name' : 'toast-label'}>
              {line}
            </p>
          ))}
        </div>
      </div>
    </div>
  );
}
