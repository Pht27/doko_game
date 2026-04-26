import { useEffect, useRef, useState } from 'react';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import './RulesPage.css';

interface RulesPageProps {
  onBack: () => void;
}

type SectionId = keyof typeof t.rulesNav;

const SECTIONS: SectionId[] = [
  'overview',
  'cards',
  'parties',
  'gameplay',
  'reservations',
  'solos',
  'announcements',
  'sonderkarten',
  'extrapunkte',
  'scoring',
];

export function RulesPage({ onBack }: RulesPageProps) {
  const [activeSection, setActiveSection] = useState<SectionId>('overview');
  const contentRef = useRef<HTMLDivElement>(null);
  const navRef = useRef<HTMLDivElement>(null);
  const pillRefs = useRef<Map<SectionId, HTMLButtonElement>>(new Map());

  useEffect(() => {
    const content = contentRef.current;
    if (!content) return;

    const observers: IntersectionObserver[] = [];

    SECTIONS.forEach((id) => {
      const el = content.querySelector(`#rp-${id}`);
      if (!el) return;
      const obs = new IntersectionObserver(
        ([entry]) => {
          if (entry.isIntersecting) setActiveSection(id);
        },
        { root: content, rootMargin: '-20% 0px -70% 0px', threshold: 0 },
      );
      obs.observe(el);
      observers.push(obs);
    });

    return () => observers.forEach((o) => o.disconnect());
  }, []);

  // Keep active pill scrolled into view
  useEffect(() => {
    const pill = pillRefs.current.get(activeSection);
    pill?.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
  }, [activeSection]);

  function scrollToSection(id: SectionId) {
    const content = contentRef.current;
    if (!content) return;
    const el = content.querySelector(`#rp-${id}`);
    el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  return (
    <div className="rp-page">
      <div className="rp-header">
        <BackButton onClick={onBack} />
        <span className="rp-header-title">{t.rulesTitle}</span>
      </div>

      <div className="rp-nav" ref={navRef}>
        {SECTIONS.map((id) => (
          <button
            key={id}
            ref={(el) => { if (el) pillRefs.current.set(id, el); }}
            className={`rp-nav-pill ${activeSection === id ? 'rp-nav-pill-active' : ''}`}
            onClick={() => scrollToSection(id)}
          >
            {t.rulesNav[id]}
          </button>
        ))}
      </div>

      <div className="rp-content" ref={contentRef}>
        {/* OVERVIEW */}
        <section className="rp-section" id="rp-overview">
          <h2 className="rp-section-title">{t.rulesNav.overview}</h2>
          <p className="rp-section-text">
            Koppeldopf ist ein Stichkartenspiel für <strong>4 Spieler</strong> mit einem{' '}
            <strong>48-Karten-Deck</strong> (ein deutsches 32-Karten-Spiel, doppelt). Gespielt
            wird <strong>gegen den Uhrzeigersinn</strong>.
          </p>
          <p className="rp-section-text">
            Es gibt zwei Parteien: <strong>Re</strong> und <strong>Kontra</strong>. Re braucht{' '}
            <strong>121+ Augen</strong> um zu gewinnen. Kontra gewinnt, wenn Re das nicht
            schafft.
          </p>
          <p className="rp-section-text">
            Jede Runde besteht aus 12 Stichen. Wer mehr Augen hat, gewinnt den Spielwert als
            positive Punkte – Verlierer erhalten negative Punkte.
          </p>
        </section>

        {/* CARDS & TRUMP */}
        <section className="rp-section" id="rp-cards">
          <h2 className="rp-section-title">{t.rulesNav.cards}</h2>
          <p className="rp-section-text">
            48 Karten: je zwei Exemplare der 24 verschiedenen Karten. Farben: ♣ Kreuz, ♠ Pik,
            ♥ Herz, ♦ Karo. Ränge: 9, B (Bube), D (Dame), K (König), 10, A.
          </p>

          <h3 className="rp-subsection-title">Kartenwerte (Augen)</h3>
          <table className="rp-table">
            <thead>
              <tr>
                <th>Rang</th>
                <th>Augen</th>
              </tr>
            </thead>
            <tbody>
              <tr><td>A</td><td>11</td></tr>
              <tr><td>10</td><td>10</td></tr>
              <tr><td>K</td><td>4</td></tr>
              <tr><td>D</td><td>3</td></tr>
              <tr><td>B</td><td>2</td></tr>
              <tr><td>9</td><td>0</td></tr>
            </tbody>
          </table>
          <p className="rp-section-text">Gesamt im Deck: <strong>240 Augen</strong>.</p>

          <h3 className="rp-subsection-title">Trumpfreihenfolge (Normalspiel)</h3>
          <table className="rp-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Karte</th>
                <th>Name</th>
              </tr>
            </thead>
            <tbody>
              <tr><td>1–2</td><td>♥ 10 (×2)</td><td>Dulle / Tolle</td></tr>
              <tr><td>3–4</td><td>♣ D (×2)</td><td>Kreuz-Dame → Re-Partei</td></tr>
              <tr><td>5–6</td><td>♠ D (×2)</td><td>Pik-Dame</td></tr>
              <tr><td>7–8</td><td>♥ D (×2)</td><td>Herz-Dame</td></tr>
              <tr><td>9–10</td><td>♦ D (×2)</td><td>Karo-Dame</td></tr>
              <tr><td>11–12</td><td>♣ B (×2)</td><td>Kreuz-Bube</td></tr>
              <tr><td>13–14</td><td>♠ B (×2)</td><td>Pik-Bube</td></tr>
              <tr><td>15–16</td><td>♥ B (×2)</td><td>Herz-Bube</td></tr>
              <tr><td>17–18</td><td>♦ B (×2)</td><td>Karo-Bube</td></tr>
              <tr><td>19–20</td><td>♦ A (×2)</td><td>Fuchs</td></tr>
              <tr><td>21–22</td><td>♦ K (×2)</td><td>Karo-König</td></tr>
              <tr><td>23–24</td><td>♦ 10 (×2)</td><td>Karo-Zehn</td></tr>
              <tr><td>25–26</td><td>♦ 9 (×2)</td><td>Karo-Neun</td></tr>
            </tbody>
          </table>

          <h3 className="rp-subsection-title">Dulle-Regel</h3>
          <p className="rp-section-text">
            Standard: Die <strong>zweite Dulle schlägt die erste</strong> – außer im{' '}
            <strong>letzten Stich</strong>, wo die erste gewinnt.
          </p>

          <h3 className="rp-subsection-title">Fehlfarben (Pik, Kreuz, Herz)</h3>
          <p className="rp-section-text">
            Innerhalb einer Fehlfarbe: <strong>A &gt; 10 &gt; K &gt; 9</strong> (♥ 10 ist Trump,
            kein Herz-Fehlfarbe).
          </p>
        </section>

        {/* PARTIES */}
        <section className="rp-section" id="rp-parties">
          <h2 className="rp-section-title">{t.rulesNav.parties}</h2>
          <ul className="rp-list">
            <li>
              <strong>Re</strong>: Die zwei Spieler mit den ♣ Damen. Brauchen{' '}
              <strong>121+ Augen</strong> zum Gewinnen.
            </li>
            <li>
              <strong>Kontra</strong>: Die anderen zwei Spieler. Gewinnen, wenn Re unter 121
              bleibt (120 reicht).
            </li>
            <li>
              Partei-Zugehörigkeit ist zu Beginn <strong>geheim</strong> – nur durch Spiel oder
              Ansagen enthüllt.
            </li>
            <li>
              Beide Spieler einer Partei gewinnen oder verlieren gemeinsam und bekommen denselben
              Punktestand eingetragen.
            </li>
          </ul>
        </section>

        {/* GAMEPLAY */}
        <section className="rp-section" id="rp-gameplay">
          <h2 className="rp-section-title">{t.rulesNav.gameplay}</h2>
          <ol className="text-white/75 text-sm leading-relaxed list-decimal pl-5 mb-2 space-y-2">
            <li>
              Der Spieler <strong>rechts</strong> vom Geber führt den ersten Stich an (spielt
              eine beliebige Karte).
            </li>
            <li>
              Jeder folgende Spieler muss <strong>Farbe bekennen</strong> (Trump zu Trump,
              Fehlfarbe zur gleichen Fehlfarbe).
            </li>
            <li>Wer nicht bekennen kann, darf jede beliebige Karte spielen.</li>
            <li>
              Der <strong>höchste Trump</strong> gewinnt einen Stich mit Trump; sonst die{' '}
              <strong>höchste Karte der angesagten Farbe</strong>.
            </li>
            <li>Wer einen Stich gewinnt, <strong>führt den nächsten</strong> an.</li>
            <li>
              Nach 12 Stichen zählt jede Partei ihre <strong>Augen</strong>. Re gewinnt bei
              121+.
            </li>
          </ol>
          <p className="rp-section-text">
            Karten werden <strong>4 × 3 ausgeteilt</strong> (vier Runden zu je 3 Karten), jeder
            Spieler erhält 12 Karten. Der Geber rotiert jede Runde.
          </p>
        </section>

        {/* RESERVATIONS */}
        <section className="rp-section" id="rp-reservations">
          <h2 className="rp-section-title">{t.rulesNav.reservations}</h2>
          <p className="rp-section-text">
            Vor dem Spiel werden Vorbehalte in Anspielreihenfolge deklariert: „Vorbehalt" oder
            „Gesund". Dann werden sie in gleicher Reihenfolge aufgedeckt.
          </p>

          <h3 className="rp-subsection-title">Priorität (höchste zuerst)</h3>
          <ol className="text-white/75 text-sm leading-relaxed list-decimal pl-5 mb-3 space-y-1">
            <li>Solo</li>
            <li>Armut</li>
            <li>Schwarze Sau (nur wenn Armut abgelehnt wurde)</li>
            <li>Hochzeit</li>
            <li>Schmeißen</li>
          </ol>

          <h3 className="rp-subsection-title">Schmeißen (Neugeben)</h3>
          <p className="rp-section-text">Ein Spieler darf schmeißen bei:</p>
          <ul className="rp-list">
            <li>Gesamt-Augen &gt; 80 oder &lt; 35</li>
            <li>≤ 3 Trümpfe</li>
            <li>Höchster Trump ist ein ♦ Bube</li>
            <li>Mindestens 5 Neunen, 5 Könige, oder Neunen + Könige ≥ 8</li>
          </ul>

          <h3 className="rp-subsection-title">Armut</h3>
          <p className="rp-section-text">
            Bedingung: ≤ 3 Trümpfe (♦ Asse zählen nicht). Der reiche Partner nimmt alle Trümpfe
            des Armutspielers, prüft sie und gibt <strong>gleich viele Karten</strong> zurück.
            Armutspieler + reicher Partner = Re-Partei. Alle Sonderkarten sind in Armut{' '}
            <strong>deaktiviert</strong>.
          </p>

          <h3 className="rp-subsection-title">Schwarze Sau</h3>
          <p className="rp-section-text">
            Wird gespielt, wenn Armut abgelehnt wird. Der Spieler, der den Stich mit der zweiten{' '}
            ♠ Dame gewinnt, muss ein <strong>Solo</strong> mit seinen restlichen Karten spielen.
          </p>

          <h3 className="rp-subsection-title">Hochzeit</h3>
          <p className="rp-section-text">
            Bedingung: Spieler hält beide ♣ Damen. Er nennt eine Bedingung (erster Stich / erster
            Fehlstich / erster Trumpfstich). Wer diesen Stich gewinnt, wird Re-Partner. Wird in
            den ersten 3 Stichen kein Partner gefunden, spielt der Spieler eine{' '}
            <strong>Stille Hochzeit</strong> (Solo).
          </p>

          <h3 className="rp-subsection-title">Stille Soli</h3>
          <ul className="rp-list">
            <li>
              <strong>Stille Hochzeit</strong>: Beide ♣ Damen still spielen. Wird als Solo
              gewertet. Sonderkarten und Extrapunkte bleiben aktiv.
            </li>
            <li>
              <strong>Kontrasolo</strong>: Pflicht wenn Spieler beide ♠ Damen und beide ♠ Könige
              hält. Die ♠ Könige werden Klabautermänner (höchste Trümpfe).
            </li>
          </ul>
        </section>

        {/* SOLOS */}
        <section className="rp-section" id="rp-solos">
          <h2 className="rp-section-title">{t.rulesNav.solos}</h2>
          <p className="rp-section-text">
            In allen Soli: Sonderkarten und Extrapunkte deaktiviert (außer in Stillen Soli). Der
            Solospieler führt immer an. Punkte werden beim Solo-Spieler <strong>verdreifacht</strong>.
          </p>

          <h3 className="rp-subsection-title">Farbsolo (♣ &gt; ♠ &gt; ♥ &gt; ♦)</h3>
          <p className="rp-section-text">
            Gleiche Trumpfstruktur wie Normalspiel, aber ♦-Trump wird durch die gewählte Farbe
            ersetzt. ♥ 10 (Dulle) bleibt höchster Trump. Ziel: 121 Augen.
          </p>

          <h3 className="rp-subsection-title">Damen-Solo</h3>
          <p className="rp-section-text">
            Nur <strong>Damen</strong> sind Trump (8 Karten). Alle anderen Karten sind Fehl.
            Reihenfolge: A &gt; 10 &gt; K &gt; B &gt; 9. Ziel: 121 Augen.
          </p>

          <h3 className="rp-subsection-title">Buben-Solo</h3>
          <p className="rp-section-text">
            Nur <strong>Buben</strong> sind Trump (8 Karten). Reihenfolge: A &gt; 10 &gt; K &gt;
            D &gt; 9. Ziel: 121 Augen.
          </p>

          <h3 className="rp-subsection-title">Fleischloses (Nullo)</h3>
          <p className="rp-section-text">
            <strong>Kein Trump</strong>. Alle Karten sind Fehl. Reihenfolge: A &gt; 10 &gt; K
            &gt; D &gt; B &gt; 9. Ziel: 121 Augen.
          </p>

          <h3 className="rp-subsection-title">Knochenloses</h3>
          <p className="rp-section-text">
            Kein Trump. Reihenfolge: A &gt; K &gt; D &gt; B &gt; 10 &gt; 9 (Zehnen sind niedrig).
            Ziel: <strong>keinen Stich gewinnen</strong>. Spiel endet sofort wenn der Solospieler
            einen Stich gewinnt (Niederlage).
          </p>

          <h3 className="rp-subsection-title">Schlanker Martin</h3>
          <p className="rp-section-text">
            Normalspiel, aber keine Sonderkarten und keine Extrapunkte. Gleichstandsregel
            umgekehrt: zweite gleiche Karte schlägt die erste. Ziel:{' '}
            <strong>wenigste Stiche</strong> für den Solospieler.
          </p>
        </section>

        {/* ANNOUNCEMENTS */}
        <section className="rp-section" id="rp-announcements">
          <h2 className="rp-section-title">{t.rulesNav.announcements}</h2>
          <p className="rp-section-text">
            Ansagen erhöhen den Einsatz. Reihenfolge: Re/Kontra → Keine 90 → Keine 60 → Keine 30
            → Schwarz. Jede Ansage erhöht den Spielwert um <strong>+1</strong>.
          </p>
          <p className="rp-section-text">
            <strong>Zeitfenster:</strong> Ansagen bis vor der zweiten Karte des zweiten Stichs
            erlaubt. Jede Ansage schiebt das Fenster um <strong>einen vollen Stich</strong> vor.
          </p>

          <h3 className="rp-subsection-title">Pflichtansage</h3>
          <p className="rp-section-text">
            Ist der <strong>erste Stich ≥ 35 Augen</strong> wert, muss der Gewinner ansagen (Re
            oder Kontra). Gilt auch für den zweiten Stich wenn ebenfalls ≥ 35 Augen. Nicht in
            Soli.
          </p>

          <h3 className="rp-subsection-title">Feigheit</h3>
          <p className="rp-section-text">
            Die Gewinner-Partei verliert, wenn sie mehr als <strong>2 Ansagen</strong> unterlassen
            hat, gemessen daran wie hoch die Gegner verloren haben.
          </p>
          <ul className="rp-list">
            <li>Nichts angesagt, Verlierer hat &lt; 60: Verlierer gewinnt (+3 fehlende Ansagen)</li>
            <li>Nichts angesagt, Verlierer hat &lt; 30: Verlierer gewinnt, +2 Extrapunkte</li>
            <li>Nur „Re" angesagt, Verlierer hat &lt; 30: Verlierer gewinnt</li>
          </ul>
          <p className="rp-section-text">Gilt nicht in Soli.</p>
        </section>

        {/* SONDERKARTEN */}
        <section className="rp-section" id="rp-sonderkarten">
          <h2 className="rp-section-title">{t.rulesNav.sonderkarten}</h2>
          <p className="rp-section-text">
            Alle Sonderkarten sind in Soli und Armut <strong>deaktiviert</strong> – außer in
            Stillen Soli (Stille Hochzeit, Kontrasolo), wo sie aktiv bleiben.
          </p>

          <h3 className="rp-subsection-title">Schweinchen</h3>
          <p className="rp-section-text">
            Bedingung: Spieler hält beide ♦ Asse. Diese werden Schweinchen und rangieren{' '}
            <strong>über den Dullen</strong>. Beim Spielen des ersten Schweinchens ankündbar.
          </p>

          <h3 className="rp-subsection-title">Superschweinchen</h3>
          <p className="rp-section-text">
            Benötigt aktive Schweinchen. Bedingung: beide ♦ Zehnen auf einer Hand. Rangieren{' '}
            <strong>über den Schweinchen</strong>.
          </p>

          <h3 className="rp-subsection-title">Hyperschweinchen</h3>
          <p className="rp-section-text">
            Benötigt aktive Superschweinchen. Bedingung: beide ♦ Könige auf einer Hand. Rangieren
            über den Superschweinchen.
          </p>

          <h3 className="rp-subsection-title">Linksdrehender / Rechtsdrehender Gehängter</h3>
          <p className="rp-section-text">
            Bedingung: beide ♦ Buben. Beim ersten ♦ Buben kann „Linksdrehender Gehängter" angesagt
            werden → Spielrichtung <strong>umkehren</strong> (Uhrzeigersinn ↔ Gegenuhrzeigersinn).
            Beim zweiten ♦ Buben „Rechtsdrehender Gehängter" → erneut umkehren.
          </p>

          <h3 className="rp-subsection-title">Genscherdamen</h3>
          <p className="rp-section-text">
            Bedingung: beide ♥ Damen. Beim ersten ♥ Dame: „Genschern" ansagen →{' '}
            <strong>neuen Partner wählen</strong>. Das neue Paar wird Re-Partei. Alle bisherigen
            Ansagen verfallen; keine Feigheit.
          </p>

          <h3 className="rp-subsection-title">Gegengenscherdamen</h3>
          <p className="rp-section-text">
            Benötigt aktive Genscherdamen. Bedingung: beide ♦ Damen. Nach dem Genschern beim
            ersten ♦ Dame kontern: <strong>neuen Partner wählen</strong>. Gleiche Regeln wie
            Genschern.
          </p>

          <h3 className="rp-subsection-title">Heidmann</h3>
          <p className="rp-section-text">
            Bedingung: beide ♠ Buben. Beim ersten ♠ Buben: „Heidmann" ansagen →{' '}
            <strong>Buben rangieren über Damen</strong> in der Trumpfreihenfolge. Muss beim ersten
            ♠ Buben angesagt werden, sonst verfällt der Effekt permanent.
          </p>

          <h3 className="rp-subsection-title">Heidfrau</h3>
          <p className="rp-section-text">
            Benötigt aktiven Heidmann. Bedingung: beide ♠ Damen. Nach Heidmann-Ansage beim
            nächsten ♠ Dame: <strong>Heidmann-Effekt umkehren</strong> (Damen wieder über Buben).
          </p>

          <h3 className="rp-subsection-title">Kemmerich</h3>
          <p className="rp-section-text">
            Bedingung: beide ♥ Buben. Beim Spielen eines ♥ Buben: eine{' '}
            <strong>eigene Ansage zurückziehen</strong>. Kann nur Ansagen der eigenen Partei
            zurückziehen (Partner-Ansage nur wenn beide bereits angesagt haben). Nur eine Ansage
            insgesamt zurückziehbar.
          </p>
        </section>

        {/* EXTRAPUNKTE */}
        <section className="rp-section" id="rp-extrapunkte">
          <h2 className="rp-section-title">{t.rulesNav.extrapunkte}</h2>
          <p className="rp-section-text">
            In Soli deaktiviert (außer in Stillen Soli). Extrapunkte beider Parteien werden
            gegeneinander verrechnet.
          </p>

          <table className="rp-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Bedingung</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Doppelkopf</td>
                <td>Stich ≥ 40 Augen → +1</td>
              </tr>
              <tr>
                <td>Fuchs gefangen</td>
                <td>♦ Ass landet bei Gegenpartei → +1</td>
              </tr>
              <tr>
                <td>Karlchen</td>
                <td>♣ Bube gewinnt letzten Stich → +1 (deaktiviert bei Heidmann)</td>
              </tr>
              <tr>
                <td>Agathe</td>
                <td>♦ Dame schlägt Karlchen (♣ Bube) im letzten Stich → +1</td>
              </tr>
              <tr>
                <td>Fischauge</td>
                <td>♦ 9 gewinnt einen Stich → +1</td>
              </tr>
              <tr>
                <td>Gans gefangen</td>
                <td>Fuchs schlägt Fischauge der Gegenpartei → +1</td>
              </tr>
              <tr>
                <td>Klabautermann</td>
                <td>♠ Dame schlägt ♠ König der Gegenpartei → +1</td>
              </tr>
              <tr>
                <td>Kaffeekränzchen</td>
                <td>Stich aus 4 Damen → +1</td>
              </tr>
            </tbody>
          </table>

          <h3 className="rp-subsection-title">Stich-Gewinner-Sonderregeln</h3>
          <p className="rp-section-text">
            Diese Regeln geben <strong>keinen Extrapunkt</strong> – sie bestimmen nur, wer den
            Stich bekommt. Tiere sind: Fuchs (♦ A), Schweinchen (♦ A wenn aktiv), Fischauge
            (♦ 9 nach dem ersten Trumpfstich), Superschweinchen (♦ 10), Hyperschweinchen (♦ K).
          </p>

          <table className="rp-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Bedingung</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Festmahl</td>
                <td>
                  Stich mit ≥ 3 Tieren, zwei vom gleichen Typ: die zweite Karte der Mehrheit
                  gewinnt. Bei zwei Paaren gewinnt die letzte Karte des Stichs.
                </td>
              </tr>
              <tr>
                <td>Blutbad</td>
                <td>
                  Stich mit ≥ 3 verschiedenen Tiertypen: die einzige Nicht-Tier-Karte gewinnt.
                  Sind alle Karten Tiere, gewinnt das Fischauge.
                </td>
              </tr>
              <tr>
                <td>Meuterei</td>
                <td>
                  Ein ♠ König an Stelle 1 oder 2, danach eine ♠ Dame (Klabautermann-Fangversuch),
                  danach ein zweiter ♠ König – und die ♠ Dame wäre die höchste Karte: der zweite
                  ♠ König gewinnt stattdessen. Kein Klabautermann-Punkt wird vergeben.
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        {/* SCORING */}
        <section className="rp-section" id="rp-scoring">
          <h2 className="rp-section-title">{t.rulesNav.scoring}</h2>

          <table className="rp-table">
            <thead>
              <tr>
                <th>Komponente</th>
                <th>Punkte</th>
                <th>Bedingung</th>
              </tr>
            </thead>
            <tbody>
              <tr><td>Gewonnen</td><td>+1</td><td>Gewinner-Partei</td></tr>
              <tr><td>Gegen die Alten</td><td>+1</td><td>Kontra gewinnt</td></tr>
              <tr><td>Keine 90</td><td>+1</td><td>Verlierer-Partei &lt; 90</td></tr>
              <tr><td>Keine 60</td><td>+1</td><td>Verlierer-Partei &lt; 60</td></tr>
              <tr><td>Keine 30</td><td>+1</td><td>Verlierer-Partei &lt; 30</td></tr>
              <tr><td>Schwarz</td><td>+1</td><td>Verlierer gewinnt keinen Stich</td></tr>
              <tr><td>Pro Ansage</td><td>+1</td><td>Jede erfüllte Ansage</td></tr>
              <tr><td>Extrapunkte</td><td>±1</td><td>Werden gegeneinander verrechnet</td></tr>
            </tbody>
          </table>

          <ul className="rp-list">
            <li>
              Beide <strong>Gewinner</strong> tragen den Gesamtwert als{' '}
              <strong>positiv</strong> ein, beide Verlierer als <strong>negativ</strong>.
            </li>
            <li>
              Im Solo werden die Punkte des Solospielers beim Eintragen{' '}
              <strong>verdreifacht</strong>.
            </li>
          </ul>
        </section>
      </div>
    </div>
  );
}
