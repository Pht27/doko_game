import { useState } from 'react';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import './RulesPage.css';

const cards = import.meta.glob('../../assets/cards/*.svg', {
  eager: true,
  query: '?url',
  import: 'default',
}) as Record<string, string>;

function card(name: string) {
  return cards[`../../assets/cards/${name}.svg`] ?? '';
}

interface RulesPageProps {
  onBack: () => void;
}

type SectionId = 'grundlagen' | 'trumpf' | 'vorbehalte' | 'soli' | 'ansagen' | 'sonderkarten' | 'extrapunkte' | 'spielwert';

const SECTION_KEYWORDS: Record<SectionId, string> = {
  grundlagen: 'grundlagen spieler karten augen 121 re kontra team ziel deck parteien',
  trumpf: 'trumpf dulle dame bube karo herz reihenfolge fehlfarbe pik kreuz fuchs stich bedienungspflicht',
  vorbehalte: 'vorbehalt solo hochzeit armut schwarze sau schmeißen gesund reservation priorität',
  soli: 'solo farbsolo damensolo bubensolo fleischloses knochenloses schlanker martin stille hochzeit kontrasolo',
  ansagen: 'ansagen re kontra keine 90 60 30 schwarz pflichtansage feigheit timing',
  sonderkarten: 'sonderkarten schweinchen superschweinchen hyperschweinchen genschern heidmann kemmerich linksdrehend gehängter',
  extrapunkte: 'extrapunkte fuchs karlchen doppelkopf agathe fischauge gans festmahl blutbad klabautermann kaffeekränzchen',
  spielwert: 'spielwert punkte wertung gewonnen gegen alten solo dreifach',
};

export function RulesPage({ onBack }: RulesPageProps) {
  const [openSections, setOpenSections] = useState<Set<SectionId>>(new Set(['grundlagen']));
  const [search, setSearch] = useState('');

  function toggleSection(id: SectionId) {
    setOpenSections((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  const q = search.toLowerCase().trim();

  function sectionVisible(id: SectionId, bodyText: string) {
    if (!q) return true;
    return bodyText.toLowerCase().includes(q) || SECTION_KEYWORDS[id].includes(q);
  }

  return (
    <div className="rp-page">
      <div className="rp-header">
        <div className="rp-header-top">
          <BackButton onClick={onBack} />
        </div>
        <div className="rp-header-cards">
          <div className="rp-hcard"><img src={card('h10')} alt="Herz 10" /></div>
          <div className="rp-hcard"><img src={card('krD')} alt="Kreuz Dame" /></div>
          <div className="rp-hcard"><img src={card('kA')} alt="Karo Ass" /></div>
          <div className="rp-hcard"><img src={card('krB')} alt="Kreuz Bube" /></div>
        </div>
        <h1 className="rp-header-title">{t.rulesTitle}</h1>
        <p className="rp-header-sub">Koppeldopf — Karten, Trumpf &amp; Spielregeln</p>
      </div>

      <div className="rp-search-bar">
        <div className="rp-search-wrap">
          <svg className="rp-search-icon" width="15" height="15" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            className="rp-search-input"
            type="search"
            placeholder="Regel suchen …"
            autoComplete="off"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <div className="rp-sections">

        {/* 1. Grundlagen */}
        {sectionVisible('grundlagen', 'Koppeldopf 4 Spieler 48 Karten Re Kontra Augen Ass Zehn König Dame Bube Neun') && (
          <AccordionSection
            id="grundlagen"
            icon={<><span className="rp-blk">♣</span><span className="rp-red">♦</span></>}
            title="Grundlagen"
            subtitle="Spieler, Deck, Ziel"
            open={openSections.has('grundlagen')}
            onToggle={() => toggleSection('grundlagen')}
          >
            <div className="rp-block">
              <h3 className="rp-block-label">Das Spiel</h3>
              <p className="rp-p">Koppeldopf ist eine Variante von Doppelkopf für <strong>4 Spieler</strong>. Gespielt wird <strong>gegen den Uhrzeigersinn</strong>.</p>
              <p className="rp-p">Das Deck besteht aus <strong>48 Karten</strong> — ein normales 32-Karten-Blatt, doppelt. Jede Karte (9, Bube, Dame, König, 10, Ass in <span className="rp-blk">♣ ♠</span> <span className="rp-red">♥ ♦</span>) gibt es zweimal.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Parteien</h3>
              <div className="rp-party-row">
                <div className="rp-party-pill rp-party-re">
                  <div className="rp-pill-label">Re</div>
                  <div className="rp-pill-sub">Beide ♣ Damen · Ziel: 121+ Augen</div>
                </div>
                <div className="rp-party-pill rp-party-kontra">
                  <div className="rp-pill-label">Kontra</div>
                  <div className="rp-pill-sub">Die anderen zwei · Ziel: Re stoppen</div>
                </div>
              </div>
              <div className="rp-info-box rp-info-blue">Partei-Zugehörigkeit ist geheim — nur durch Ausspielen der ♣ Dame oder eine Ansage wird sie offenbart.</div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Augenwerte</h3>
              <table className="rp-augen-table">
                <thead><tr><th>Karte</th><th style={{ textAlign: 'right' }}>Augen</th></tr></thead>
                <tbody>
                  <tr><td>Ass</td><td className="rp-val">11</td></tr>
                  <tr><td>Zehn</td><td className="rp-val">10</td></tr>
                  <tr><td>König</td><td className="rp-val">4</td></tr>
                  <tr><td>Dame</td><td className="rp-val">3</td></tr>
                  <tr><td>Bube</td><td className="rp-val">2</td></tr>
                  <tr><td>Neun</td><td className="rp-val">0</td></tr>
                </tbody>
              </table>
              <p className="rp-p rp-muted" style={{ marginTop: 8, fontSize: '0.8rem' }}>Gesamt: 240 Augen im Deck</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Geben</h3>
              <p className="rp-p">Karten werden <strong>4 × 3</strong> ausgeteilt (vier Runden à 3 Karten). Der Geber rotiert jede Runde. Der Spieler rechts vom Geber spielt zuerst.</p>
            </div>
          </AccordionSection>
        )}

        {/* 2. Trumpf & Fehlfarben */}
        {sectionVisible('trumpf', 'Trumpf Fehlfarben Dulle Dame Bube Karo Herz Fuchs Stich Pik Kreuz Bedienungspflicht') && (
          <AccordionSection
            id="trumpf"
            icon={<span className="rp-red" style={{ fontSize: '1.2rem' }}>♦</span>}
            title="Trumpf & Fehlfarben"
            subtitle="26 Trümpfe, Stichregeln"
            open={openSections.has('trumpf')}
            onToggle={() => toggleSection('trumpf')}
          >
            <div className="rp-block">
              <h3 className="rp-block-label">Trumpfreihenfolge — hoch nach niedrig</h3>
              <div className="rp-trump-list">
                <TrumpItem rank="1–2" img={card('h10')} alt="Herz 10" tier="dulle"><span className="rp-red">♥</span> 10 — Dulle <span className="rp-badge rp-badge-dulle">Dulle</span></TrumpItem>
                <TrumpItem rank="3–4" img={card('krD')} alt="Kreuz Dame" tier="dame"><span className="rp-blk">♣</span> Dame <span className="rp-badge rp-badge-re">Re</span></TrumpItem>
                <TrumpItem rank="5–6" img={card('pD')} alt="Pik Dame" tier="dame"><span className="rp-blk">♠</span> Dame</TrumpItem>
                <TrumpItem rank="7–8" img={card('hD')} alt="Herz Dame" tier="dame"><span className="rp-red">♥</span> Dame</TrumpItem>
                <TrumpItem rank="9–10" img={card('kD')} alt="Karo Dame" tier="dame"><span className="rp-red">♦</span> Dame</TrumpItem>
                <TrumpItem rank="11–12" img={card('krB')} alt="Kreuz Bube" tier="bube"><span className="rp-blk">♣</span> Bube — Karlchen</TrumpItem>
                <TrumpItem rank="13–14" img={card('pB')} alt="Pik Bube" tier="bube"><span className="rp-blk">♠</span> Bube</TrumpItem>
                <TrumpItem rank="15–16" img={card('hB')} alt="Herz Bube" tier="bube"><span className="rp-red">♥</span> Bube</TrumpItem>
                <TrumpItem rank="17–18" img={card('kB')} alt="Karo Bube" tier="bube"><span className="rp-red">♦</span> Bube</TrumpItem>
                <TrumpItem rank="19–20" img={card('kA')} alt="Karo Ass" tier="karo"><span className="rp-red">♦</span> Ass — Fuchs <span className="rp-badge rp-badge-fuchs">Fuchs</span></TrumpItem>
                <TrumpItem rank="21–22" img={card('kK')} alt="Karo König" tier="karo"><span className="rp-red">♦</span> König</TrumpItem>
                <TrumpItem rank="23–24" img={card('k10')} alt="Karo Zehn" tier="karo"><span className="rp-red">♦</span> Zehn</TrumpItem>
                <TrumpItem rank="25–26" img={card('k9')} alt="Karo Neun" tier="karo"><span className="rp-red">♦</span> Neun</TrumpItem>
              </div>
              <p className="rp-p rp-muted" style={{ marginTop: 10, fontSize: '0.81rem' }}>Alle Damen, alle Buben, <span className="rp-red">♥</span> 10 und alle <span className="rp-red">♦</span>-Karten sind Trumpf.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Dulle-Regel</h3>
              <p className="rp-p">Standard: <strong>die zweite gespielte Dulle schlägt die erste</strong> — außer im letzten Stich, wo die erste gewinnt.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Fehlfarben</h3>
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-title"><span className="rp-blk">♣ ♠</span> Fehlfarbe</div><div className="rp-item-desc">Ass &gt; Zehn &gt; König &gt; Neun</div></div>
                <div className="rp-item"><div className="rp-item-title"><span className="rp-red">♥</span> Fehlfarbe</div><div className="rp-item-desc">Ass &gt; König &gt; Neun — <span className="rp-red">♥</span> Zehn ist Trumpf (Dulle)!</div></div>
              </div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Stichregeln</h3>
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-title">Bedienungspflicht</div><div className="rp-item-desc">Wer die angespielte Farbe hat, muss bedienen. Trumpf muss mit Trumpf bedient werden.</div></div>
                <div className="rp-item"><div className="rp-item-title">Stechen / Abwerfen</div><div className="rp-item-desc">Wer nicht bedienen kann, darf stechen (Trumpf) oder abwerfen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Gleichstand</div><div className="rp-item-desc">Bei gleichen Karten gewinnt die zuerst gespielte — Ausnahme: Dulle (zweite schlägt erste).</div></div>
              </div>
            </div>
          </AccordionSection>
        )}

        {/* 3. Vorbehalte */}
        {sectionVisible('vorbehalte', 'Vorbehalt Solo Hochzeit Armut Schwarze Sau Schmeißen Gesund Priorität Neugeben') && (
          <AccordionSection
            id="vorbehalte"
            icon={<span style={{ fontSize: '0.85rem', fontWeight: 700, letterSpacing: '-0.5px' }}>VB</span>}
            title="Vorbehalte"
            subtitle="Schmeißen, Armut, Hochzeit, Solo"
            open={openSections.has('vorbehalte')}
            onToggle={() => toggleSection('vorbehalte')}
          >
            <div className="rp-block">
              <h3 className="rp-block-label">Ablauf</h3>
              <p className="rp-p">Vor dem Spiel meldet jeder Spieler in Spielreihenfolge „Vorbehalt" oder „Gesund". Vorbehalte werden dann in derselben Reihenfolge aufgedeckt.</p>
              <div className="rp-info-box rp-info-blue">Priorität (höher schlägt niedriger): <strong>Solo &gt; Armut &gt; Schwarze Sau &gt; Hochzeit &gt; Schmeißen</strong></div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Schmeißen — Neugeben</h3>
              <p className="rp-p">Neugeben erlaubt, wenn die Hand eine der folgenden Bedingungen erfüllt:</p>
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-desc">Augen &gt; 80 oder &lt; 35</div></div>
                <div className="rp-item"><div className="rp-item-desc">Höchstens 3 Trümpfe auf der Hand</div></div>
                <div className="rp-item"><div className="rp-item-desc">Höchster Trumpf ist ein <span className="rp-red">♦</span> Bube</div></div>
                <div className="rp-item"><div className="rp-item-desc">Mindestens 5 Neunen, 5 Könige, oder Neunen + Könige ≥ 8</div></div>
              </div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Armut</h3>
              <p className="rp-p">Bedingung: <strong>höchstens 3 Trümpfe</strong> (Füchse / <span className="rp-red">♦</span> Asse zählen dabei nicht). Der arme Spieler bietet seine Trümpfe reihum an. Wer annimmt, erhält alle Trümpfe und gibt dieselbe Anzahl Karten zurück. Der Reiche und der Arme bilden Re. Falls niemand annimmt: Schwarze Sau.</p>
              <div className="rp-info-box rp-info-gold">Alle Sonderkarten sind in der Armut deaktiviert.</div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Schwarze Sau</h3>
              <p className="rp-p">Tritt auf, wenn Armut abgelehnt wird. Normales Spiel, bis jemand den Stich mit der <strong>zweiten <span className="rp-blk">♠</span> Dame</strong> gewinnt — dieser muss ab sofort Solo spielen. Der Armut-Spieler spielt aus.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Hochzeit</h3>
              <p className="rp-p">Bedingung: beide <span className="rp-blk">♣</span> Damen auf einer Hand. Der Spieler nennt eine Findbedingung: <strong>erster Stich</strong>, <strong>erster Fehlstich</strong> oder <strong>erster Trumpfstich</strong>. Wer den passenden Stich gewinnt, wird Partner (Re). Findet sich in den ersten drei Stichen kein Partner: Stille Hochzeit (Solo).</p>
            </div>
          </AccordionSection>
        )}

        {/* 4. Soli */}
        {sectionVisible('soli', 'Solo Farbsolo Damensolo Bubensolo Fleischloses Knochenloses Schlanker Martin Stille Hochzeit Kontrasolo') && (
          <AccordionSection
            id="soli"
            icon={<span style={{ fontSize: '0.85rem', fontWeight: 700 }}>1v3</span>}
            title="Soli"
            subtitle="1 gegen 3 — alle Typen"
            open={openSections.has('soli')}
            onToggle={() => toggleSection('soli')}
          >
            <div className="rp-block">
              <div className="rp-info-box rp-info-blue">Der Solo-Spieler spielt immer aus. Punkte werden am Ende <strong>verdreifacht</strong>. Sonderkarten und Extrapunkte sind deaktiviert — außer in Stillen Soli.</div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Angekündigte Soli</h3>
              <div className="rp-item-list">
                <div className="rp-item">
                  <div className="rp-item-title">Farbsolo (<span className="rp-blk">♣ ♠</span> <span className="rp-red">♥ ♦</span>)</div>
                  <div className="rp-item-desc">Gleiche Trumpfstruktur wie normal, aber das <span className="rp-red">♦</span>-Trumpfmuster wird durch die gewählte Farbe ersetzt. Dulle bleibt höchster Trumpf. Ziel: 121 Augen.</div>
                </div>
                <div className="rp-item"><div className="rp-item-title">Damensolo</div><div className="rp-item-desc">Nur Damen sind Trumpf (8 Karten). Fehlreihenfolge: A &gt; 10 &gt; K &gt; B &gt; 9. Ziel: 121 Augen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Bubensolo</div><div className="rp-item-desc">Nur Buben sind Trumpf (8 Karten). Fehlreihenfolge: A &gt; 10 &gt; K &gt; D &gt; 9. Ziel: 121 Augen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Fleischloses</div><div className="rp-item-desc">Kein Trumpf. Reihenfolge: A &gt; 10 &gt; K &gt; D &gt; B &gt; 9 (Zehnen hoch). Ziel: 121 Augen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Knochenloses</div><div className="rp-item-desc">Kein Trumpf. Reihenfolge: A &gt; K &gt; D &gt; B &gt; 10 &gt; 9 (Zehnen niedrig). Ziel: keinen Stich gewinnen. Beim ersten eigenen Stich verloren.</div></div>
                <div className="rp-item"><div className="rp-item-title">Schlanker Martin</div><div className="rp-item-desc">Normale Regeln, keine Sonderkarten. Gleichstand umgekehrt: zweite gleiche Karte gewinnt. Ziel: wenigste Stiche. Niedrigste Priorität unter allen Soli.</div></div>
              </div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Stille Soli</h3>
              <div className="rp-item-list">
                <div className="rp-item">
                  <div className="rp-item-title">Stille Hochzeit</div>
                  <div className="rp-item-desc">Spieler mit beiden <span className="rp-blk">♣</span> Damen erklärt keine Hochzeit. Wird erst beim Ausspielen der zweiten <span className="rp-blk">♣</span> Dame offenbart. Sonderkarten und Extrapunkte bleiben aktiv. Am Ende als Solo gewertet.</div>
                </div>
                <div className="rp-item">
                  <div className="rp-item-title">Kontrasolo</div>
                  <div className="rp-item-desc">Bedingung: beide <span className="rp-blk">♠</span> Damen und beide <span className="rp-blk">♠</span> Könige auf einer Hand. Pflicht — kein Wahlrecht. Die <span className="rp-blk">♠</span> Könige werden zu Klabautermännern (höchste Trümpfe). Ansage: „Kontra".</div>
                </div>
              </div>
            </div>
          </AccordionSection>
        )}

        {/* 5. Ansagen */}
        {sectionVisible('ansagen', 'Ansagen Re Kontra Keine 90 60 30 Schwarz Pflichtansage Feigheit Zeitfenster') && (
          <AccordionSection
            id="ansagen"
            icon={
              <span style={{ fontSize: '0.75rem', fontWeight: 700, textAlign: 'center', lineHeight: 1.2 }}>
                <span style={{ color: 'var(--rp-re-label)', display: 'block' }}>Re</span>
                <span style={{ color: 'var(--rp-kontra-label)', display: 'block' }}>Ko</span>
              </span>
            }
            title="Ansagen"
            subtitle="Re, Kontra, Abstufen, Feigheit"
            open={openSections.has('ansagen')}
            onToggle={() => toggleSection('ansagen')}
          >
            <div className="rp-block">
              <h3 className="rp-block-label">Reihenfolge</h3>
              <div className="rp-item-list">
                <div className="rp-item" style={{ borderLeft: '3px solid var(--rp-re)' }}>
                  <div className="rp-item-title" style={{ color: 'var(--rp-re-label)' }}>Re / Kontra</div>
                  <div className="rp-item-desc">Erste Ansage — gibt Partei preis und verdoppelt den Spielwert.</div>
                </div>
                <div className="rp-item">
                  <div className="rp-item-title">Keine 90 → Keine 60 → Keine 30 → Schwarz</div>
                  <div className="rp-item-desc">Jede Stufe erhöht den Wert um +1 und verschärft das eigene Siegziel. Muss aufsteigend angesagt werden.</div>
                </div>
              </div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Zeitfenster</h3>
              <p className="rp-p">Ansagen sind erlaubt bis die <strong>zweite Karte des zweiten Stichs</strong> gespielt wird. Jede Ansage verschiebt die Frist um einen vollen Stich nach vorn.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Pflichtansage</h3>
              <p className="rp-p">Gewinnt ein Spieler einen Stich mit <strong>mindestens 35 Augen</strong>, muss er sofort ansagen (Re oder Kontra). Gilt nicht in Soli.</p>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Feigheit</h3>
              <p className="rp-p">Die gewinnende Partei verliert, wenn sie gemessen am Ergebnis zu wenig angesagt hat. Maximal <strong>2 fehlende Ansagen</strong> sind erlaubt — jede weitere kostet den Sieg plus einen Extra-Minuspunkt.</p>
              <div className="rp-info-box rp-info-red">Beispiel: Nichts angesagt, Verlierer hat weniger als 60 Augen — 3 fehlende Ansagen — Gewinner verliert tatsächlich.</div>
              <p className="rp-p" style={{ marginTop: 8 }}>Gilt nicht in Soli.</p>
            </div>
          </AccordionSection>
        )}

        {/* 6. Sonderkarten */}
        {sectionVisible('sonderkarten', 'Sonderkarten Schweinchen Superschweinchen Hyperschweinchen Genschern Heidmann Kemmerich Linksdrehend Gehängter Fuchs') && (
          <AccordionSection
            id="sonderkarten"
            icon={<span style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--rp-text-muted)' }}>SK</span>}
            title="Sonderkarten"
            subtitle="Schweinchen, Genschern, Heidmann …"
            open={openSections.has('sonderkarten')}
            onToggle={() => toggleSection('sonderkarten')}
          >
            <div className="rp-info-box rp-info-gold" style={{ marginTop: 16 }}>Alle Sonderkarten sind in Soli und Armut deaktiviert — außer in Stillen Soli (Stille Hochzeit, Kontrasolo).</div>

            <div className="rp-block">
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-title">Schweinchen</div><div className="rp-item-desc">Beide <span className="rp-red">♦</span> Asse auf einer Hand. Diese werden zu Schweinchen — höchste Trümpfe, über der Dulle. Ansage beim ersten gespielten Schweinchen möglich.</div></div>
                <div className="rp-item"><div className="rp-item-title">Superschweinchen</div><div className="rp-item-desc">Benötigt aktive Schweinchen. Beide <span className="rp-red">♦</span> Zehnen auf einer Hand — rangieren über den Schweinchen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Hyperschweinchen</div><div className="rp-item-desc">Benötigt aktive Superschweinchen. Beide <span className="rp-red">♦</span> Könige auf einer Hand — rangieren über den Superschweinchen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Linksdrehender / Rechtsdrehender Gehängter</div><div className="rp-item-desc">Beide <span className="rp-red">♦</span> Buben auf einer Hand. Beim ersten <span className="rp-red">♦</span> Buben: Spielrichtung umkehren. Beim zweiten: erneut umkehren.</div></div>
                <div className="rp-item"><div className="rp-item-title">Genscherdamen</div><div className="rp-item-desc">Beide <span className="rp-red">♥</span> Damen auf einer Hand. Beim ersten Ausspielen: „Genschern" — neuen Partner frei wählen. Das neue Paar ist Re. Bisherige Ansagen verfallen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Gegengenscherdamen</div><div className="rp-item-desc">Benötigt aktive Genscherdamen. Beide <span className="rp-red">♦</span> Damen auf einer Hand. Nach dem Genschern: erneut Partner wählen (Gegengenschern).</div></div>
                <div className="rp-item"><div className="rp-item-title">Heidmann</div><div className="rp-item-desc">Beide <span className="rp-blk">♠</span> Buben auf einer Hand. Beim ersten <span className="rp-blk">♠</span> Buben: „Heidmann" — Buben rangieren jetzt über Damen. Muss beim ersten <span className="rp-blk">♠</span> Buben angesagt werden, sonst verfällt der Effekt.</div></div>
                <div className="rp-item"><div className="rp-item-title">Heidfrau</div><div className="rp-item-desc">Benötigt angesagten Heidmann. Beide <span className="rp-blk">♠</span> Damen auf einer Hand. Beim nächsten <span className="rp-blk">♠</span> Damen-Ausspielen: Heidmann-Effekt rückgängig machen.</div></div>
                <div className="rp-item"><div className="rp-item-title">Kemmerich</div><div className="rp-item-desc">Beide <span className="rp-red">♥</span> Buben auf einer Hand. Beim Ausspielen eines <span className="rp-red">♥</span> Buben: eine Ansage der eigenen Partei zurückziehen. Insgesamt nur eine Rücknahme möglich.</div></div>
              </div>
            </div>
          </AccordionSection>
        )}

        {/* 7. Extrapunkte */}
        {sectionVisible('extrapunkte', 'Extrapunkte Fuchs Karlchen Doppelkopf Agathe Fischauge Gans Festmahl Blutbad Klabautermann Kaffeekränzchen Meuterei') && (
          <AccordionSection
            id="extrapunkte"
            icon={<span style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--rp-text-muted)' }}>EP</span>}
            title="Extrapunkte"
            subtitle="Fuchs, Karlchen, Doppelkopf …"
            open={openSections.has('extrapunkte')}
            onToggle={() => toggleSection('extrapunkte')}
          >
            <div className="rp-info-box rp-info-gold" style={{ marginTop: 16 }}>Alle Extrapunkte sind in Soli deaktiviert — außer in Stillen Soli. Extrapunkte beider Parteien werden gegeneinander verrechnet.</div>

            <div className="rp-block">
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-title">Doppelkopf</div><div className="rp-item-desc">Stich mit mindestens 40 Augen: +1 Punkt für die gewinnende Partei.</div></div>
                <div className="rp-item"><div className="rp-item-title">Fuchs gefangen</div><div className="rp-item-desc">Ein <span className="rp-red">♦</span> Ass (Fuchs) landet bei der Gegenpartei: +1 Punkt für die Gegenpartei.</div></div>
                <div className="rp-item"><div className="rp-item-title">Karlchen</div><div className="rp-item-desc">Ein <span className="rp-blk">♣</span> Bube gewinnt den letzten Stich: +1 Punkt. Deaktiviert wenn Heidmann aktiv; reaktiviert durch Heidfrau.</div></div>
                <div className="rp-item"><div className="rp-item-title">Agathe</div><div className="rp-item-desc">Eine <span className="rp-red">♦</span> Dame schlägt einen gegnerischen <span className="rp-blk">♣</span> Buben (Karlchen) im letzten Stich: +1 Punkt für die Agathe-Partei.</div></div>
                <div className="rp-item"><div className="rp-item-title">Fischauge</div><div className="rp-item-desc"><span className="rp-red">♦</span> Neunen werden nach dem ersten Trumpf zu Fischaugen. Gewinnt ein Fischauge einen Stich: +1 Punkt.</div></div>
                <div className="rp-item"><div className="rp-item-title">Gans gefangen</div><div className="rp-item-desc">Ein Fuchs schlägt ein gegnerisches Fischauge (Gans): +1 Punkt für die Fuchs-Partei.</div></div>
                <div className="rp-item"><div className="rp-item-title">Klabautermann</div><div className="rp-item-desc">Ein <span className="rp-blk">♠</span> König (der Gegenpartei) wird von einer <span className="rp-blk">♠</span> Dame gefangen: +1 Punkt für die ♠-Damen-Partei.</div></div>
                <div className="rp-item"><div className="rp-item-title">Kaffeekränzchen</div><div className="rp-item-desc">Ein Stich besteht aus 4 Damen beliebiger Farbe: +1 Punkt für die gewinnende Partei.</div></div>
              </div>
            </div>

            <div className="rp-block">
              <h3 className="rp-block-label">Stich-Gewinner-Sonderregeln</h3>
              <p className="rp-p">Diese Regeln geben <strong>keinen Extrapunkt</strong> — sie bestimmen nur, wer den Stich bekommt. Tiere: Fuchs (♦ A), Schweinchen (♦ A wenn aktiv), Fischauge (♦ 9 nach erstem Trumpfstich), Superschweinchen (♦ 10), Hyperschweinchen (♦ K).</p>
              <div className="rp-item-list">
                <div className="rp-item"><div className="rp-item-title">Festmahl</div><div className="rp-item-desc">Stich mit ≥ 3 Tieren, davon zwei gleich: die zweite Karte der Mehrheitsgattung gewinnt. Bei zwei Paaren gewinnt die letzte Karte.</div></div>
                <div className="rp-item"><div className="rp-item-title">Blutbad</div><div className="rp-item-desc">Stich mit ≥ 3 verschiedenen Tiergattungen: die Nicht-Tier-Karte gewinnt. Sind alle Karten Tiere, gewinnt das Fischauge. Vorrang vor Festmahl.</div></div>
                <div className="rp-item"><div className="rp-item-title">Meuterei</div><div className="rp-item-desc">Ein ♠ König an Stelle 1 oder 2, danach eine ♠ Dame (Klabautermann-Fangversuch), danach ein zweiter ♠ König — und die ♠ Dame wäre die höchste Karte: der zweite ♠ König gewinnt stattdessen. Kein Klabautermann-Punkt.</div></div>
              </div>
            </div>
          </AccordionSection>
        )}

        {/* 8. Spielwert */}
        {sectionVisible('spielwert', 'Spielwert Punkte Wertung Gewonnen Gegen Alten Solo Dreifach') && (
          <AccordionSection
            id="spielwert"
            icon={<span style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--rp-re-label)' }}>+/−</span>}
            title="Spielwert"
            subtitle="Wie Punkte berechnet werden"
            open={openSections.has('spielwert')}
            onToggle={() => toggleSection('spielwert')}
          >
            <div className="rp-block">
              <table className="rp-score-table">
                <thead><tr><th>Komponente</th><th>Bedingung</th><th style={{ textAlign: 'right' }}>Pkt</th></tr></thead>
                <tbody>
                  <tr><td>Gewonnen</td><td>Siegende Partei</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Gegen die Alten</td><td>Kontra gewinnt</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Keine 90</td><td>Verlierer &lt; 90 Augen</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Keine 60</td><td>Verlierer &lt; 60 Augen</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Keine 30</td><td>Verlierer &lt; 30 Augen</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Schwarz</td><td>Verlierer: kein Stich</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Pro Ansage</td><td>Je erfüllte Ansage</td><td className="rp-pts">+1</td></tr>
                  <tr><td>Extrapunkte</td><td>Gegeneinander verrechnet</td><td className="rp-pts">±1</td></tr>
                </tbody>
              </table>
              <div className="rp-info-box rp-info-blue" style={{ marginTop: 14 }}>Gewinner erhalten den Gesamtwert als positive Punkte, Verlierer als negative. Im Solo werden die Punkte des Solo-Spielers <strong>verdreifacht</strong>.</div>
            </div>
          </AccordionSection>
        )}

        {q && !(['grundlagen','trumpf','vorbehalte','soli','ansagen','sonderkarten','extrapunkte','spielwert'] as SectionId[]).some(id => SECTION_KEYWORDS[id].includes(q)) && (
          <div className="rp-no-results">Keine Regeln gefunden.</div>
        )}
      </div>
    </div>
  );
}

interface AccordionSectionProps {
  id: SectionId;
  icon: React.ReactNode;
  title: string;
  subtitle: string;
  open: boolean;
  onToggle: () => void;
  children: React.ReactNode;
}

function AccordionSection({ icon, title, subtitle, open, onToggle, children }: AccordionSectionProps) {
  return (
    <div className={`rp-section${open ? ' rp-section-open' : ''}`}>
      <div className="rp-section-header" onClick={onToggle}>
        <div className="rp-section-icon">{icon}</div>
        <div className="rp-section-title-group">
          <div className="rp-section-title">{title}</div>
          <div className="rp-section-subtitle">{subtitle}</div>
        </div>
        <svg className="rp-chevron" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </div>
      <div className="rp-section-body">
        <div className="rp-section-content">{children}</div>
      </div>
    </div>
  );
}

interface TrumpItemProps {
  rank: string;
  img: string;
  alt: string;
  tier: 'dulle' | 'dame' | 'bube' | 'karo';
  children: React.ReactNode;
}

function TrumpItem({ rank, img, alt, tier, children }: TrumpItemProps) {
  return (
    <div className={`rp-trump-item rp-trump-${tier}`}>
      <span className="rp-trump-rank">{rank}</span>
      <div className="rp-trump-card-wrap"><img src={img} alt={alt} /></div>
      <span className="rp-trump-name">{children}</span>
    </div>
  );
}
