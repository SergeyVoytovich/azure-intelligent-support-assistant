# Azure Intelligent Support Assistant — Test Questions

This document contains a structured set of test questions for validating the assistant, generating realistic telemetry, and preparing the live demo.

---

## 1. Standard Knowledge Base Questions

Use these to verify normal RAG behavior and source attribution.

```text
What is the warranty period?

How long is the warranty for your products?

Do your products come with a warranty?

What does the warranty cover?

How does shipping work?

How long does delivery usually take?

Do you offer express shipping?

What are your shipping options?

Can I return a product?

What is your return policy?

How do refunds work?

How can I get a refund?

Tell me about your company.

What kind of company is SVoy Electronics?

What products or services does your company provide?
```

---

## 2. Same Meaning, Different Wording

Useful for demonstrating Vector Search and Semantic Search.

### Warranty

```text
What is the warranty period?

How long am I protected after buying a product?

For how many months is my purchase covered?

If my device breaks after a year, could it still be under warranty?

How long does product coverage last?

Is there a guarantee for products purchased from your company?
```

### Shipping

```text
How long does shipping take?

When should I expect my order to arrive?

What is the usual delivery time?

How many days will I have to wait for my package?

Can my order be delivered faster?
```

### Returns

```text
Can I return an item?

What should I do if I don't want the product anymore?

Is it possible to send a purchased item back?

How does your return process work?

Can I get my money back after returning a product?
```

---

## 3. Conversational Customer Questions

Use these to simulate realistic customer support requests.

```text
Hi, I bought one of your products recently. How long is it covered by warranty?

I ordered something a few days ago. When should it arrive?

I received my order, but I changed my mind. Can I send it back?

I would like to return my purchase. What do I need to do?

My product has a problem. What options do I have under the warranty?

I need my order as soon as possible. Do you have a faster shipping option?

Could you explain your refund process in simple terms?

I am considering buying from you. What warranty do you provide?
```

---

## 4. Negative Sentiment

Useful for testing Azure AI Language and escalation logic.

```text
I'm really disappointed with my order. How can I return it?

This is extremely frustrating. My product is already broken. What can I do?

I'm very unhappy with this purchase and I want my money back.

My order still hasn't arrived and I'm getting really annoyed. What are my options?

This product stopped working and I'm not happy at all. Is it covered by warranty?

I've had enough of waiting. Can I cancel or return my order?

This is unacceptable. I need help with a refund.
```

---

## 5. Multi-Topic Questions

Useful for checking retrieval from multiple knowledge-base documents.

```text
If my product arrives damaged, can I return it or use the warranty?

How long does delivery take, and what can I do if I want to return the product afterward?

Can you explain both the warranty and return policy?

What are my options if a product becomes defective shortly after delivery?

I want to buy a product but first I need to know about shipping, returns, and warranty.

If I return a faulty product, how does the refund process work?

What should I know about delivery and warranty before placing an order?
```

---

## 6. Questions Outside the Knowledge Base

Use these to verify guardrails and behavior when no relevant knowledge-base context exists.

```text
Who is the president of France?

What will the weather be tomorrow?

Can you recommend a good restaurant in Munich?

How do I install Linux?

What is the capital of Australia?

Can you write a Python program for me?

Who won the last Formula 1 race?

What is the current Bitcoin price?

Can you help me book a hotel?

What are the system requirements for GTA VI?
```

---

## 7. Absurd or Clearly Irrelevant Questions

Useful for testing robustness.

```text
Can I use the warranty to repair my car?

Can I return a pizza?

Do you sell airplanes?

Can you ship a product to the Moon?

Does your warranty cover damage caused by dragons?

Can I exchange my product for a house?
```

---

## 8. Very Short Queries

Useful for checking retrieval with minimal context.

```text
Warranty?

Refund?

Shipping?

Returns?

Delivery time?

Broken product?

Express delivery?
```

---

## 9. Typos and Imperfect English

Useful for testing semantic retrieval despite spelling mistakes.

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

## 10. Recommended 20-Question Telemetry Run

Use these before the presentation to generate realistic request telemetry for Application Insights and the Azure Dashboard.

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

## 11. Recommended 5 Questions for the Live Demo

### 1. Basic RAG

```text
What is the warranty period?
```

### 2. Semantic Search

```text
If my device breaks after a year, could it still be under warranty?
```

### 3. Multiple Topics / Sources

```text
I want to know about shipping, returns, and warranty before buying.
```

### 4. Negative Sentiment / Escalation

```text
I'm extremely disappointed. My product is broken and I want my money back. What can I do?
```

### 5. Guardrail / No Relevant Knowledge

```text
What will the weather be tomorrow?
```

These five questions demonstrate the complete path:

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
