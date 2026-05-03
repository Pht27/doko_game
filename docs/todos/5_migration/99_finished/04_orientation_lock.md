# Schritt 3: Orientation Lock Refactoring

Orientation Lock auf die richtigen Routen beschränken. Kompliziert durch iOS, das die Screen Orientation API nicht unterstützt.

---

## Ziel-Verhalten

| Route | Orientation |
|-------|------------|
| `/lobby` | Landscape (erzwungen) |
| `/game/:id` | Landscape (erzwungen) |
| Alle anderen (`/`, `/rules`, `/analog/*`) | Portrait (Standard, kein Lock) |

---

## Aktueller Stand

Die App hat einen globalen Orientation-Lock, wahrscheinlich mit `screen.orientation.lock('landscape')`. Das muss auf den `GamingLayout` beschränkt werden.

---

## iOS-Problem

`screen.orientation.lock()` wird auf iOS Safari **nicht unterstützt** – es wirft einen Fehler oder tut einfach nichts.

**Was auf iOS funktioniert:**
- CSS `@media (orientation: portrait)` zum Erkennen
- Ein Overlay anzeigen, das den User bittet, das Gerät zu drehen (kein echter Lock)
- `window.screen.orientation` lesen, aber nicht schreiben

**Was auf Android funktioniert:**
- `screen.orientation.lock('landscape')` (in PWA-Kontext)
- Requires `display: standalone` im Web App Manifest

---

## Research

Stand nach Schritt 2 (React Router + Layout-Splitting):

- `App.tsx` — sauber, kein globaler Lock
- `GamingLayout.tsx` — hatte Lock-Logik bereits drin, aber **kein Unlock beim Unmount**
- `AppLayout.tsx` — hatte `screen.orientation.lock('portrait')` als Workaround für fehlendes Unlock → laut Spec unnötig
- `useOrientationLock`-Hook existierte noch nicht
- `PortraitOverlay` war Full-Screen-Block

---

## Plan

1. `useOrientationLock.ts` extrahieren: Lock + Unlock (cleanup) + Fullscreen-Reapply + `lockFailed` zurückgeben
2. `GamingLayout.tsx` vereinfachen: Hook einbinden, `lockFailed` an Overlay weitergeben
3. `AppLayout.tsx`: Portrait-Lock entfernen (war Workaround, nicht Spec)
4. `PortraitOverlay.tsx` neu: minimizable Popup
   - Zeigt Hinweis + Rotations-Icon
   - Auto-minimiert nach 3 s auf kleines Icon in der Mitte unten
   - Beim erneuten Portrait-Eintritt: re-expandiert
   - "Verstanden"-Button zum sofortigen Minimieren

---

## Implementierungsansatz

```tsx
// GamingLayout.tsx
useEffect(() => {
  // Versuche den Lock (Android/Desktop)
  screen.orientation?.lock?.('landscape').catch(() => {
    // iOS oder Browser unterstützt es nicht – Overlay zeigen
  });
  
  return () => {
    screen.orientation?.unlock?.();
  };
}, []);
```

**Für iOS:** Das bestehende `PortraitOverlay`-Komponente (zeigt eine Warnung wenn im Portrait-Modus) weiterhin nutzen, aber nur im `GamingLayout` einbinden.

---

## Schritte

1. `useOrientationLock`-Hook extrahieren (Lock + Unlock + iOS-Fallback)
2. Hook nur in `GamingLayout` aufrufen
3. `PortraitOverlay` nur in `GamingLayout` rendern
4. Globalen Lock aus `App.tsx` / altem Code entfernen
5. Auf iOS testen (Simulator oder echtes Gerät)

---

## Betroffene Dateien

- `Code/frontend/src/layouts/GamingLayout.tsx`
- `Code/frontend/src/hooks/useOrientationLock.ts` (neu)
- `Code/frontend/src/components/PortraitOverlay/PortraitOverlay.tsx` (neu gestaltet)
- `Code/frontend/src/layouts/AppLayout.tsx` (Portrait-Lock entfernt)

---

## Status
erledigt
