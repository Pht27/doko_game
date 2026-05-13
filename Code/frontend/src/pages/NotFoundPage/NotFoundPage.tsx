import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/Button/Button';
import { t } from '@/utils/translations';

export function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      height: '100%',
      gap: '16px',
      background: 'var(--app-bg)',
      color: 'var(--app-text)',
      textAlign: 'center',
      padding: '24px',
    }}>
      <div style={{ fontSize: '4rem', lineHeight: 1 }}>♠</div>
      <h1 style={{ fontSize: '1.4rem', fontWeight: 700, margin: 0 }}>{t.notFoundTitle}</h1>
      <p style={{ fontSize: '0.9rem', color: 'var(--app-text-muted)', margin: 0 }}>
        {t.notFoundDescription}
      </p>
      <Button onClick={() => navigate('/')}>{t.notFoundButton}</Button>
    </div>
  );
}
