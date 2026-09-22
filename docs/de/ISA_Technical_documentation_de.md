# Technische Dokumentation

## Azure Intelligent Support Assistant

**Projekt:** Intelligenter Kunden-Support-Assistent
**Technologien:** .NET 10, Vue 3, Azure Functions, Azure AI Services, Azure DevOps
**Cloud-Plattform:** Microsoft Azure
**Umgebung:** Development / Demo
**Region der Hauptressourcen:** Germany West Central

---

## 1. Zweck und technischer Umfang

Der Azure Intelligent Support Assistant ist eine cloudbasierte Anwendung zur automatisierten Bearbeitung wiederkehrender Kundenanfragen.

Das System beantwortet Fragen nicht ausschließlich auf Basis des allgemeinen Wissens eines Sprachmodells, sondern verwendet eine unternehmensspezifische Wissensbasis. Relevante Informationen werden zunächst mit Azure AI Search ermittelt und anschließend als Kontext an GPT-4o übergeben.

Damit wird Retrieval-Augmented Generation (RAG) eingesetzt.

Die Anwendung unterstützt insbesondere folgende technische Szenarien:

- Verarbeitung natürlichsprachlicher Kundenfragen;
- Suche nach relevanten Informationen in einer Dokumenten-Wissensbasis;
- hybride Suche aus Text-, Vektor- und semantischer Suche;
- Generierung einer Antwort mit GPT-4o;
- Rückgabe der verwendeten Quelldokumente;
- Analyse von Kundenanfragen mit Azure AI Language;
- Ermittlung eines Eskalationsstatus;
- automatisiertes Deployment über Azure DevOps;
- Monitoring, Alerting und Kostenüberwachung.

Die aktuelle Implementierung ist als vollständige Development- und Demo-Lösung ausgelegt. Sie bildet keine vollständig gehärtete Produktionsplattform ab.

---

## 2. Bereitgestellte Anwendung

Das Frontend ist öffentlich über Azure Static Web Apps erreichbar:

```text
https://gentle-sand-0b9bfc703.4.azurestaticapps.net
```

Das Frontend kommuniziert mit einer Azure Function App.

Der zentrale API-Endpunkt lautet:

```http
POST /api/chat
```

Beispielanfrage:

```json
{
  "question": "What is the warranty period?"
}
```

Ein erfolgreicher Aufruf liefert beispielsweise:

```json
{
  "answer": "The warranty period for SVoy Electronics products is 24 months.",
  "sources": [
    "warranty.pdf",
    "company-overview.pdf"
  ],
  "escalationRequired": false,
  "requestId": "..."
}
```

Neben der generierten Antwort werden damit auch Quelleninformationen, der Eskalationsstatus und eine Request ID an das Frontend übertragen.

---

## 3. Lösungsarchitektur

Die Anwendung trennt Benutzeroberfläche, Backend, AI-Dienste, Wissensspeicher, Monitoring und Deployment voneinander.

```mermaid
flowchart LR
    User[Benutzer]

    SWA[Azure Static Web Apps<br/>Vue 3 / TypeScript]

    Function[Azure Functions<br/>.NET 10 Isolated]

    Language[Azure AI Language<br/>Sentiment / Key Phrases]

    Search[Azure AI Search<br/>Hybrid / Vector / Semantic]

    GPT[Azure AI Foundry<br/>GPT-4o]

    Embeddings[Azure AI Foundry<br/>text-embedding-3-small]

    Blob[Azure Blob Storage<br/>Knowledge Base]

    DI[Azure AI Document Intelligence]

    AppInsights[Application Insights]

    Logs[Log Analytics]

    DevOps[Azure DevOps<br/>CI/CD]

    User --> SWA
    SWA --> Function

    Function --> Language
    Function --> Search
    Function --> GPT

    Blob --> DI
    DI --> Embeddings
    Embeddings --> Search

    Function --> AppInsights
    AppInsights --> Logs

    DevOps --> Function
    DevOps --> SWA
```

Das Frontend enthält keine direkte AI- oder Datenzugriffslogik. Die Orchestrierung der Azure-Dienste erfolgt im .NET-Backend.

---

## 4. Backend-Architektur

Das Backend basiert auf:

- C#;
- .NET 10;
- Azure Functions;
- .NET Isolated Worker Model;
- Azure SDKs.

Der Quellcode ist in mehrere Verantwortungsbereiche aufgeteilt:

```text
SupportAssistant.Api
SupportAssistant.Application
SupportAssistant.Domain
SupportAssistant.Infrastructure
```

Zusätzlich bestehen separate Testprojekte:

```text
SupportAssistant.UnitTests
SupportAssistant.IntegrationTests
SupportAssistant.ArchitectureTests
```

Diese Trennung reduziert direkte Abhängigkeiten zwischen Geschäftslogik, Azure-spezifischer Infrastruktur und API-Schicht.

Die Azure Functions übernehmen insbesondere:

- HTTP-Verarbeitung;
- Validierung eingehender Anfragen;
- AI-Language-Aufrufe;
- Retrieval über Azure AI Search;
- Aufbau des Modellkontexts;
- GPT-Aufrufe;
- Ermittlung der Antwortquellen;
- Eskalationslogik;
- Telemetrie.

---

## 5. Verarbeitung der Wissensbasis

Die Wissensbasis besteht aus Dokumenten in Azure Blob Storage.

Die Aufbereitung erfolgt über eine eigene .NET-basierte Ingestion-Pipeline und nicht über einen manuell konfigurierten Portal-Import.

Der Ablauf lautet:

```text
Dokument
   ↓
Azure Blob Storage
   ↓
Azure AI Document Intelligence
   ↓
Textextraktion
   ↓
Normalisierung
   ↓
Chunking
   ↓
text-embedding-3-small
   ↓
Azure AI Search
```

### 5.1 Dokumentextraktion

Azure AI Document Intelligence extrahiert den Textinhalt aus den bereitgestellten Dokumenten.

Der extrahierte Text wird anschließend im Backend normalisiert.

### 5.2 Chunking

Große Dokumente werden nicht als einzelner Datensatz gespeichert.

Der Text wird in kleinere Chunks zerlegt, damit beim Retrieval nur die tatsächlich relevanten Abschnitte in den Prompt aufgenommen werden müssen.

### 5.3 Embeddings

Für jeden Chunk wird mit folgendem Modell ein Vektor erzeugt:

```text
text-embedding-3-small
```

Dieser Vektor repräsentiert den semantischen Inhalt des Textabschnitts.

### 5.4 Indexierung

Text, Vektor und Metadaten werden anschließend in Azure AI Search gespeichert.

Zu den gespeicherten Metadaten gehören unter anderem Informationen über das Quelldokument.

---

## 6. Retrieval und RAG

Bei einer Kundenfrage wird zunächst die Wissensbasis abgefragt.

Die Suchkonfiguration kombiniert mehrere Verfahren:

- klassische Textsuche;
- Vektorsuche;
- semantische Suche.

Für die Vektorsuche wird ein HNSW-basierter Index verwendet.

Zusätzlich ist eine Semantic Configuration vorhanden:

```text
semantic-config
```

Der Ablauf einer Anfrage kann vereinfacht folgendermaßen dargestellt werden:

```text
Benutzerfrage
    ↓
Azure AI Search
    ↓
Hybrid Retrieval
    ↓
relevante Dokument-Chunks
    ↓
Prompt Context
    ↓
GPT-4o
    ↓
Antwort + Quellen
```

Die Anwendung verwendet damit nicht ausschließlich Vector Search.

Durch Hybrid Search können sowohl exakte Begriffe als auch semantisch ähnliche Formulierungen berücksichtigt werden.

---

## 7. Generative AI

Für die Antwortgenerierung wird über Azure AI Foundry folgendes Modell verwendet:

```text
GPT-4o
```

Das Sprachmodell erhält:

1. die Benutzerfrage;
2. die zuvor abgerufenen relevanten Dokumentabschnitte;
3. entsprechende System- und Prompt-Anweisungen.

Dadurch wird die Antwort auf den zur Verfügung gestellten Wissenskontext ausgerichtet.

RAG reduziert das Risiko unbelegter Antworten, kann Halluzinationen eines Sprachmodells jedoch nicht technisch vollständig ausschließen.

Aus diesem Grund werden zusätzlich die für die Antwort verwendeten Quelldokumente an das Frontend zurückgegeben.

---

## 8. Analyse von Kundenanfragen

Azure AI Language wird zusätzlich zur generativen AI verwendet.

Die Anwendung kann unter anderem folgende Informationen aus einer Kundenanfrage ermitteln:

- Sentiment;
- Schlüsselphrasen.

Diese Informationen stehen der Anwendungslogik zur weiteren Bewertung einer Anfrage zur Verfügung.

Die Entscheidung, ob eine Anfrage eskaliert werden soll, wird nicht ausschließlich dem Sprachmodell überlassen.

Die Anwendung enthält dafür deterministische Logik und gibt das Ergebnis über folgendes Feld zurück:

```json
{
  "escalationRequired": true
}
```

Eine tatsächliche Integration mit einem externen Ticket- oder CRM-System ist nicht Bestandteil der aktuellen Projektversion.

---

## 9. Azure-Ressourcen

Die produktive Development-/Demo-Umgebung befindet sich in folgender Resource Group:

```text
rg-isa-dev-uxfdhanjmza7m
```

Wichtige Ressourcen:

| Ressource             | Name                           | Aufgabe                           |
| --------------------- | ------------------------------ | --------------------------------- |
| Azure Function App    | `func-isa-dev-uxfdhanjmza7m` | .NET Backend und REST API         |
| Function Plan         | `plan-isa-dev-uxfdhanjmza7m` | Flex Consumption Hosting          |
| Azure Static Web Apps | `swa-isa-dev-uxfdhanjmza7m`  | Vue-Frontend                      |
| Azure AI Search       | `search-isa-dev-uxfdhanj`    | RAG Retrieval und Index           |
| Azure AI Foundry      | `ai-isa-dev-uxfdhanj`        | Generative AI und Embeddings      |
| Foundry Project       | `project-isa-dev-uxfdhanj`   | AI-Projekt                        |
| Document Intelligence | `doc-isa-dev-uxfdhanj`       | Dokumentextraktion                |
| Storage Account       | `storageisadevuxfdhanj`      | Dokument- und Deployment-Speicher |
| Managed Identity      | `id-isa-dev-uxfdhanj`        | Runtime-Identität                |
| Application Insights  | `appi-isa-dev-uxfdhanj`      | Anwendungstelemetrie              |
| Log Analytics         | `log-isa-dev-uxfdhanj`       | Log- und Telemetrieanalyse        |

Die meisten Ressourcen befinden sich in:

```text
Germany West Central
```

Azure Static Web Apps wird in:

```text
West Europe
```

bereitgestellt.

---

## 10. Infrastructure as Code

Die Azure-Infrastruktur wird mit Bicep verwaltet.

Die wichtigsten Dateien sind:

```text
infra/
├── main.bicep
├── web.bicep
├── environments/
│   └── dev.parameters.json
└── modules/
    ├── core.bicep
    ├── ai.bicep
    ├── security.bicep
    ├── app.bicep
    └── budget.bicep
```

### `main.bicep`

Ist der zentrale Einstiegspunkt für die Subscription- und Resource-Group-Infrastruktur und orchestriert die einzelnen Module.

### `core.bicep`

Enthält zentrale Plattformkomponenten, darunter:

- Storage Account;
- Azure AI Search;
- Application Insights;
- Log Analytics.

### `ai.bicep`

Verwaltet AI-Komponenten, darunter:

- Azure AI Foundry;
- Foundry Project;
- GPT-Modellbereitstellung;
- Embedding-Modell;
- Document Intelligence.

### `security.bicep`

Verwaltet:

- User Assigned Managed Identity;
- Azure RBAC Role Assignments.

### `app.bicep`

Definiert:

- Azure Functions Hosting Plan;
- Function App;
- Runtime-Konfiguration;
- Azure-Service-Endpunkte.

### `budget.bicep`

Verwaltet:

- Azure Budget;
- Budget-Benachrichtigungen.

### `web.bicep`

Verwaltet Azure Static Web Apps getrennt von der Backend-Infrastruktur.

Diese modulare Aufteilung reduziert Kopplung und erleichtert Änderungen und Reviews der Infrastruktur.

---

## 11. Sicherheitsmodell

### 11.1 Managed Identity

Die Backend-Anwendung verwendet folgende User Assigned Managed Identity:

```text
id-isa-dev-uxfdhanj
```

Die Anwendung verwendet:

```text
DefaultAzureCredential
```

In Azure wird dabei die konfigurierte User Assigned Managed Identity verwendet.

Die Client ID wird der Function App über:

```text
AZURE_CLIENT_ID
```

zur Verfügung gestellt.

Dadurch müssen für die Runtime-Kommunikation keine Azure Client Secrets im Anwendungscode gespeichert werden.

---

## 12. Azure RBAC

Der Zugriff auf Azure-Dienste erfolgt über Azure RBAC.

Beispiele für verwendete Rollen sind:

```text
Search Index Data Reader
Search Index Data Contributor
Search Service Contributor
Cognitive Services OpenAI User
Cognitive Services Language Reader
Cognitive Services User
```

Die Rollen werden auf den jeweils benötigten Azure-Ressourcen vergeben.

Breite, zusätzlich auf Resource-Group-Ebene vergebene Runtime-Rollen wurden entfernt.

Damit folgt die Runtime-Konfiguration dem Least-Privilege-Prinzip deutlich enger als eine pauschale Berechtigung auf der gesamten Resource Group.

---

## 13. Azure AI Search Authentifizierung

Die Runtime-Anwendung verwendet Microsoft Entra ID und Azure RBAC für den Zugriff auf Azure AI Search.

Die Search-Ressource erlaubt derzeit zusätzlich weiterhin lokale API-Key-Authentifizierung.

Dies ist für die aktuelle Development-/Demo-Umgebung bewusst dokumentiert.

Eine mögliche zukünftige Härtung besteht darin, lokale Authentifizierung vollständig zu deaktivieren, sobald alle administrativen und technischen Zugriffswege ausschließlich über Entra ID erfolgen.

---

## 14. Function App CORS

Browserzugriffe auf das Backend sind auf das bereitgestellte Frontend eingeschränkt.

Erlaubter Origin:

```text
https://gentle-sand-0b9bfc703.4.azurestaticapps.net
```

Die aktuelle Konfiguration enthält keinen globalen Wildcard-Origin:

```text
*
```

Damit kann nicht jede beliebige Webseite browserseitig auf die Function API zugreifen.

---

## 15. Transportverschlüsselung

Die Function App ist für HTTPS konfiguriert.

Die Infrastruktur verwendet:

```text
HTTPS only
```

und mindestens:

```text
TLS 1.2
```

Dadurch wird unverschlüsselte HTTP-Kommunikation verhindert.

---

## 16. CI/CD und Branch-Modell

Quellcode und Pipeline-Konfiguration befinden sich im GitHub-Repository.

Azure DevOps verwendet dieses Repository als Grundlage für CI/CD.

Das Branch-Modell lautet:

```text
feature/*
    ↓
Pull Request
    ↓
dev
    ↓
automatisiertes Deployment
    ↓
main
```

Der Branch:

```text
dev
```

übernimmt im Projekt die Funktion des im Aufgabenblatt genannten `develop`-Branches.

### Pull Requests

Pull Requests auf `dev` oder `main` führen Validierungen, Builds und Tests aus.

Sie führen kein Azure Deployment aus.

### `dev`

Nach einem erfolgreichen Merge nach `dev` wird die vollständige CI/CD-Pipeline ausgeführt.

Dabei werden Backend und Frontend automatisch in die Azure Development-/Demo-Umgebung deployed.

### `main`

`main` ist der stabile Release-Branch.

Der aktuelle Pipeline-Aufbau führt dort CI-Prüfungen aus, jedoch kein automatisches Azure Deployment.

Final geprüfte Änderungen werden zum Projektabschluss von `dev` nach `main` übernommen.

---

## 17. Workload Identity Federation

Die Azure-DevOps-Pipeline verwendet folgende Azure Resource Manager Service Connection:

```text
sc-isa-dev-wif
```

Die Authentifizierung erfolgt mit:

```text
Workload Identity Federation
```

Dadurch muss für Azure DevOps kein langfristig gültiges Client Secret gespeichert werden.

Dies reduziert:

- Secret-Rotation;
- Secret-Ablauf;
- Risiko versehentlich veröffentlichter Deployment-Credentials.

---

## 18. CI-Pipeline

Die zentrale CI/CD-Pipeline befindet sich unter:

```text
pipelines/pr-ci.yml
```

Zu den ausgeführten Schritten gehören unter anderem:

1. Installation der benötigten .NET- und Node.js-Versionen;
2. Restore der .NET-Abhängigkeiten;
3. Installation der Frontend-Abhängigkeiten;
4. Dependency- und Vulnerability-Checks;
5. Gitleaks Secret Scan;
6. Trivy Scan;
7. Bicep-Validierung;
8. .NET Build;
9. Unit Tests;
10. Architecture Tests;
11. Frontend Tests;
12. Frontend Build;
13. Veröffentlichung von Test- und Coverage-Ergebnissen;
14. Erstellung der Deployment-Artefakte.

Bei einem erfolgreichen Lauf auf `dev` folgen zusätzlich:

15. Deployment der Function App;
16. Vorbereitung der Static-Web-App-Konfiguration;
17. Deployment des Vue-Frontends.

---

## 19. Sicherheitsprüfungen in CI

### Gitleaks

Gitleaks prüft das Repository auf versehentlich gespeicherte Secrets.

Dazu können beispielsweise gehören:

- API Keys;
- Tokens;
- Passwörter;
- Client Secrets.

### Trivy

Trivy führt einen Security Scan der Dateien und Abhängigkeiten durch.

### .NET Dependency Scan

Die .NET-Abhängigkeiten werden auf bekannte verwundbare Pakete geprüft.

### npm audit

Für die Frontend-Abhängigkeiten wird:

```bash
npm audit
```

ausgeführt.

Damit werden sowohl Backend- als auch Frontend-Abhängigkeiten automatisiert in die Security-Prüfung einbezogen.

---

## 20. Tests und Qualitätssicherung

Das Backend besitzt mehr als 150 automatisierte .NET-Tests.

Die Tests sind in drei Kategorien getrennt.

### Unit Tests

Unit Tests prüfen Geschäfts- und Anwendungslogik unabhängig von echten Azure-Ressourcen.

### Architecture Tests

Architecture Tests prüfen strukturelle Abhängigkeitsregeln innerhalb der .NET-Solution.

### Integration Tests

Integration Tests prüfen Szenarien mit realen Azure-Diensten.

Sie sind vom normalen Pull-Request-Lauf getrennt, damit nicht jede Codeänderung unnötig reale Azure-Ressourcen verwendet.

Test- und Coverage-Ergebnisse werden in Azure DevOps veröffentlicht.

---

## 21. Frontend-Tests

Das Vue-Frontend wird mit Vitest getestet.

Die Tests prüfen unter anderem:

- Darstellung des Chats;
- erfolgreiche API-Antworten;
- Ladezustände;
- Fehlerzustände;
- Eskalationsverhalten.

Frontend-Tests und Build sind Bestandteil der CI-Pipeline.

---

## 22. Monitoring-Konzept

Die Monitoring-Kette lautet:

```text
Azure Functions
    ↓
Application Insights
    ↓
Log Analytics
    ↓
Azure Monitor
```

Die Function App überträgt Telemetrie über:

```text
APPLICATIONINSIGHTS_CONNECTION_STRING
```

an Application Insights.

Application Insights ist Workspace-basiert mit folgendem Log Analytics Workspace verbunden:

```text
log-isa-dev-uxfdhanj
```

Gesammelt werden insbesondere:

- Requests;
- Statuscodes;
- Antwortzeiten;
- Fehler;
- Dependencies;
- weitere Anwendungstelemetrie.

---

## 23. Log Analytics und KQL

Telemetriedaten können zentral über KQL ausgewertet werden.

Beispiel für die letzten Requests:

```kusto
AppRequests
| where TimeGenerated > ago(30m)
| project TimeGenerated, Name, ResultCode, DurationMs
| order by TimeGenerated desc
```

Ein realer erfolgreicher `/api/chat`-Request wurde nach der Monitoring-Konfiguration im Log Analytics Workspace erfasst.

Damit wurde die vollständige Verbindung:

```text
Function App
→ Application Insights
→ Log Analytics
```

praktisch überprüft.

---

## 24. Azure Monitor Alerts

Für die Demo-Umgebung bestehen drei wesentliche Alert-Regeln.

### Fehlerrate

Die erste Regel überwacht den Anteil fehlgeschlagener Requests.

Schwellwert:

```text
> 5 %
```

Die Berechnung basiert auf den in Log Analytics gespeicherten Requests.

### Antwortzeit

Die zweite Regel überwacht die durchschnittliche Request-Dauer.

Schwellwert:

```text
> 3000 ms
```

### GPT-Token-Nutzung

Die dritte Regel überwacht die Azure-AI-Foundry-Metrik:

```text
TokenTransaction Count
```

für das Deployment:

```text
gpt-4o
```

Der konfigurierte Alert-Schwellwert liegt bei:

```text
8000
```

und repräsentiert ungefähr 80 % des für das Projekt angenommenen 10.000-Token-Limits.

Alle drei Regeln verwenden eine Azure Monitor Action Group zur Benachrichtigung.

---

## 25. Monitoring-Dashboard

Für das Projekt wurde ein Azure Dashboard erstellt.

Es enthält aktuell unter anderem folgende Kennzahlen:

- Server Requests;
- Successful Requests;
- Average Server Response Time;
- aktuelle Azure-Kosten.

Damit können technische Betriebsdaten und Kosten während einer Demonstration auf einer gemeinsamen Oberfläche dargestellt werden.

---

## 26. Kostenüberwachung

Die Kosten werden über Azure Cost Management auf Ebene der Projekt-Resource-Group überwacht.

Verwendeter Scope:

```text
rg-isa-dev-uxfdhanjmza7m
```

Die Analyse wird nach Azure Service gruppiert.

Zum Zeitpunkt der Erstellung dieser Dokumentation betrugen die bisher aufgelaufenen Kosten ungefähr:

```text
US$0.15
```

Davon entfielen ungefähr:

```text
Foundry Tools   ~ US$0.14
Foundry Models  ~ US$0.01
```

Die übrigen eingesetzten Dienste verursachten zu diesem Zeitpunkt keine oder nur sehr geringe sichtbare Kosten.

Diese Zahlen sind eine Momentaufnahme der Development-/Demo-Umgebung und keine Prognose für einen produktiven Betrieb.

---

## 27. Budget

Für das Projekt wurde ein monatliches Azure Budget von:

```text
€30
```

konfiguriert.

Das Budget dient nicht als technisches Ausgabenlimit, sondern als Frühwarnmechanismus für steigende Azure-Kosten.

Budget-Benachrichtigungen ermöglichen es, unerwartete Kosten während Entwicklung und Demonstration frühzeitig zu erkennen.

---

## 28. Zentrale Architekturentscheidungen

### 28.1 Serverless Backend

Für das Backend wurde Azure Functions mit Flex Consumption gewählt.

Die Anwendung verarbeitet einzelne HTTP-Anfragen und benötigt keinen permanent laufenden Anwendungsserver.

Dadurch passt ein verbrauchsorientiertes Serverless-Modell gut zum aktuellen Lastprofil.

---

### 28.2 Static Web Apps für das Frontend

Das Vue-Frontend wird als statische Anwendung gebaut.

Azure Static Web Apps stellt diese Dateien direkt bereit und erfordert keinen eigenen Webserver.

Für die aktuelle Demo-Umgebung wird der Free-Tarif verwendet.

---

### 28.3 RAG statt Fine-Tuning

Unternehmenswissen wird nicht durch Fine-Tuning dauerhaft in das Sprachmodell integriert.

Stattdessen wird RAG verwendet.

Dies hat mehrere Vorteile:

- Dokumente können unabhängig vom Modell aktualisiert werden;
- Quellen können zur Antwort zurückgegeben werden;
- kein erneutes Modelltraining bei Dokumentänderungen;
- Unternehmenswissen bleibt separat vom Basismodell gespeichert.

---

### 28.4 Eigene Ingestion-Pipeline

Die Dokumentverarbeitung wurde bewusst als eigene .NET-Implementierung realisiert.

Dadurch bleiben:

- Chunking;
- Metadaten;
- Document Intelligence;
- Embedding-Erzeugung;
- Search-Indexierung

Teil der eigenen Anwendung und sind nicht von einer ausschließlich im Azure Portal konfigurierten Importpipeline abhängig.

---

### 28.5 Hybrid Search

Ausschließliche Vektorsuche wurde nicht verwendet.

Die Kombination aus:

- Keyword Search;
- Vector Search;
- Semantic Search

ermöglicht sowohl exakte Treffer als auch semantisch ähnliche Ergebnisse.

---

### 28.6 Managed Identity

Runtime-Zugriffe auf Azure-Dienste erfolgen bevorzugt über Managed Identity und Azure RBAC.

Dadurch befinden sich keine langfristig gültigen Service-Credentials im Anwendungscode.

---

### 28.7 Workload Identity Federation

Auch für CI/CD wird auf ein langfristiges Azure Client Secret verzichtet.

Azure DevOps verwendet stattdessen Workload Identity Federation.

Damit verwenden sowohl Runtime als auch Deployment moderne identitätsbasierte Authentifizierungsmechanismen.

---

### 28.8 Infrastructure as Code

Bicep wurde verwendet, damit Infrastrukturänderungen:

- versioniert;
- reviewed;
- reproduziert;
- automatisiert deployed

werden können.

Manuelle Portal-Konfiguration wird dadurch auf die Bereiche reduziert, in denen sie für Betrieb, Monitoring oder Demonstration sinnvoll ist.

---

## 29. Bekannte Einschränkungen

Die aktuelle Lösung besitzt bewusst dokumentierte Einschränkungen.

### Nur eine Azure-Umgebung

Aktuell existiert nur eine vollständige Development-/Demo-Umgebung.

Separate:

```text
Development
Test
Staging
Production
```

Umgebungen sind nicht aufgebaut.

### `dev` als Deployment-Branch

Das automatische Deployment erfolgt aktuell aus `dev`.

`main` wird als stabiler Release-Branch verwendet, führt aber derzeit kein eigenes Production-Deployment aus.

### Azure AI Search Local Authentication

Azure AI Search erlaubt derzeit zusätzlich API-Key-Authentifizierung.

Die Anwendung selbst verwendet Managed Identity und Azure RBAC.

Eine vollständige Deaktivierung lokaler Authentifizierung wäre ein möglicher zukünftiger Hardening-Schritt.

### Netzwerkisolation

Private Endpoints und eine vollständig private Azure-Netzwerkarchitektur sind nicht Bestandteil der aktuellen Lösung.

### Kein produktives Ticketsystem

Die Anwendung kann eine notwendige Eskalation kennzeichnen.

Eine automatische Übertragung an ein CRM- oder Ticketsystem ist jedoch nicht implementiert.

### Lasttests

Umfangreiche Last-, Stress- und Performance-Tests liegen außerhalb des aktuellen Projektumfangs.

---

## 30. Mögliche Weiterentwicklung

Für eine produktive Weiterentwicklung wären insbesondere folgende Erweiterungen sinnvoll:

- separate Dev-, Test-, Staging- und Production-Umgebungen;
- vollständig getrennte Production-CD-Pipeline;
- Private Endpoints;
- Deaktivierung lokaler API-Key-Authentifizierung;
- Benutzer-Authentifizierung;
- persistente Chat-Historie;
- Integration mit Ticket- oder CRM-Systemen;
- automatische Re-Indexierung geänderter Dokumente;
- Post-Deployment Smoke Tests;
- End-to-End-Tests;
- Last- und Performance-Tests;
- zusätzliche Custom Metrics;
- erweitertes Logging;
- Azure Workbooks für detaillierte Betriebsanalysen.

---

## 31. Technische Gesamtbewertung

Die aktuelle Lösung bildet einen vollständigen Azure-basierten Entwicklungs- und Deployment-Prozess ab.

Sie verbindet:

- Vue 3 Frontend;
- .NET Azure Functions;
- Azure AI Foundry;
- Azure AI Search;
- Azure AI Language;
- Azure AI Document Intelligence;
- Azure Blob Storage;
- Retrieval-Augmented Generation;
- Hybrid Search;
- Managed Identity;
- Azure RBAC;
- Workload Identity Federation;
- Bicep;
- Azure DevOps CI/CD;
- automatisierte Tests;
- Security Scanning;
- Application Insights;
- Log Analytics;
- Azure Monitor;
- Azure Cost Management.

Die Kernfunktionen wurden Ende-zu-Ende in Azure getestet. Eine Kundenfrage kann vom bereitgestellten Frontend über die Function App verarbeitet, gegen die Wissensbasis gesucht, mit GPT-4o beantwortet und zusammen mit den verwendeten Quellen an den Benutzer zurückgegeben werden.

Die aktuelle Architektur ist damit für den Ausbildungs- und Demonstrationszweck vollständig funktionsfähig und besitzt gleichzeitig klar dokumentierte Grenzen für einen späteren produktiven Ausbau.
