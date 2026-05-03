## Lokale Entwicklung

### PostgreSQL (einmalig anlegen, dann nur noch starten)

```sh
# Einmalig: Container anlegen
sudo docker run -d \
  --name doko-postgres-dev \
  -e POSTGRES_DB=doko \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:17-alpine

# Danach (nach Rechnerneustart etc.):
sudo docker start doko-postgres-dev
```

### Backend + Frontend

```sh
cd ~/programming/doko/claude_website
dotnet run --project Code/backend/Doko.Api

cd ~/programming/doko/claude_website/Code/frontend
npm run dev -- --host
```

## Server – SSH

```sh
ssh root@178.104.162.39
```

Keyphrase: Providername

## Ansible Playbook ausführen

Ansible SSHt als `root` auf den Server. Dafür muss der private Key im SSH-Agent geladen sein.

```sh
# 1. SSH-Agent starten (einmalig pro Terminal-Session)
eval "$(ssh-agent -s)"

# 2. Key laden (wähle den richtigen – probiere ed25519 zuerst)
ssh-add ~/.ssh/id_ed25519
# oder:
# ssh-add ~/.ssh/id_rsa

# 3. Prüfen ob der Key geladen ist
ssh-add -l

# 4. Verbindung testen
ssh root@178.104.162.39 "echo OK"

# 5. Playbook ausführen
cd ~/programming/doko/claude_website/infrastructure
ansible-playbook -i inventory.ini playbook.yml
```

Falls Schritt 4 mit Permission denied fehlschlägt: Der öffentliche Key (`~/.ssh/id_ed25519.pub`)
muss auf dem Server in `/root/.ssh/authorized_keys` stehen.

```sh
# Key auf Server kopieren (einmalig, braucht einmalig Passwort-Login)
ssh-copy-id -i ~/.ssh/id_ed25519.pub root@178.104.162.39
```
