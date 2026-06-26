export type TopologyNode = {
  id: string;
  type: TopologyNodeType;
  name: string;
  property: TopologyNodeProperties;
};

export type TopologyNodeType =
  | 'function'
  | 'serviceBusQueue'
  | 'serviceBusTopic'
  | 'serviceBusSubscription';

export type TopologyNodeProperties =
  | FunctionNodeProperties
  | ServiceBusQueueNodeProperties
  | ServiceBusTopicNodeProperties
  | ServiceBusSubscriptionNodeProperties;

export type FunctionNodeProperties = {
  functionAppName: string;
  enabled: boolean;
};

export type ServiceBusQueueStatus = 'Active' | 'Disabled' | 'SendDisabled' | 'ReceiveDisabled';

export type ServiceBusQueueNodeProperties = {
  namespace: string;
  status: ServiceBusQueueStatus;
};

export type ServiceBusTopicNodeProperties = {
  namespace: string;
};

export type ServiceBusSubscriptionNodeProperties = {
  namespace: string;
  topicName: string;
};
