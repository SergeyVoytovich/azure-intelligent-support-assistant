targetScope = 'subscription'

@description('Resource group whose consts should be monitored')
param resourceGroup string

@description('Email address for budget notifications')
param email string

@description('Monthly budget')
param amount int = 30

@description('Budget start date')
param start string = '2026-09-01'

@description('Budget end date')
param end string = '2027-09-01'


resource budget 'Microsoft.Consumption/budgets@2024-08-01' = {
  name: 'budget-support-assistant'
  properties: {
    category: 'Cost'
    amount: amount
    timeGrain: 'Monthly'
    timePeriod:{
      startDate: start
      endDate: end
    }
    filter:{
      dimensions:{
        name: 'ResourceGroupName'
        operator: 'In'
        values: [
          resourceGroup
        ]
      }
    }
    notifications:{
      Actual50Percent: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 50
        thresholdType: 'Actual'
        contactEmails: [
          email
        ]
        locale: 'de-de'
      }

      Actual80Percent: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 80
        thresholdType: 'Actual'
        contactEmails: [
          email
        ]
        locale: 'de-de'
      }

      Actual100Percent: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: [
          email
        ]
        locale: 'de-de'
      }
    }
  }
}

output budgetName string = budget.name
