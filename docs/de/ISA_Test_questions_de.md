# Azure Intelligent Support Assistant — Testfragen

Dieses Dokument enthält strukturierte Testfragen zur Prüfung des Assistenten, zur Erzeugung realistischer Telemetriedaten und zur Vorbereitung der Live-Demo.

---

## 1. Standardfragen zur Wissensbasis

Diese Fragen dienen zur Prüfung des normalen RAG-Verhaltens und der Quellenanzeige.

```text
Wie lange ist die Garantiezeit?

Wie lange gilt die Garantie für Ihre Produkte?

Haben Ihre Produkte eine Garantie?

Was deckt die Garantie ab?

Wie funktioniert der Versand?

Wie lange dauert die Lieferung normalerweise?

Bieten Sie Expressversand an?

Welche Versandmöglichkeiten bieten Sie an?

Kann ich ein Produkt zurückgeben?

Wie lautet Ihre Rückgaberichtlinie?

Wie funktioniert die Rückerstattung?

Wie kann ich eine Rückerstattung erhalten?

Erzählen Sie mir etwas über Ihr Unternehmen.

Was für ein Unternehmen ist SVoy Electronics?

Welche Produkte oder Dienstleistungen bietet Ihr Unternehmen an?
```

---

## 2. Gleiche Bedeutung, unterschiedliche Formulierungen

Nützlich zur Demonstration von Vector Search und Semantic Search.

### Garantie

```text
Wie lange ist die Garantiezeit?

Wie lange bin ich nach dem Kauf eines Produkts abgesichert?

Für wie viele Monate ist mein Kauf durch die Garantie abgedeckt?

Wenn mein Gerät nach einem Jahr kaputtgeht, könnte es noch unter Garantie stehen?

Wie lange gilt der Garantieschutz für das Produkt?

Gibt es eine Garantie auf Produkte, die bei Ihrem Unternehmen gekauft wurden?
```

### Versand

```text
Wie lange dauert der Versand?

Wann sollte meine Bestellung ungefähr ankommen?

Wie lange dauert die Lieferung üblicherweise?

Wie viele Tage muss ich auf mein Paket warten?

Kann meine Bestellung schneller geliefert werden?
```

### Rückgabe

```text
Kann ich einen Artikel zurückgeben?

Was soll ich tun, wenn ich das Produkt nicht mehr möchte?

Kann ich einen gekauften Artikel zurückschicken?

Wie funktioniert Ihr Rückgabeprozess?

Kann ich mein Geld zurückbekommen, nachdem ich ein Produkt zurückgegeben habe?
```

---

## 3. Gesprächsnahe Kundenfragen

Diese Fragen simulieren realistische Support-Anfragen.

```text
Hallo, ich habe vor Kurzem eines Ihrer Produkte gekauft. Wie lange gilt dafür die Garantie?

Ich habe vor ein paar Tagen etwas bestellt. Wann sollte es ankommen?

Ich habe meine Bestellung erhalten, aber meine Meinung geändert. Kann ich sie zurückschicken?

Ich möchte meinen Kauf zurückgeben. Was muss ich dafür tun?

Mein Produkt hat ein Problem. Welche Möglichkeiten habe ich im Rahmen der Garantie?

Ich brauche meine Bestellung so schnell wie möglich. Gibt es eine schnellere Versandoption?

Können Sie mir den Rückerstattungsprozess einfach erklären?

Ich überlege, bei Ihnen zu kaufen. Welche Garantie bieten Sie an?
```

---

## 4. Negatives Sentiment

Nützlich zur Prüfung von Azure AI Language und der Eskalationslogik.

```text
Ich bin wirklich enttäuscht von meiner Bestellung. Wie kann ich sie zurückgeben?

Das ist extrem frustrierend. Mein Produkt ist bereits kaputt. Was kann ich tun?

Ich bin mit diesem Kauf sehr unzufrieden und möchte mein Geld zurück.

Meine Bestellung ist immer noch nicht angekommen und ich werde langsam wirklich verärgert. Welche Möglichkeiten habe ich?

Dieses Produkt funktioniert nicht mehr und ich bin überhaupt nicht zufrieden. Fällt das unter die Garantie?

Ich habe genug vom Warten. Kann ich meine Bestellung stornieren oder zurückgeben?

Das ist nicht akzeptabel. Ich brauche Hilfe bei einer Rückerstattung.
```

---

## 5. Fragen zu mehreren Themen

Nützlich zur Prüfung des Retrievals aus mehreren Dokumenten der Wissensbasis.

```text
Wenn mein Produkt beschädigt ankommt, kann ich es zurückgeben oder die Garantie nutzen?

Wie lange dauert die Lieferung und was kann ich tun, wenn ich das Produkt anschließend zurückgeben möchte?

Können Sie mir sowohl die Garantie- als auch die Rückgaberegeln erklären?

Welche Möglichkeiten habe ich, wenn ein Produkt kurz nach der Lieferung defekt wird?

Ich möchte ein Produkt kaufen, aber vorher etwas über Versand, Rückgabe und Garantie wissen.

Wenn ich ein defektes Produkt zurückgebe, wie funktioniert dann die Rückerstattung?

Was sollte ich vor einer Bestellung über Lieferung und Garantie wissen?
```

---

## 6. Fragen außerhalb der Wissensbasis

Diese Fragen dienen zur Prüfung von Guardrails und des Verhaltens, wenn kein relevanter Kontext in der Wissensbasis vorhanden ist.

```text
Wer ist der Präsident von Frankreich?

Wie wird das Wetter morgen?

Können Sie ein gutes Restaurant in München empfehlen?

Wie installiere ich Linux?

Was ist die Hauptstadt von Australien?

Können Sie ein Python-Programm für mich schreiben?

Wer hat das letzte Formel-1-Rennen gewonnen?

Wie hoch ist der aktuelle Bitcoin-Preis?

Können Sie mir helfen, ein Hotel zu buchen?

Welche Systemanforderungen hat GTA VI?
```

---

## 7. Absurde oder eindeutig irrelevante Fragen

Nützlich zur Prüfung der Robustheit.

```text
Kann ich die Garantie nutzen, um mein Auto reparieren zu lassen?

Kann ich eine Pizza zurückgeben?

Verkaufen Sie Flugzeuge?

Können Sie ein Produkt zum Mond liefern?

Deckt Ihre Garantie Schäden durch Drachen ab?

Kann ich mein Produkt gegen ein Haus eintauschen?
```

---

## 8. Sehr kurze Anfragen

Nützlich zur Prüfung des Retrievals mit minimalem Kontext.

```text
Garantie?

Rückerstattung?

Versand?

Rückgabe?

Lieferzeit?

Defektes Produkt?

Expressversand?
```

---

## 9. Tippfehler und unvollkommenes Englisch

Nützlich zur Prüfung von Semantic Retrieval trotz Schreibfehlern.

```text
How long is waranty?

Can I retrun my product?

How long dose shipping take?

How can I get refound?

My produt is broken what can I do?

Can i send item bak?

What is warrenty period?
```

---

## 10. Empfohlener Lauf mit 20 Fragen für Telemetrie

Diese Fragen sollten vor der Präsentation verwendet werden, um realistische Request-Telemetrie für Application Insights und das Azure Dashboard zu erzeugen.

```text
What is the warranty period?

How long am I protected after buying a product?

For how many months is my purchase covered?

How long does delivery usually take?

When should I expect my order to arrive?

Do you offer express shipping?

Can I return a product?

What should I do if I don't want the product anymore?

How do refunds work?

Can you explain both the warranty and return policy?

If my product arrives damaged, can I return it or use the warranty?

I want to know about shipping, returns, and warranty before buying.

I'm really disappointed with my order. How can I return it?

This product stopped working and I'm not happy at all. Is it covered by warranty?

My order still hasn't arrived and I'm getting really annoyed. What are my options?

How long is waranty?

Can I retrun my product?

What will the weather be tomorrow?

Can you recommend a restaurant in Munich?

Can you write a Python program for me?
```

---

## 11. Empfohlene 5 Fragen für die Live-Demo

### 1. Einfaches RAG

```text
What is the warranty period?
```

### 2. Semantic Search

```text
If my device breaks after a year, could it still be under warranty?
```

### 3. Mehrere Themen / Quellen

```text
I want to know about shipping, returns, and warranty before buying.
```

### 4. Negatives Sentiment / Eskalation

```text
I'm extremely disappointed. My product is broken and I want my money back. What can I do?
```

### 5. Guardrail / keine relevanten Informationen

```text
What will the weather be tomorrow?
```

Diese fünf Fragen zeigen die gesamte Verarbeitungskette:

```text
Frontend
→ Azure Function
→ Azure AI Language
→ Azure AI Search
→ RAG
→ GPT-4o
→ Sources
→ Escalation
→ Application Insights
→ Azure Dashboard
```
