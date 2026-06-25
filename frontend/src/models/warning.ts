export type TopologyWarning = {
  code: TopologyWarningCode;
  message: string;
  nodeId?: string;
};

export type TopologyWarningCode =
  | 'connectionNotFound'
  | 'resourceNotFound'
  | 'metricUnavailable'
  | 'unsupportedBinding';
