// Central translations file — all UI strings are defined here.
// Use plain values for static labels; functions for strings that require interpolation.

export const t = {
  // ── App / Loading ──────────────────────────────────────────────────────────
  startingGame: 'Spiel wird gestartet…',
  retry: 'Wiederholen',
  loading: 'Laden…',

  // ── LandingPage ────────────────────────────────────────────────────────────
  landingTitle: 'Doppelkopf',
  multiplayer: 'Mehrspieler',
  createLobby: 'Lobby erstellen',
  testGame: 'Testspiel starten',
  releaseNotesTitle: 'Versionshinweise',

  // ── MultiplayerBrowserPage / LobbyDetailView ───────────────────────────────
  back: '← Zurück',
  lobbyTitle: 'Lobby',
  noLobbiesAvailable: 'Keine Lobbys vorhanden',
  seatLabel: (n: number) => `Sitz ${n + 1}`,
  waitingForPlayers: 'Warte auf Spieler…',
  inviteLink: 'Einladungslink',
  copyLink: 'Link kopieren',
  linkCopied: 'Kopiert!',
  startGame: 'Spiel starten',
  leaveSeat: 'Platz verlassen',
  playerSlot: (n: number) => `Spieler ${n + 1}`,
  youSuffix: ' (Du)',
  playerCount: (current: number, total: number) => `${current} von ${total} Spielern`,
  joiningLobby: 'Trete Lobby bei…',
  lobbyFull: 'Lobby ist voll',
  lobbyNotFound: 'Lobby nicht gefunden',

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
  bestaetigenSolo: 'Bestätigen',
  hochzeitLabel: (condition: string) =>
    `Hochzeit (${hochzeitConditionLabels[condition] ?? condition})`,
  hochzeitConditionLabel: (condition: string) =>
    hochzeitConditionLabels[condition] ?? condition,
  reservationCategoryLabel: (category: string) =>
    (reservationCategoryLabels[category] as string | undefined) ?? category,
  soloLabel: (reservation: string) =>
    (soloLabels[reservation] as string | undefined) ?? reservation,

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

  // ── SchwarzesSauSoloDialog ─────────────────────────────────────────────────
  schwarzesSauSoloTitle: (playerId: number) => `S${playerId}: Schwarze Sau - Solo wählen`,
  schwarzesSauSoloSubtitle: 'Du hast die zweite Pik Dame gewonnen. Wähle ein Solo.',

  // ── SonderkarteOverlay ─────────────────────────────────────────────────────
  sonderkarteBadge: 'Sonderkarte',
  genscherBadge: 'Genscher',
  aktivieren: 'Aktivieren',
  nichtAktivieren: 'Nicht aktivieren',
  genscherPartnerWaehlen: 'Neuen Partner wählen',
  abbrechen: 'Abbrechen',

  // ── ResultScreen ───────────────────────────────────────────────────────────
  winnerLabel: (winner: string) => `${winner} gewinnt!`,
  reAugen: 'Re Augen',
  kontraAugen: 'Kontra Augen',
  reLabel: 'Re',
  kontraLabel: 'Kontra',
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
  awardLabel: (type: string, player: number) => `${type} (S${player})`,
  playerLabel: (seat: number) => `Spieler ${seat + 1}`,
  seatShort: (seat: number) => `S${seat + 1}`,
  punkteAenderung: 'Punkteänderung',
  gesamtstand: 'Gesamtstand',
  bereit: 'Bereit',
  zurueckziehen: 'Zurückziehen',
  bereitCount: (count: number) => `${count} / 4 bereit`,
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
  gameModeLabel: (mode: string | null) =>
    mode ? ((gameModeLabels[mode] as string | undefined) ?? mode) : 'Normalspiel',
  phaseLabel: (phase: string) =>
    (phaseLabels[phase] as string | undefined) ?? phase,
  spielInfo: 'Spielinfo',
  schliessen: 'Schließen',
  nochKeineErgebnisse: 'Noch keine Ergebnisse',
  spielverlauf: 'Spielverlauf',

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
  announcedSuffix: '(angesagt)',
} as const;

// Mapping from API condition keys to German display labels
const hochzeitConditionLabels: Record<string, string> = {
  FirstTrick: 'Erster Stich',
  FirstFehlTrick: 'Erster Fehlstich',
  FirstTrumpTrick: 'Erster Trumpfstich',
};

// Mapping from reservation category keys to German display labels
const reservationCategoryLabels: Record<string, string> = {
  Solo: 'Solo',
  SchlankerMartin: 'Schlanker Martin',
  Armut: 'Armut',
  Schmeissen: 'Schmeißen',
  Hochzeit: 'Hochzeit',
};

// Mapping from solo reservation names to German display labels
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

// Mapping from ReservationPriority enum names to German game mode labels
const gameModeLabels: Record<string, string> = {
  ...soloLabels,
  Armut: 'Armut',
  Hochzeit: 'Hochzeit',
};

// Mapping from ExtrapunktType enum names to German display labels
const extrapunktLabels: Record<string, string> = {
  Doppelkopf: 'Doppelkopf',
  FuchsGefangen: 'Fuchs gefangen',
  Karlchen: 'Karlchen',
  Agathe: 'Agathe',
  Fischauge: 'Fischauge',
  GansGefangen: 'Gans gefangen',
  Festmahl: 'Festmahl',
  Blutbad: 'Blutbad',
  Klabautermann: 'Klabautermann',
  Kaffeekranzchen: 'Kaffeekränzchen',
};

// Mapping from GamePhase enum names to German labels
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
};
