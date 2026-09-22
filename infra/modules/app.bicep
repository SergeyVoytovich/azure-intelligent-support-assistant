@description('Primary Azure region.')
param location string

@description('Environment name.')
param environment string

@description('Resource name prefix.')
param prefix string

@description('Unique resource suffix.')
param suffix string

@description('Existing runtime managed identity name.')
param runtimeIdentityName string

@description('Runtime managed identity client ID.')
param runtimeIdentityClientId string

@description('Existing storage account name.')
param storageAccountName string

@description('Application Insights cooonection string')
param applicationInsightsConnectionString string

@description('Azure AI Search endpoint.')
param searchEndpoint string

@description('Azure OpenAI / Foundry endpoint.')
param foundryEndpoint string

@description('Azure OpenAI Responses API endpoint.')
param foundryResponsesEndpoint string

@description('Document Intelligence endpoint.')
param documentIntelligenceEndpoint string

@description('Azure AI Language endpoint.')
param languageEndpoint string

@description('Knowledge Base blob container name.')
param knowledgeContainerName string

@description('Chat model deployment name.')
param chatDeployment string = 'gpt-4o'

@description('Embedding model deployment name.')
param embeddingDeployment string = 'text-embedding-3-small'

var functionAppName = 'func-${prefix}-${environment}-${suffix}'
var hostingPlanName = 'plan-${prefix}-${environment}-${suffix}'
var deploymentContainerName = 'deploy-${prefix}-${environment}-${take(suffix, 8)}'

resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' existing = {
  parent: storage
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-06-01' = {
  parent: blobService
  name: deploymentContainerName

  properties: {
    publicAccess: 'None'
  }
}

resource runtimeIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: runtimeIdentityName
}

resource hostingPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: hostingPlanName
  location: location
  kind: 'functionapp'

  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }

  properties: {
    reserved: true
  }

  tags: {
    project: 'Intelligent Support Assistant'
    environment: environment
    managedBy: 'bicep'
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${runtimeIdentity.id}': {}
    }
  }

  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true

    siteConfig: {
      minTlsVersion: '1.2'
    }

    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainer.name}'

          authentication: {
            type: 'UserAssignedIdentity'
            userAssignedIdentityResourceId: runtimeIdentity.id
          }
        }
      }

      scaleAndConcurrency: {
        maximumInstanceCount: 40
        instanceMemoryMB: 2048
      }

      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
  }

  resource appSettings 'config' = {
    name: 'appsettings'

    properties: {
      AzureWebJobsStorage__accountName: storageAccountName
      AzureWebJobsStorage__credential: 'managedidentity'
      AzureWebJobsStorage__clientId: runtimeIdentityClientId
      
      AZURE_CLIENT_ID: runtimeIdentityClientId
      APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsightsConnectionString

      DocumentIntelligence__Endpoint: documentIntelligenceEndpoint
      AzureSearch__Endpoint: searchEndpoint

      Foundry__Endpoint: foundryEndpoint
      Foundry__ResponsesEndpoint: foundryResponsesEndpoint
      Foundry__ChatDeployment: chatDeployment
      Foundry__EmbeddingDeployment: embeddingDeployment

      Language__Endpoint: languageEndpoint

      Storage__AccountName: storageAccountName
      Storage__KnowledgeContainer: knowledgeContainerName
    }
  }

  tags: {
    project: 'Intelligent Support Assistant'
    environment: environment
    managedBy: 'bicep'
  }
}

output functionAppName string = functionApp.name
output functionAppHostname string = functionApp.properties.defaultHostName
output deploymentContainerName string = deploymentContainer.name
