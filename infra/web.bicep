targetScope = 'resourceGroup'

@description('Name of the Static Web App.')
param staticWebAppName string

@description('Static Web Apps control-plane region, independent of the Functions region.')
param location string = 'westeurope'

@description('Environment tag.')
param environment string = 'dev'

resource web 'Microsoft.Web/staticSites@2024-04-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    provider: 'DevOps'
  }
  tags: {
    project: 'Intelligent Support Assistant'
    environment: environment
    managedBy: 'bicep'
  }
}

output staticWebAppName string = web.name
output staticWebAppHostname string = web.properties.defaultHostname
