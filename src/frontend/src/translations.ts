// Central translations file — all UI strings are defined here.
// Use plain values for static labels; functions for strings that require interpolation.

export const t = {
  // ── App / Loading ──────────────────────────────────────────────────────────
  startingGame: 'Spiel wird gestartet…',
  retry: 'Wiederholen',
  loading: 'Laden…',

  // ── ArmutInfoBanner ────────────────────────────────────────────────────────
  armutInfoBanner: (count: number, hasTrump: boolean) =>
    `Armut: ${count} Karte(n) zurückgegeben${hasTrump ? ' · mit Trump' : ' · kein Trump'}`,

  // ── HealthCheckDialog ──────────────────────────────────────────────────────
  healthCheckTitle: (playerId: number) => `S${playerId}: Gesund oder Vorbehalt?`,
  gesund: 'Gesund',
  vorbehalt: 'Vorbehalt',

  // ── ReservationDialog ──────────────────────────────────────────────────────
  reservationTitle: (playerId: number) => `S${playerId}: Ansagen`,
  pass: 'Passen',
  hochzeitLabel: (condition: string) =>
    `Hochzeit (${hochzeitConditionLabels[condition] ?? condition})`,

  // ── ArmutPartnerDialog ─────────────────────────────────────────────────────
  armutPartnerTitle: (playerId: number) => `S${playerId}: Armut annehmen?`,
  armutPartnerDescription:
    'Ein Mitspieler hat Armut (≤ 3 Trümpfe). Möchtest du sein reicher Partner werden?',
  annehmen: 'Annehmen',
  ablehnen: 'Ablehnen',

  // ── ArmutReturnDialog ──────────────────────────────────────────────────────
  armutReturnTitle: (playerId: number, count: number) =>
    `S${playerId}: ${count} Karte(n) zurückgeben`,
  armutReturnDescription: (selected: number, total: number) =>
    `Wähle ${total} Karte(n) aus deiner Hand (${selected}/${total})`,
  bestaetigen: 'Bestätigen',

  // ── SonderkarteOverlay ─────────────────────────────────────────────────────
  aktiviereSonderkarten: 'Sonderkarten aktivieren?',
  sonderkartenDescription: 'Wähle aus, welche Sonderkarten aktiviert werden sollen, oder keine.',
  genscherPartnerLabel: 'Genscher-Partner (Spieler-ID):',
  playerLabel: (id: number) => `Spieler ${id}`,
  karteAusspielen: 'Karte ausspielen',
  abbrechen: 'Abbrechen',

  // ── ResultScreen ───────────────────────────────────────────────────────────
  winnerLabel: (winner: string) => `${winner} gewinnt!`,
  reAugen: 'Re Augen',
  kontraAugen: 'Kontra Augen',
  spielwert: 'Spielwert',
  spielwertBerechnung: 'Berechnung:',
  hinweis: 'Hinweis',
  feigheit: 'Feigheit',
  zusatzpunkte: 'Zusatzpunkte:',
  gesamtergebnis: 'Gesamtergebnis',
  soloFaktor: (factor: number) => `× ${factor} (Solo)`,
  extrapunkteNetto: 'Extrapunkte',
  neuesSpiel: 'Neues Spiel',
  awardLabel: (type: string, player: number) => `${type} (S${player})`,

  // ── GameInfo ───────────────────────────────────────────────────────────────
  stichInfo: (trickNumber: number, completed: number) =>
    `Stich ${trickNumber} · ${completed} gespielt`,

  // ── TrickArea ──────────────────────────────────────────────────────────────
  keinStich: 'Kein Stich',
  cardAlt: (rank: string, suit: string) => `${rank} ${suit}`,

  // ── PlayerLabel ────────────────────────────────────────────────────────────
  playerName: (id: number) => `S${id}`,
  kartenAnzahl: (count: number) => `${count} Karten`,
  unbekanntePartei: 'unbekannt',
  sonderkarteName: (type: string) =>
    (sonderkarteNames[type] as string | undefined) ?? type,

  // ── AnnouncementButton ─────────────────────────────────────────────────────
  // Announcement type labels shown on the buttons (keyed by the type string from the API)
  announcementLabels: {
    Re: 'Re',
    Kontra: 'Kontra',
    Keine90: 'Keine 90',
    Keine60: 'Keine 60',
    Keine30: 'Keine 30',
    Schwarz: 'Schwarz',
  } as Record<string, string>,
  announcementLabel: (type: string) =>
    (t.announcementLabels[type] as string | undefined) ?? type,
} as const;

// Mapping from API condition keys to German display labels
const hochzeitConditionLabels: Record<string, string> = {
  FirstTrick: 'Erster Stich',
  FirstFehlTrick: 'Erster Fehlstich',
  FirstTrumpTrick: 'Erster Trumpfstich',
};

// Mapping from SonderkarteType enum names to German display names
const sonderkarteNames: Record<string, string> = {
  Schweinchen: 'Schweinchen',
  Superschweinchen: 'Superschweinchen',
  Hyperschweinchen: 'Hyperschweinchen',
  LinksGehangter: 'Links­gehängter',
  RechtsGehangter: 'Rechts­gehängter',
  Genscherdamen: 'Genscherdamen',
  Gegengenscherdamen: 'Gegengenscherdamen',
  Heidmann: 'Heidmann',
  Heidfrau: 'Heidfrau',
  Kemmerich: 'Kemmerich',
  Schatz: 'Schatz',
};
