import { useEffect, useState } from 'react'

interface PortraitOverlayProps {
  requireLandscape?: boolean
}

export function PortraitOverlay({ requireLandscape = true }: PortraitOverlayProps) {
  const [isPortrait, setIsPortrait] = useState(
    () => window.matchMedia('(orientation: portrait)').matches
  )

  useEffect(() => {
    const mq = window.matchMedia('(orientation: portrait)')
    const handler = (e: MediaQueryListEvent) => setIsPortrait(e.matches)
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  if (!requireLandscape || !isPortrait) return null

  return (
    <div className="fixed inset-0 z-9999 flex flex-col items-center justify-center bg-green-900 text-white">
      {/* Phone rotate icon */}
      <svg className="mb-6 w-20 h-20 opacity-90" viewBox="0 0 64 64" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
        {/* Phone in portrait */}
        <rect x="20" y="8" width="16" height="26" rx="2" />
        {/* Arrow curving to landscape */}
        <path d="M40 18 C50 18 56 26 56 34" />
        <polyline points="52,30 56,34 60,30" />
        {/* Phone in landscape (target state) */}
        <rect x="8" y="38" width="26" height="16" rx="2" />
      </svg>
      <p className="text-xl font-semibold tracking-wide">Bitte Gerät drehen</p>
      <p className="mt-2 text-sm opacity-70">Dieses Spiel benötigt Querformat</p>
    </div>
  )
}
