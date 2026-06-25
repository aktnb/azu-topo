export type TopologyEdge = {
  id: string;
  source: string;
  target: string;
  type: TopologyEdgeType;
};

export type TopologyEdgeType = 'contains' | 'trigger' | 'output';
