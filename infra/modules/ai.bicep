param location string
param environment string
param prefix string
param suffix string

var shortSuffix = take(suffix, 8)
var shortName = '${prefix}-${environment}-${shortSuffix}'

var foundryName = 'ai-${shortName}'
resource foundry 'Microsoft.CognitiveServices/accounts@2026-03-01' = {
  name: 'ai-${shortName}'
  location: location
  kind: 'AIServices'
  sku:{
    name: 'S0'
  }
  identity:{
    type: 'SystemAssigned'
  }
  properties:{
    allowProjectManagement: true
    defaultProject: 'project-${shortName}'
    associatedProjects: [
      'project-${shortName}'
    ]
    customSubDomainName: foundryName
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }
}

resource project 'Microsoft.CognitiveServices/accounts/projects@2026-03-01' = {
  name: 'project-${shortName}' 
  parent: foundry
  location: location
  identity:{
    type: 'SystemAssigned'
  }
  properties:{
    displayName: 'Support Assistant ${environment}'
  }
}

resource gptModel 'Microsoft.CognitiveServices/accounts/deployments@2026-03-01' = {
  name: 'gpt-4o'
  parent: foundry
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
  properties:{
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-11-20'
    }
    raiPolicyName: 'Microsoft.DefaultV2'
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
  dependsOn: [  
    project
  ]
}

resource embeddingModel 'Microsoft.CognitiveServices/accounts/deployments@2026-03-01' = {
  name: 'text-embedding-3-small'
  parent: foundry
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
  properties:{
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: '1'
    }
    raiPolicyName: 'Microsoft.DefaultV2'
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
  dependsOn: [
    gptModel
  ]
}

var documentsName = 'doc-${shortName}'

resource documents 'Microsoft.CognitiveServices/accounts@2026-03-01' = {
  name: documentsName
  location: location
  kind: 'FormRecognizer'
  sku: {
    name: 'S0'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: documentsName
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }
}

// var languageName = 'lang-${shortName}'
// resource lang 'Microsoft.CognitiveServices/accounts@2026-03-01' = {
//   name: languageName
//   location: location
//   kind: 'TextAnalytics'
//   sku: {
//     name: 'S0'
//   }
//   identity: {
//     type: 'SystemAssigned'
//   }
//   properties: {
//     customSubDomainName: languageName
//     disableLocalAuth: true
//     publicNetworkAccess: 'Enabled'
//   }
// }

output foundryName string  = foundry.name
output foundryProjectName string = project.name
output foundryEndpoint string = foundry.properties.endpoints['AI Foundry API']
output documentIntelligenceName string  = documents.name
// output languageName string = lang.name
output langungeEndpoint string  = foundry.properties.endpoint
