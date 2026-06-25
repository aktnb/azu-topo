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
  functionName: string;
};

export type ServiceBusQueueNodeProperties = {
  namespace: string;
  queueName: string;
};

export type ServiceBusTopicNodeProperties = {
  namespace: string;
  topicName: string;
};

export type ServiceBusSubscriptionNodeProperties = {
  namespace: string;
  topicName: string;
  subscriptionName: string;
};
