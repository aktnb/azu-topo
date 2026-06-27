param appSuffix string
param location string = resourceGroup().location

module serviceBus 'modules/serviceBus.bicep' = {
  name: 'serviceBus'
  params: {
    namespaceName: 'my-servicebus-${appSuffix}'
    location: location
  }
}

module functionApp 'modules/functionApp.bicep' = {
  name: 'functionApp'
  params: {
    appName: 'order-processor-${appSuffix}'
    location: location
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
  }
}
