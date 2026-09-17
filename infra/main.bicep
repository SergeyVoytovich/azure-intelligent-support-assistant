targetScope = 'subscription'

@description('Primary Azure region for the deployment.')
param location string = 'germanywestcentral'

@description('Environment name for the deployment.')
@allowed(['dev', 'test', 'prod'])
param environment string = 'dev'

@description('Email address for Azure budget notifications.')
param budgetContactEmail string

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

module ai 'modules/ai.bicep' = {
    name: '${prefix}-ai-${environment}'
    scope: rg
    params: {
        location: location
        environment: environment
        prefix: prefix
        suffix: suffix
    }
}

module budget 'modules/budget.bicep' = {
    name: '${prefix}-ai-${environment}'
    params: {
        resourceGroup: rg.name
        email: budgetContactEmail
    }
}

output resourceGroupName string = rg.name

output storageAccountName string = core.outputs.storageAccountName
output searchServiceName string = core.outputs.searchServiceName

output foundryName string = ai.outputs.foundryName
output foundryProjectName string = ai.outputs.foundryProjectName
output foundryEndpoint string = ai.outputs.foundryEndpoint
output documentIntelligenceName string = ai.outputs.documentIntelligenceName
// output languageName string = ai.outputs.languageName
output languageEndpoint string = ai.outputs.langungeEndpoint
output buddgetName string = budget.outputs.budgetName
