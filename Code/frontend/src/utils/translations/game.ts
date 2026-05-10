const hochzeitConditionLabels: Record<string, string> = {
  FirstTrick: 'Erster Stich',
  FirstFehlTrick: 'Erster Fehlstich',
  FirstTrumpTrick: 'Erster Trumpfstich',
};

const reservationCategoryLabels: Record<string, string> = {
  Solo: 'Solo',
  SchlankerMartin: 'Schlanker Martin',
  Armut: 'Armut',
  Schmeissen: 'Schmeißen',
  Hochzeit: 'Hochzeit',
};

const soloLabels: Record<string, string> = {
  KaroSolo: 'Karo-Solo',
  KreuzSolo: 'Kreuz-Solo',
  PikSolo: 'Pik-Solo',
  HerzSolo: 'Herz-Solo',
  Damensolo: 'Damen-Solo',
  Bubensolo: 'Buben-Solo',
  Fleischloses: 'Fleischloses',
  Knochenloses: 'Knochenloses',
  SchlankerMartin: 'Schlanker Martin',
};

const gameModeLabels: Record<string, string> = {
  ...soloLabels,
  Armut: 'Armut',
  Hochzeit: 'Hochzeit',
};

const extrapunktLabels: Record<string, string> = {
  Doppelkopf: 'Doppelkopf',
  FuchsGefangen: 'Fuchs gefangen',
  Karlchen: 'Karlchen',
  Agathe: 'Agathe',
  Fischauge: 'Fischauge',
  GansGefangen: 'Gans gefangen',
  Klabautermann: 'Klabautermann',
  Kaffeekranzchen: 'Kaffeekränzchen',
};

const phaseLabels: Record<string, string> = {
  ReservationHealthCheck: 'Gesund/Vorbehalt',
  ReservationSoloCheck: 'Solo-Runde',
  ReservationArmutCheck: 'Armut-Runde',
  ReservationSchmeissenCheck: 'Schmeißen-Runde',
  ReservationHochzeitCheck: 'Hochzeit-Runde',
  ArmutPartnerFinding: 'Partner suchen',
  ArmutCardExchange: 'Karten tauschen',
  SchwarzesSauSoloSelect: 'Schwarze Sau - Solo',
  Playing: 'Normalspiel',
  Scoring: 'Abrechnung',
};

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
};

function gameModeLabel(mode: string | null): string {
  return mode ? ((gameModeLabels[mode] as string | undefined) ?? mode) : 'Normalspiel';
}

const announcementLabels: Record<string, string> = {
  Re: 'Re',
  Kontra: 'Kontra',
  Keine90: 'Keine 90',
  Keine60: 'Keine 60',
  Keine30: 'Keine 30',
  Schwarz: 'Schwarz',
};

export const game = {
  // ── App / Game loading ─────────────────────────────────────────────────────
  startingGame: 'Spiel wird gestartet…',

  // ── ArmutExchangeInfo ──────────────────────────────────────────────────────
  armutExchangeInfo: (count: number, hasTrump: boolean) =>
    `${count} Karte(n) getauscht · ${hasTrump ? 'mit Trumpf' : 'kein Trumpf'}`,

  // ── HealthCheckDialog ──────────────────────────────────────────────────────
  healthCheckTitle: 'Gesund oder Vorbehalt?',
  gesund: 'Gesund',
  vorbehalt: 'Vorbehalt',

  // ── ReservationDialog ──────────────────────────────────────────────────────
  reservationTitle: (playerId: number, name?: string) => `${name ?? `S${playerId + 1}`}: Ansagen`,
  pass: 'Passen',
  hochzeitLabel: (condition: string) =>
    `Hochzeit (${hochzeitConditionLabels[condition] ?? condition})`,
  hochzeitConditionLabel: (condition: string) =>
    hochzeitConditionLabels[condition] ?? condition,
  reservationCategoryLabel: (category: string) =>
    (reservationCategoryLabels[category] as string | undefined) ?? category,
  soloLabel: (reservation: string) =>
    (soloLabels[reservation] as string | undefined) ?? reservation,

  // ── ArmutPartnerDialog ─────────────────────────────────────────────────────
  armutPartnerTitle: (playerId: number, name?: string) => `${name ?? `S${playerId + 1}`}: Armut annehmen?`,
  armutPartnerDescription:
    'Ein Mitspieler hat Armut (≤ 3 Trümpfe). Möchtest du sein reicher Partner werden?',
  annehmen: 'Annehmen',
  ablehnen: 'Ablehnen',

  // ── ArmutReturnDialog ──────────────────────────────────────────────────────
  armutReturnTitle: (playerId: number, count: number, name?: string) =>
    `${name ?? `S${playerId + 1}`}: ${count} Karte(n) zurückgeben`,
  armutReturnDescription: (selected: number, total: number) =>
    `Wähle ${total} Karte(n) aus deiner Hand (${selected}/${total})`,

  // ── SchwarzesSauSoloDialog ─────────────────────────────────────────────────
  schwarzesSauSoloTitle: (playerId: number, name?: string) => `${name ?? `S${playerId + 1}`}: Schwarze Sau - Solo wählen`,
  schwarzesSauSoloSubtitle: 'Du hast die zweite Pik Dame gewonnen. Wähle ein Solo.',

  // ── SonderkarteOverlay ─────────────────────────────────────────────────────
  sonderkarteBadge: 'Sonderkarte',
  genscherBadge: 'Genscher',
  aktivieren: 'Aktivieren',
  nichtAktivieren: 'Nicht aktivieren',
  genscherPartnerWaehlen: 'Wohin genschern?',

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
  keineExtrapunkte: '–',
  neuesSpiel: 'Neues Spiel',
  awardLabel: (type: string, player: number, name?: string) => `${type} (${name ?? `S${player + 1}`})`,
  playerLabel: (seat: number) => `Spieler ${seat + 1}`,
  seatShort: (seat: number, name?: string) => name ?? `S${seat + 1}`,
  punkteAenderung: 'Punkteänderung',
  gesamtstand: 'Gesamtstand',
  bereit: 'Bereit',
  matchHistory: 'Match History',
  insgesamt: 'Insgesamt',

  // ── GeschmissenResultScreen ────────────────────────────────────────────────
  geschmissenTitle: 'Schmeißen!',
  geschmissenSubtitle: 'Das Spiel wurde zurückgegeben. Gleicher Rauskommer.',

  // ── Extrapunkte ────────────────────────────────────────────────────────────
  extrapunktLabel: (type: string) =>
    (extrapunktLabels[type] as string | undefined) ?? type,

  // ── GameInfo ───────────────────────────────────────────────────────────────
  stichInfo: (trickNumber: number, completed: number) =>
    `Stich ${trickNumber} · ${completed} gespielt`,
  gameModeLabel,
  phaseLabel: (phase: string) =>
    (phaseLabels[phase] as string | undefined) ?? phase,
  spielInfo: 'Spielinfo',
  nochKeineErgebnisse: 'Noch keine Ergebnisse',

  // ── TrickArea ──────────────────────────────────────────────────────────────
  keinStich: 'Kein Stich',
  cardAlt: (rank: string, suit: string) => `${rank} ${suit}`,

  // ── GameBoard ──────────────────────────────────────────────────────────────
  lastTrick: 'Letzter Stich',

  // ── PlayerLabel ────────────────────────────────────────────────────────────
  playerName: (id: number) => `S${id + 1}`,
  kartenAnzahl: (count: number) => `${count} Karten`,
  unbekanntePartei: 'unbekannt',
  sonderkarteName: (type: string) =>
    (sonderkarteNames[type] as string | undefined) ?? type,

  // ── GameAnnouncePopup ──────────────────────────────────────────────────────
  gameModePopupMessage: (mode: string | null, playerSeat: number | null, name?: string) => {
    const modeLabel = gameModeLabel(mode);
    if (playerSeat !== null && mode !== null) return `${modeLabel} · ${name ?? `S${playerSeat + 1}`}`;
    return modeLabel;
  },
  sonderkartePopupMessage: (playerSeat: number, sonderkarteType: string, name?: string) =>
    `${name ?? `S${playerSeat + 1}`} · ${(sonderkarteNames[sonderkarteType] as string | undefined) ?? sonderkarteType}`,

  // ── AnnouncementButton ─────────────────────────────────────────────────────
  announcementLabels,
  announcementLabel: (type: string) =>
    (announcementLabels[type] as string | undefined) ?? type,
  announcedSuffix: '(angesagt)',
};
