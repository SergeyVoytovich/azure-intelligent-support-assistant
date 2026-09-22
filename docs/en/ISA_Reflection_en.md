# Project Reflection

## Azure Intelligent Support Assistant

The Azure Intelligent Support Assistant project was especially valuable to me because it brought together several areas in one project that appear much simpler when considered separately: backend and frontend development, Azure infrastructure, AI services, security, CI/CD, testing, monitoring, and cost control.

Before starting the project, I viewed many of these components mainly as separate technologies. During implementation, it became much clearer how they interact in a real cloud application and which problems arise specifically at the boundaries between services.

---

## What Worked Well

One of the most successful decisions, in my view, was the overall application architecture.

The backend was implemented with .NET 10 and Azure Functions, while the frontend uses Vue 3 and Azure Static Web Apps. AI functionality is distributed across dedicated Azure services, and the backend is responsible for orchestrating them.

This approach made it possible to clearly separate responsibilities between components and avoid moving Azure-specific logic into the frontend.

The implementation of Retrieval-Augmented Generation also worked well.

Instead of sending a question directly to GPT-4o, the application first queries Azure AI Search and retrieves relevant sections from the knowledge base. Only then is the retrieved context passed to the language model.

The use of Hybrid Search proved particularly useful. The combination of Keyword Search, Vector Search, and Semantic Search makes it possible to find both exact matches and semantically similar content.

A custom .NET-based ingestion pipeline was implemented for document processing:

```text
Blob Storage
→ Document Intelligence
→ Normalization
→ Chunking
→ Embeddings
→ Azure AI Search
```

This helped me understand the complete data lifecycle in a RAG system instead of relying only on a ready-made import wizard in the Azure Portal.

Another strong decision was automation through Infrastructure as Code.

The infrastructure was described using Bicep and split into several modules:

```text
core
ai
security
app
budget
web
```

As the project grew, this became significantly more convenient than managing all resources manually through the Azure Portal.

CI/CD also became an important part of the project.

The pipeline automatically performs:

- backend build;
- frontend build;
- Unit Tests;
- Architecture Tests;
- Frontend Tests;
- dependency checks;
- Gitleaks;
- Trivy;
- Bicep validation;
- creation of deployment artifacts;
- automatic deployment from `dev`.

As a result, changes pass through multiple validation stages before they reach Azure.

---

## Main Challenges

The biggest challenge in the project was not the C# code itself, but configuring access between Azure services.

The main architectural idea was to use Managed Identity and Azure RBAC instead of storing API Keys and Client Secrets.

Conceptually, this looks relatively simple. In practice, however, for every Azure service it is necessary to understand exactly:

- which Identity performs the request;
- which resource it accesses;
- which specific role is required;
- at which Scope that role must be assigned.

Azure AI Search configuration required particularly significant effort.

Although the RBAC roles were correctly assigned, access through Microsoft Entra ID initially did not work. The problem was not only related to the roles, but also to the configuration of the Search Service, where the AAD authentication challenge behavior had to be configured correctly.

This was an important lesson for me: when troubleshooting authorization problems, checking only Role Assignments is not enough. The calling application's configuration, Identity, RBAC, and the settings of the target service must all be checked together.

In addition, broad temporary Role Assignments were created at the Resource Group level during troubleshooting. After the application was stabilized, they were removed and the required permissions were retained only on the specific resources.

This brought the configuration closer to the Principle of Least Privilege.

---

## CI/CD and Infrastructure as Code

Another challenge was bringing already functioning Azure infrastructure back into alignment with Bicep.

Some resources and parameters were changed manually during development because problems needed to be diagnosed quickly.

Afterward, the working configuration had to be carefully transferred back into Infrastructure as Code so that a subsequent deployment would not delete or modify settings that were already working correctly.

A useful tool for this was:

```text
az deployment ... what-if
```

The What-If check before deployment made it possible to see which resources would be created, modified, or deleted.

In a future project, I would apply this approach even more strictly: after any minimal manual fix in Azure, I would immediately transfer the change into Bicep instead of postponing it until a later stage.

---

## What I Would Do Differently

If I were starting the project again, I would define the final Azure resource architecture and access model earlier.

In particular, before implementing the main business logic, I would prepare a table such as:

```text
Identity
→ Resource
→ Required Role
→ Scope
```

This would have simplified the work with Managed Identity and reduced the number of experimental RBAC changes.

I would also configure Application Insights, Log Analytics, and Alerts earlier.

In this project, full monitoring was added closer to the final stage. It works and collects real data, but an earlier setup would have made it possible to analyze telemetry throughout the entire development period.

I would also document the differences between the KQL schemas in Application Insights and the Log Analytics Workspace earlier.

In practice, the same data can have different table and field names depending on where the query is executed. This is a small technical detail, but it can consume a noticeable amount of time during diagnostics.

---

## What I Learned About Managed Identity

Before this project, Managed Identity mainly seemed to me like a way to “avoid storing passwords.”

After implementing it in practice, it became clear that its main advantage is broader.

Managed Identity together with Azure RBAC makes it possible to build an access model in which:

- the application does not need to know Azure credentials;
- credential rotation is not required;
- permissions can be assigned separately for each resource;
- access can be controlled centrally through Azure.

However, Managed Identity does not remove complexity entirely.

It shifts complexity from managing secrets to correctly managing identities and permissions.

In practice, this is a more secure model, but it requires a good understanding of Azure RBAC.

---

## What I Learned About RAG

Working on the project also helped me better understand the limitations of Retrieval-Augmented Generation.

RAG significantly improves answer quality for a specialized knowledge base, but it does not guarantee a correct answer by itself.

The quality of the result depends on several stages:

```text
Document extraction
→ Chunking
→ Embeddings
→ Retrieval
→ Ranking
→ Prompt
→ Language Model
```

An error or poor configuration at any of these stages can reduce the quality of the final answer.

One of the most important insights for me was that RAG is not simply “connecting GPT to documents,” but a complete information pipeline.

Even with good Retrieval, a language model can theoretically generate a statement that is not directly supported by the retrieved context.

For a real production scenario, I would add additional answer verification mechanisms and stricter rules for cases where the relevance of retrieved documents is low.

---

## Testing

I consider the amount of automated testing in the project a positive result.

The backend contains more than 150 .NET tests, including Unit Tests, Architecture Tests, and Integration Tests.

The frontend is also covered by automated tests using Vitest.

An especially important insight for me was the difference between Unit Tests and Integration Tests.

Mocked Unit Tests are effective for checking application business logic, but they cannot guarantee that a real Azure service is configured correctly.

For example, a Unit Test cannot fully verify:

- Azure RBAC;
- Managed Identity;
- Semantic Search configuration;
- real model deployments;
- actual Azure AI Search behavior;
- network and service configuration.

For this reason, Integration Tests and real end-to-end checks are significantly more valuable in cloud applications than in fully local applications.

---

## CI/CD and Security

Configuring Workload Identity Federation for Azure DevOps was another useful experience.

As a result, the pipeline does not require a long-lived Client Secret for Azure deployment.

Together with Managed Identity, this made it possible to build an application where both runtime and CI/CD use identity-based authentication.

The additional checks with Gitleaks, Trivy, dependency scanning, and `npm audit` also showed that security is better integrated directly into the pipeline rather than performed as a separate manual check before submission or release.

---

## Monitoring and Operations

Application Insights and Log Analytics made it possible to look at the project not only from a developer perspective but also from an operational perspective.

After configuration, it became possible to analyze real:

- Requests;
- Status Codes;
- Response Times;
- Errors;
- Dependencies.

Alerts were also configured for:

- Error Rate > 5%;
- Average Response Time > 3 seconds;
- high GPT token usage.

A Dashboard with the main metrics and current costs was also created.

This stage showed me that a completed cloud application is not just working code. It is also necessary to understand what happens to the application after deployment.

---

## Cost Control

Before this project, I usually considered cloud costs as a separate administrative topic.

Here, Budget and Cost Management became part of the technical solution itself.

A monthly Budget of:

```text
€30
```

was configured for the project, while the actual Development/Demo environment costs are monitored separately.

At the time the documentation was prepared, accumulated costs were approximately:

```text
US$0.15
```

This experience showed that infrastructure cost should be considered together with architecture, not only after implementation.

---

## Possible Future Development

For a production version, I would first add:

- separate Dev, Test, Staging, and Production environments;
- a separate Production deployment from `main`;
- user authentication;
- Private Endpoints;
- complete disabling of local API-key authentication;
- automatic synchronization and re-indexing of the knowledge base;
- integration with a real ticketing or CRM system;
- Post-Deployment Smoke Tests;
- End-to-End Tests;
- Load and Performance Tests;
- extended monitoring and Custom Metrics.

One particularly important next step would be real escalation to a human support agent instead of only returning the field:

```json
{
  "escalationRequired": true
}
```

---

## Conclusion

The main result of the project for me is not only the creation of a working AI application.

Even more important was the practical experience of building a complete Azure system where multiple aspects must be considered at the same time:

- application architecture;
- AI;
- data;
- Identity;
- RBAC;
- Infrastructure as Code;
- CI/CD;
- testing;
- security;
- monitoring;
- cost.

The practical problems with Azure RBAC, Managed Identity, and service configuration were especially valuable because they demonstrated the difference between theoretical knowledge of Azure and operating a real cloud system.

As a result of the project, I now have a much better understanding not only of individual Azure services, but also of how to design, deploy, diagnose, and operate an application consisting of multiple interconnected cloud components.
