param location string
param environment string
param prefix string
param suffix string


var shortSuffix = take(suffix, 8)

var storageAccountName = 'storage${prefix}${environment}${shortSuffix}'
resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' = {
    name: storageAccountName
    location: location
    sku: {
        name: 'Standard_LRS'
    }
    kind: 'StorageV2'
    properties: {
        allowBlobPublicAccess: false
        minimumTlsVersion: 'TLS1_2'
        supportsHttpsTrafficOnly: true
    }
  }

var shortName = '${prefix}-${environment}-${shortSuffix}'

resource blob 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' = {
  name: 'default'
  parent: storage
}

var knowledgeContainerName = 'knowledge-${shortName}'
resource knowledgeContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-06-01' = {
  name: knowledgeContainerName
  parent: blob
  properties: {
    publicAccess: 'None'
  }
}

var searchServiceName = 'search-${shortName}'
resource search 'Microsoft.Search/searchServices@2025-05-01' = {
  name: searchServiceName
  location: location
  sku: {
      name: 'free'
  }
  properties: {
    replicaCount: 1
    hostingMode: 'Default'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

output storageAccountName string = storage.name
output searchServiceName string = search.name
