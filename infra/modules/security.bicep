param location string
param environment string
param prefix string
param suffix string

param storageAccount string
param searchService string
param foundryName string
param documentsName string

var shortSuffix = take(suffix, 8)
var shortName = '${prefix}-${environment}-${shortSuffix}'

resource runtimeIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'id-${shortName}'
  location: location
}

// ------------------------------------------------------------
// Existing resources
// ------------------------------------------------------------

resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccount
}

resource search 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchService
}

resource foundry 'Microsoft.CognitiveServices/accounts@2026-03-01' existing = {
  name: foundryName
}

resource documents 'Microsoft.CognitiveServices/accounts@2026-03-01' existing = {
  name: documentsName
}

// ------------------------------------------------------------
// Storage RBAC
// ------------------------------------------------------------

// Azure Functions host storage.
// Required by the official Flex Consumption managed-identity template.
var storageBlobDataOwnerRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
)

resource storageBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, runtimeIdentity.id, storageBlobDataOwnerRoleId)
  scope: storage
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataOwnerRoleId
  }
}

// Required for Flex Consumption deployment storage.
var storageBlobDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
)

resource storageBlobContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, runtimeIdentity.id, storageBlobDataContributorRoleId)
  scope: storage
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributorRoleId
  }
}

var storageQueueDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
)

resource storageQueueContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, runtimeIdentity.id, storageQueueDataContributorRoleId)
  scope: storage
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageQueueDataContributorRoleId
  }
}

var storageTableDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
)

resource storageTableContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, runtimeIdentity.id, storageTableDataContributorRoleId)
  scope: storage
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageTableDataContributorRoleId
  }
}

// ------------------------------------------------------------
// Azure AI Search RBAC
// ------------------------------------------------------------

var searchIndexDataReaderRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '1407120a-92aa-4202-b7e9-c0e197c71c8f'
)

resource searchReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(search.id, runtimeIdentity.id, searchIndexDataReaderRoleId)
  scope: search
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: searchIndexDataReaderRoleId
  }
}

// ------------------------------------------------------------
// Azure OpenAI / Foundry RBAC
// ------------------------------------------------------------

var openAiUserRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
)

resource openAIUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  // Keep v2: this assignment already exists with this deterministic name.
  name: guid(foundry.id, runtimeIdentity.id, openAiUserRoleId, 'v2')
  scope: foundry
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: openAiUserRoleId
  }
}

// ------------------------------------------------------------
// Azure AI Language RBAC
// ------------------------------------------------------------

var languageReaderRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '7628b7b8-a8b2-4cdc-b46f-e9b35248918e'
)

resource languageReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, runtimeIdentity.id, languageReaderRoleId)
  scope: foundry
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: languageReaderRoleId
  }
}

// ------------------------------------------------------------
// Document Intelligence RBAC
// ------------------------------------------------------------

var cognitiveServicesUserRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'a97b65f3-24c7-4388-baec-2e87135dc908'
)

resource documentsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  // Keep v2: this assignment already exists with this deterministic name.
  name: guid(documents.id, runtimeIdentity.id, cognitiveServicesUserRoleId, 'v2')
  scope: documents
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: cognitiveServicesUserRoleId
  }
}

// ------------------------------------------------------------
// Outputs
// ------------------------------------------------------------

output identityName string = runtimeIdentity.name
output identityClientId string = runtimeIdentity.properties.clientId
output identityPrincipalId string = runtimeIdentity.properties.principalId
