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

// Existing resousece
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


// new resources

var storageBlobDataReaderRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')
resource storageBlobREader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, runtimeIdentity.id, storageBlobDataReaderRoleId)
  scope: storage
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataReaderRoleId
  }
}

var searchIndexDataReaderRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '1407120a-92aa-4202-b7e9-c0e197c71c8f')
resource searchReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(search.id, runtimeIdentity.id, searchIndexDataReaderRoleId)
  scope: search
  properties:{
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: searchIndexDataReaderRoleId
  }
}

var openAiUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd' )
resource openAIUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, runtimeIdentity.id, openAiUserRoleId)
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: openAiUserRoleId
  }
}

var languageReaderRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7628b7b8-a8b2-4cdc-b46f-e9b35248918e' )
resource languageReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, runtimeIdentity.id, languageReaderRoleId)
  scope: foundry
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: languageReaderRoleId
  }
}

var cognitiveServicesUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a97b65f3-24c7-4388-baec-2e87135dc908')
resource documentsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, runtimeIdentity.id, cognitiveServicesUserRoleId)
  scope: documents
  properties: {
    principalId: runtimeIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: cognitiveServicesUserRoleId
  }
}

output identityName string = runtimeIdentity.name
output identityClientId string = runtimeIdentity.properties.clientId
output identityPrincipalId string = runtimeIdentity.properties.principalId
