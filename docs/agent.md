# Todo Workflow Orchestrator

Ein lokaler Python-Orchestrator, der Todo-Tasks in strukturierten Phasen mit spezialisierten KI-Agents abarbeitet.

## Ablauf

```
Research → Plan → [Freigabe] → Implement → Review → [Freigabe] → Release Notes → Finish
```

Jede Phase nutzt ein konfiguriertes Modell. An zwei Punkten wird auf Benutzerfreigabe gewartet.

---

## Setup (einmalig)

```bash
python3 -m venv .venv
.venv/bin/pip install -r .claude/requirements-orchestrator.txt
cp .env.example .env   # ANTHROPIC_API_KEY eintragen
```

---

## Verwendung

```bash
./todo <pfad-zur-todo.md> [optionen]
```

### Beispiele

```bash
# Vollständigen Workflow starten
./todo docs/todos/1_bugfixes/mein-bug.md

# Nur testen, ohne API-Calls oder Dateiänderungen
./todo docs/todos/1_bugfixes/mein-bug.md --dry-run

# Nach Unterbrechung weitermachen
./todo docs/todos/1_bugfixes/mein-bug.md --resume

# Direkt bei einer bestimmten Phase einsteigen
./todo docs/todos/1_bugfixes/mein-bug.md --step implement

# Modell für eine Phase überschreiben
./todo docs/todos/1_bugfixes/mein-bug.md --model plan=claude-opus-4-7
```

### Alle Optionen

| Option | Beschreibung |
|---|---|
| `--dry-run` | Workflow durchsimulieren ohne API-Calls oder Dateiänderungen |
| `--resume` | Abgebrochenen Workflow an letzter gespeicherter Phase fortsetzen |
| `--step <phase>` | Direkt zu einer Phase springen (`research`, `plan`, `implement`, `review`, `release_notes`, `finish`) |
| `--model PHASE=MODEL` | Modell für eine Phase überschreiben, z.B. `--model plan=claude-opus-4-7` |
| `--config <pfad>` | Alternativen Config-Pfad angeben (Standard: `.claude/todo_orchestrator_config.yaml`) |

---

## Modell-Konfiguration

Drei Wege, Priorität von hoch nach niedrig:

### 1. CLI (pro Aufruf)
```bash
./todo mein-todo.md --model research=claude-sonnet-4-6 --model plan=claude-opus-4-7
```

### 2. Umgebungsvariablen (`.env`)
```env
DEFAULT_RESEARCH_MODEL=claude-haiku-4-5-20251001
DEFAULT_PLAN_MODEL=claude-sonnet-4-6
DEFAULT_IMPLEMENT_MODEL=claude-sonnet-4-6
DEFAULT_REVIEW_MODEL=claude-haiku-4-5-20251001
DEFAULT_RELEASE_MODEL=claude-haiku-4-5-20251001
```

### 3. Config-Datei (`.claude/todo_orchestrator_config.yaml`)
```yaml
models:
  research: claude-haiku-4-5-20251001
  plan: claude-sonnet-4-6
  implement: claude-sonnet-4-6
  review: claude-haiku-4-5-20251001
  release_notes: claude-haiku-4-5-20251001
```

---

## Todo-Format

Jede Todo-Datei sollte dieses Format haben (wird vom Orchestrator befüllt):

```markdown
# Todo: <Titel>

## Status
- [ ] Research
- [ ] Plan
- [ ] Implement
- [ ] Review
- [ ] Release Notes
- [ ] Done

## Research

## Plan

## Implementation Notes

## Review

## Release Notes
```

Bestehende Todos ohne dieses Format funktionieren auch — der Orchestrator hängt fehlende Sections an.

---

## Phasen im Detail

| Phase | Modell-Tier | Was passiert |
|---|---|---|
| **Research** | cheap | Codebase durchsuchen, relevante Dateien und Patterns finden |
| **Plan** | strong | Implementierungsplan erstellen, in Todo schreiben |
| **[Freigabe]** | — | Benutzer prüft den Plan (`y/n`) |
| **Implement** | strong | Änderungen gemäß Plan umsetzen |
| **Review** | cheap | Plan vs. Implementierung vergleichen, Findings dokumentieren |
| **[Freigabe]** | — | Benutzer prüft die Implementierung (`y/n`) |
| **Release Notes** | cheap | Version bumpen, `RELEASENOTES.md` aktualisieren (auf Deutsch) |
| **Finish** | cheap | Todo nach `99_finished/` verschieben, CSharpier formatieren, committen |

---

## Berechtigungen pro Phase

Der Orchestrator setzt Berechtigungen im Code durch — das Modell kann nur das, was erlaubt ist:

| Phase | Erlaubt |
|---|---|
| Research | Dateien lesen, Todo schreiben |
| Plan | Dateien lesen, Todo schreiben |
| Implement | Dateien lesen + schreiben, Todo schreiben |
| Review | Dateien lesen, Todo schreiben |
| Release Notes | Dateien lesen + schreiben, Todo schreiben |
| Finish | Dateien verschieben, Formatter, `git add/commit/status` |

Git-Operationen (add, commit, status) sind **ausschließlich in der Finish-Phase** verfügbar. Kein push, kein merge, kein Branch-Wechsel.

---

## Versionslogik (Release Notes)

| Änderungstyp | Bump |
|---|---|
| Kleiner Fix, interne Verbesserung | Patch: `X.Y.Z → X.Y.Z+1` |
| Neue sichtbare Funktion | Minor: `X.Y.Z → X.Y+1.0` |
| Major (`X`) | **Niemals automatisch** |

---

## Dateistruktur

```
.claude/
  todo_orchestrator/       ← Python-Package (Orchestrator-Logik)
  agents/                  ← Systemrollen pro Phase (Markdown)
  prompts/                 ← Prompt-Templates pro Phase (Markdown)
  commands/                ← Claude Code Slash-Commands
  todo_orchestrator_config.yaml
  requirements-orchestrator.txt

todo                       ← Shell-Wrapper (Entry Point)
.env                       ← API-Key + optionale Modell-Overrides
.venv/                     ← Python-Virtualenv (nach Setup)
```

---

## Troubleshooting

**`.venv` nicht gefunden:**
```bash
python3 -m venv .venv && .venv/bin/pip install -r .claude/requirements-orchestrator.txt
```

**Workflow nach Absturz fortsetzen:**
```bash
./todo docs/todos/mein-todo.md --resume
```
Der Zustand wird als `.mein-todo_state.json` neben der Todo-Datei gespeichert.

**Nur eine einzelne Phase neu ausführen:**
```bash
./todo docs/todos/mein-todo.md --step review
```

**Teures Modell nur für den Plan:**
```bash
./todo docs/todos/mein-todo.md --model plan=claude-opus-4-7
```
