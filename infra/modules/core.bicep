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

    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }

    semanticSearch: 'free'
  }
}

// ------------------------------------------------------------
// Monitoring
// ------------------------------------------------------------

var logAnalyticsWorkspaceName = 'log-${shortName}'
var applicationInsightsName = 'appi-${shortName}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: logAnalyticsWorkspaceName
  location: location

  properties: {
    retentionInDays: 30

    sku: {
      name: 'PerGB2018'
    }

    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }

    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'

  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    RetentionInDays: 30
    SamplingPercentage: 100
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}



output storageAccountName string = storage.name
output searchServiceName string = search.name

output knowledgeContainerName string = knowledgeContainer.name

output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsName string = applicationInsights.name
output logAnalyticsWorkspaceName string = logAnalytics.name
