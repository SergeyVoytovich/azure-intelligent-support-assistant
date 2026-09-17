targetScope = 'subscription'

@description('Primary Azure region for the deployment.')
param location string = 'germanywestcentral'

@description('Environment name for the deployment.')
@allowed(['dev', 'test', 'prod'])
param environment string = 'dev'

var resourcerGroupName string = 'dev'
var suffix  = uniqueString(subscription().id, resourcerGroupName)
var prefix = 'isa'

resource rg 'Microsoft.Resources/resourceGroups@2024-11-01' = {
    name: 'rg-${prefix}-${environment}-${suffix}' 
    location: location
    tags: {
        project: 'Intelligent Support Assistant'
        environment: environment
        managedBy: 'bicep'
    }
}

module core 'modules/core.bicep' = {
    name: '${prefix}-core-${environment}'
    scope: rg
    params: {
        location: location
        environment: environment
        prefix: prefix
        suffix: suffix
    }
}

output resourceGroupName string = rg.name
