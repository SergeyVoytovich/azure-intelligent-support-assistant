# Reflexion zum Projekt

## Azure Intelligent Support Assistant

Das Projekt Azure Intelligent Support Assistant war für mich vor allem deshalb wertvoll, weil es mehrere Bereiche in einer einzigen Arbeit zusammengeführt hat, die einzeln deutlich einfacher wirken: Backend- und Frontend-Entwicklung, Azure-Infrastruktur, AI-Dienste, Sicherheit, CI/CD, Tests, Monitoring und Kostenkontrolle.

Vor Beginn des Projekts habe ich viele dieser Komponenten eher als einzelne Technologien betrachtet. Während der Umsetzung wurde wesentlich klarer, wie sie in einer realen Cloud-Anwendung zusammenspielen und welche Probleme insbesondere an den Schnittstellen zwischen den Diensten entstehen.

---

## Was gut funktioniert hat

Eine der gelungensten Entscheidungen war aus meiner Sicht die Gesamtarchitektur der Anwendung.

Das Backend wurde mit .NET 10 und Azure Functions umgesetzt, das Frontend mit Vue 3 und Azure Static Web Apps. Die AI-Funktionalität wurde auf separate Azure-Dienste verteilt, während das Backend deren Orchestrierung übernimmt.

Dieser Ansatz hat es ermöglicht, die Verantwortlichkeiten zwischen den Komponenten klar zu trennen und Azure-spezifische Logik nicht in das Frontend zu verlagern.

Auch die Umsetzung von Retrieval-Augmented Generation hat sich bewährt.

Anstatt eine Frage direkt an GPT-4o zu senden, fragt die Anwendung zunächst Azure AI Search ab und erhält relevante Abschnitte aus der Wissensbasis. Erst danach wird der gefundene Kontext an das Sprachmodell übergeben.

Besonders nützlich war der Einsatz von Hybrid Search. Die Kombination aus Keyword Search, Vector Search und Semantic Search ermöglicht sowohl exakte Treffer als auch semantisch ähnliche Inhalte.

Für die Dokumentverarbeitung wurde eine eigene Ingestion Pipeline auf .NET-Basis implementiert:

```text
Blob Storage
→ Document Intelligence
→ Normalization
→ Chunking
→ Embeddings
→ Azure AI Search
```

Dadurch konnte ich den vollständigen Lebenszyklus der Daten in einem RAG-System besser verstehen, statt mich auf einen fertigen Import-Assistenten im Azure Portal zu beschränken.

Ein weiterer wichtiger Punkt war die Automatisierung durch Infrastructure as Code.

Die Infrastruktur wurde mit Bicep beschrieben und in mehrere Module aufgeteilt:

```text
core
ai
security
app
budget
web
```

Mit zunehmendem Projektumfang hat sich das als deutlich praktischer erwiesen, als sämtliche Ressourcen manuell über das Azure Portal zu verwalten.

Auch CI/CD wurde zu einem wichtigen Bestandteil des Projekts.

Die Pipeline führt automatisch Folgendes aus:

- Backend-Build;
- Frontend-Build;
- Unit Tests;
- Architecture Tests;
- Frontend Tests;
- Dependency Checks;
- Gitleaks;
- Trivy;
- Bicep Validation;
- Erstellung von Deployment Artifacts;
- automatisches Deployment aus `dev`.

Dadurch durchlaufen Änderungen mehrere Prüfungsstufen, bevor sie in Azure bereitgestellt werden.

---

## Größte Herausforderungen

Die größte Schwierigkeit im Projekt war nicht der C#-Code selbst, sondern die Konfiguration der Zugriffe zwischen den Azure-Diensten.

Die grundlegende Architekturidee bestand darin, Managed Identity und Azure RBAC anstelle von API Keys und Client Secrets zu verwenden.

Konzeptionell wirkt das relativ einfach. In der Praxis muss jedoch für jeden Azure-Dienst genau geklärt werden:

- welche Identity den Request ausführt;
- auf welche Ressource sie zugreift;
- welche konkrete Rolle erforderlich ist;
- auf welchem Scope diese Rolle vergeben werden muss.

Besonders viel Zeit hat die Konfiguration von Azure AI Search benötigt.

Obwohl die RBAC-Rollen korrekt zugewiesen waren, funktionierte der Zugriff über Microsoft Entra ID zunächst nicht. Die Ursache lag nicht nur bei den Rollen, sondern auch in der Konfiguration des Search Service, bei der das Verhalten des AAD Authentication Challenge korrekt eingestellt werden musste.

Dieser Fall war für mich eine wichtige Erkenntnis: Bei Autorisierungsproblemen reicht es nicht aus, nur die Role Assignments zu prüfen. Gleichzeitig müssen die Konfiguration der aufrufenden Anwendung, die Identity, RBAC und die Einstellungen des Zielservices kontrolliert werden.

Zusätzlich entstanden während der Konfiguration vorübergehend breite Role Assignments auf Resource-Group-Ebene. Nach Stabilisierung der Anwendung wurden sie entfernt und die notwendigen Berechtigungen auf die konkreten Ressourcen beschränkt.

Dadurch konnte die Konfiguration stärker an das Least-Privilege-Prinzip angenähert werden.

---

## CI/CD und Infrastructure as Code

Eine weitere Herausforderung bestand darin, die bereits funktionierende Azure-Infrastruktur wieder mit Bicep in Einklang zu bringen.

Ein Teil der Ressourcen und Parameter wurde während der Entwicklung manuell angepasst, weil Probleme schnell diagnostiziert werden mussten.

Danach musste die funktionierende Konfiguration sorgfältig zurück in Infrastructure as Code übertragen werden, damit ein späteres Deployment keine bereits korrekt funktionierenden Einstellungen löscht oder verändert.

Ein hilfreiches Werkzeug war dabei:

```text
az deployment ... what-if
```

Die What-If-Prüfung vor dem Deployment zeigte, welche Ressourcen erstellt, geändert oder gelöscht würden.

In einem zukünftigen Projekt würde ich diesen Ansatz noch konsequenter anwenden: Nach jeder minimalen manuellen Korrektur in Azure würde ich die Änderung sofort in Bicep übernehmen, anstatt sie bis zu einem späteren Arbeitsschritt aufzuschieben.

---

## Was ich anders machen würde

Wenn ich das Projekt noch einmal beginnen würde, würde ich die endgültige Azure-Ressourcenarchitektur und das Berechtigungsmodell früher festlegen.

Insbesondere würde ich noch vor der Implementierung der wichtigsten Geschäftslogik eine Tabelle vorbereiten:

```text
Identity
→ Resource
→ Required Role
→ Scope
```

Das hätte die Arbeit mit Managed Identity deutlich vereinfacht und die Anzahl experimenteller RBAC-Änderungen reduziert.

Außerdem würde ich Application Insights, Log Analytics und Alerts früher einrichten.

In diesem Projekt wurde das vollständige Monitoring erst relativ spät umgesetzt. Es funktioniert und sammelt reale Daten, aber bei einer früheren Aktivierung hätte die Telemetrie über den gesamten Entwicklungszeitraum analysiert werden können.

Darüber hinaus würde ich die Unterschiede zwischen den KQL-Schemas in Application Insights und im Log Analytics Workspace früher dokumentieren.

In der Praxis können dieselben Daten je nach Abfragekontext unterschiedliche Tabellen- und Feldnamen besitzen. Das ist nur ein kleines technisches Detail, kann bei der Diagnose aber erheblich Zeit kosten.

---

## Was ich über Managed Identity gelernt habe

Vor diesem Projekt erschien mir Managed Identity hauptsächlich als Möglichkeit, „keine Passwörter speichern zu müssen“.

Nach der praktischen Umsetzung wurde deutlich, dass der eigentliche Vorteil weiter reicht.

Managed Identity in Kombination mit Azure RBAC ermöglicht ein Zugriffsmodell, bei dem:

- die Anwendung keine Azure Credentials kennen muss;
- keine Credential Rotation erforderlich ist;
- Berechtigungen pro Ressource vergeben werden können;
- Zugriffe zentral über Azure kontrolliert werden können.

Managed Identity beseitigt die Komplexität allerdings nicht vollständig.

Sie verschiebt die Komplexität von der Verwaltung von Secrets hin zur korrekten Verwaltung von Identitäten und Berechtigungen.

In der Praxis ist dieses Modell sicherer, erfordert jedoch ein gutes Verständnis von Azure RBAC.

---

## Was ich über RAG gelernt habe

Die Arbeit an diesem Projekt hat mir auch geholfen, die Grenzen von Retrieval-Augmented Generation besser zu verstehen.

RAG verbessert die Qualität von Antworten für eine spezialisierte Wissensbasis deutlich, garantiert aber nicht automatisch eine korrekte Antwort.

Die Ergebnisqualität hängt von mehreren Stufen ab:

```text
Document extraction
→ Chunking
→ Embeddings
→ Retrieval
→ Ranking
→ Prompt
→ Language Model
```

Ein Fehler oder eine schlechte Konfiguration in einer dieser Stufen kann die Qualität der endgültigen Antwort verschlechtern.

Besonders wichtig war für mich die Erkenntnis, dass RAG nicht einfach bedeutet, „GPT mit Dokumenten zu verbinden“, sondern eine vollständige Informationspipeline darstellt.

Selbst bei gutem Retrieval kann ein Sprachmodell theoretisch Aussagen erzeugen, die nicht unmittelbar im gefundenen Kontext enthalten sind.

Für ein reales Production-Szenario würde ich daher zusätzliche Prüfmechanismen und strengere Regeln für Fälle mit geringer Relevanz der gefundenen Dokumente ergänzen.

---

## Tests

Den Umfang der automatisierten Tests bewerte ich positiv.

Das Backend enthält mehr als 150 .NET-Tests, darunter Unit Tests, Architecture Tests und Integration Tests.

Auch das Frontend ist mit Vitest automatisiert getestet.

Eine besonders wichtige Erkenntnis war für mich der Unterschied zwischen Unit Tests und Integration Tests.

Mocked Unit Tests eignen sich gut zur Prüfung der eigenen Geschäftslogik, können aber nicht garantieren, dass ein realer Azure-Dienst korrekt konfiguriert ist.

Mit einem Unit Test lassen sich beispielsweise folgende Punkte nicht vollständig überprüfen:

- Azure RBAC;
- Managed Identity;
- Semantic Search Configuration;
- reale Model Deployments;
- tatsächliches Verhalten von Azure AI Search;
- Netzwerk- und Servicekonfiguration.

Deshalb haben Integration Tests und reale End-to-End-Prüfungen bei Cloud-Anwendungen einen deutlich höheren Stellenwert als bei vollständig lokalen Anwendungen.

---

## CI/CD und Sicherheit

Die Einrichtung von Workload Identity Federation für Azure DevOps war ebenfalls eine wichtige Erfahrung.

Dadurch benötigt die Pipeline kein dauerhaft gültiges Client Secret für das Azure Deployment.

Zusammen mit Managed Identity konnte so eine Anwendung aufgebaut werden, bei der sowohl Runtime als auch CI/CD identity-based authentication verwenden.

Die zusätzlichen Prüfungen mit Gitleaks, Trivy, Dependency Scanning und `npm audit` haben ebenfalls gezeigt, dass Sicherheitsprüfungen besser direkt in die Pipeline integriert werden, statt erst vor Abgabe oder Release manuell durchgeführt zu werden.

---

## Monitoring und Betrieb

Application Insights und Log Analytics haben mir ermöglicht, das Projekt nicht nur aus Entwicklersicht, sondern auch aus Betriebssicht zu betrachten.

Nach der Einrichtung konnten reale Werte analysiert werden, darunter:

- Requests;
- Status Codes;
- Response Times;
- Errors;
- Dependencies.

Zusätzlich wurden Alerts für folgende Bedingungen eingerichtet:

- Error Rate > 5%;
- Average Response Time > 3 seconds;
- hohe GPT-Token-Nutzung.

Außerdem wurde ein Dashboard mit den wichtigsten Kennzahlen und den aktuellen Kosten erstellt.

Dieser Arbeitsschritt hat mir gezeigt, dass eine fertige Cloud-Anwendung nicht nur aus funktionierendem Code besteht. Es muss auch nachvollziehbar sein, was nach dem Deployment mit der Anwendung geschieht.

---

## Kostenkontrolle

Vor diesem Projekt habe ich Cloud-Kosten eher als separates administratives Thema betrachtet.

Hier wurden Budget und Cost Management zu einem Bestandteil der technischen Lösung.

Für das Projekt ist ein monatliches Budget von:

```text
€30
```

eingerichtet, während die tatsächlichen Kosten der Development-/Demo-Umgebung separat überwacht werden.

Zum Zeitpunkt der Dokumentation lagen die aufgelaufenen Kosten bei ungefähr:

```text
US$0.15
```

Diese Erfahrung hat gezeigt, dass Infrastrukturkosten bereits zusammen mit der Architektur berücksichtigt werden sollten und nicht erst nach deren Umsetzung.

---

## Mögliche Weiterentwicklung

Für eine Production-Version würde ich zuerst folgende Punkte ergänzen:

- getrennte Dev-, Test-, Staging- und Production-Umgebungen;
- separates Production Deployment aus `main`;
- Benutzer-Authentifizierung;
- Private Endpoints;
- vollständige Deaktivierung lokaler API-Key-Authentifizierung;
- automatische Synchronisierung und Re-Indexierung der Wissensbasis;
- Integration mit einem realen Ticket- oder CRM-System;
- Post-Deployment Smoke Tests;
- End-to-End Tests;
- Load und Performance Tests;
- erweitertes Monitoring und Custom Metrics.

Ein besonders wichtiger nächster Schritt wäre eine echte Übergabe einer Anfrage an einen menschlichen Support-Mitarbeiter, anstatt nur folgendes Feld zurückzugeben:

```json
{
  "escalationRequired": true
}
```

---

## Fazit

Das wichtigste Ergebnis des Projekts ist für mich nicht nur die Erstellung einer funktionierenden AI-Anwendung.

Noch wichtiger war die praktische Erfahrung beim Aufbau eines vollständigen Azure-Systems, bei dem gleichzeitig folgende Aspekte berücksichtigt werden müssen:

- Anwendungsarchitektur;
- AI;
- Daten;
- Identity;
- RBAC;
- Infrastructure as Code;
- CI/CD;
- Tests;
- Sicherheit;
- Monitoring;
- Kosten.

Besonders hilfreich waren die praktischen Probleme mit Azure RBAC, Managed Identity und der Servicekonfiguration, weil sie den Unterschied zwischen theoretischem Azure-Wissen und dem Betrieb eines realen Cloud-Systems deutlich gemacht haben.

Durch das Projekt verstehe ich nun nicht nur einzelne Azure-Dienste besser, sondern auch, wie eine Anwendung aus mehreren verbundenen Cloud-Komponenten entworfen, bereitgestellt, diagnostiziert und betrieben wird.
